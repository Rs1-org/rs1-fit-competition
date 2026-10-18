import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, forkJoin, map, of, startWith, switchMap } from 'rxjs';
import { MatchEventService } from '../../core/services/match-event.service';
import { EventContext, EventInput, MatchEvent } from '../../core/models/match-event';
import { EventMatchPicker } from '../event-match-picker/event-match-picker';
import { apiErrorMessage } from '../../core/utils/api-error';

const publicId = [Validators.required, Validators.pattern(/^[0-9a-f]{32}$/)];
@Component({ selector: 'app-match-event-form', imports: [ReactiveFormsModule, RouterLink, EventMatchPicker],
  templateUrl: './match-event-form.html', styleUrl: '../../pages/match-events/events.css' })
export class MatchEventForm {
  private readonly service = inject(MatchEventService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  readonly editId = signal('');
  readonly loading = signal(true);
  readonly loadError = signal('');
  readonly error = signal('');
  readonly fieldErrors = signal<string[]>([]);
  readonly busy = signal(false);
  readonly context = signal<EventContext | null>(null);
  readonly contextLoading = signal(false);
  readonly contextError = signal('');
  readonly lookups = signal<import('../../core/models/match-event').EventLookups | null>(null);
  private readonly reload$ = new BehaviorSubject(0);
  private readonly selectedMatch$ = new BehaviorSubject('');
  readonly form = this.fb.nonNullable.group({ matchPublicId: ['', publicId], eventTypePublicId: ['', publicId],
    matchPartPublicId: ['', publicId], selectionPublicId: ['', publicId], playerPublicId: [''], relatedPlayerPublicId: [''],
    minute: [0, [Validators.required, Validators.min(0), Validators.max(130), Validators.pattern(/^\d+$/)]],
    addedMinute: [0, [Validators.required, Validators.min(0), Validators.max(30), Validators.pattern(/^\d+$/)]],
    note: ['', Validators.maxLength(500)],
  });
  readonly selectedTeam = toSignal(this.form.controls.selectionPublicId.valueChanges, { initialValue: '' });
  readonly selectedType = toSignal(this.form.controls.eventTypePublicId.valueChanges, { initialValue: '' });
  readonly substitution = computed(() => this.lookups()?.eventTypes.find(x => x.publicId === this.selectedType())?.code === 'substitution');
  readonly players = computed(() => this.context()?.selections.find(x => x.publicId === this.selectedTeam())?.players ?? []);
  constructor() {
    combineLatest([this.route.paramMap, this.route.queryParamMap, this.reload$]).pipe(
      switchMap(([params, query]) => {
        this.loading.set(true); this.loadError.set(''); this.context.set(null);
        const id = params.get('id') ?? 'new'; this.editId.set(id === 'new' ? '' : id);
        return forkJoin({ lookups: this.service.lookups(), event: this.editId() ? this.service.get(id) : of(null) }).pipe(
          map(result => ({ ...result, match: query.get('match') ?? '' })),
          catchError(err => { this.loadError.set(apiErrorMessage(err, 'The event form could not be loaded.')); this.loading.set(false); return of(null); }),
        );
      }), takeUntilDestroyed(),
    ).subscribe(result => {
      if (!result) return;
      this.lookups.set(result.lookups);
      if (result.event) this.patch(result.event);
      else this.form.reset({ matchPublicId: result.match, minute: 0, addedMinute: 0 });
      this.loading.set(false); this.selectedMatch$.next(this.form.controls.matchPublicId.value);
    });
    this.selectedMatch$.pipe(switchMap(id => {
      this.contextError.set(''); this.context.set(null); this.contextLoading.set(!!id);
      return id ? this.service.context(id).pipe(map(data => ({ data, error: '' })),
        catchError(err => of({ data: null, error: apiErrorMessage(err, 'Match teams and players could not be loaded.') }))) : of({ data: null, error: '' });
    }), takeUntilDestroyed()).subscribe(result => { this.context.set(result.data); this.contextError.set(result.error); this.contextLoading.set(false); });
    this.form.controls.selectionPublicId.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => {
      this.form.controls.playerPublicId.setValue(''); this.form.controls.relatedPlayerPublicId.setValue('');
    });
    this.form.controls.eventTypePublicId.valueChanges.pipe(takeUntilDestroyed()).subscribe(id => {
      if (this.lookups()?.eventTypes.find(x => x.publicId === id)?.code !== 'substitution') this.form.controls.relatedPlayerPublicId.setValue('');
    });
  }
  private patch(event: MatchEvent): void {
    this.form.reset({ matchPublicId: event.matchPublicId, eventTypePublicId: event.eventType.publicId,
      matchPartPublicId: event.matchPart.publicId, selectionPublicId: event.selection.publicId,
      playerPublicId: event.player?.publicId ?? '', relatedPlayerPublicId: event.relatedPlayer?.publicId ?? '',
      minute: event.minute, addedMinute: event.addedMinute, note: event.note ?? '' });
    // Dependent controls reset on selection changes; restore saved players after the reset emissions.
    this.form.patchValue({ playerPublicId: event.player?.publicId ?? '', relatedPlayerPublicId: event.relatedPlayer?.publicId ?? '' });
  }
  chooseMatch(id: string): void {
    if (this.busy() || this.editId()) return;
    this.form.patchValue({ matchPublicId: id, selectionPublicId: '', playerPublicId: '', relatedPlayerPublicId: '' });
    this.selectedMatch$.next(id);
  }
  retry(): void { this.reload$.next(this.reload$.value + 1); }
  retryContext(): void { this.selectedMatch$.next(this.form.controls.matchPublicId.value); }
  save(): void {
    if (this.busy() || this.loading() || this.loadError() || this.contextLoading() || !this.context()) return;
    this.form.markAllAsTouched(); this.error.set(''); this.fieldErrors.set([]);
    if (this.form.invalid) { this.error.set('Complete the required fields. Use whole minutes from 0 to 130, added time from 0 to 30 and a note of at most 500 characters.'); return; }
    const value = this.form.getRawValue();
    if (this.substitution() && (!value.playerPublicId || !value.relatedPlayerPublicId || value.playerPublicId === value.relatedPlayerPublicId)) {
      this.error.set('A substitution needs two different players: player off and player on.'); return;
    }
    const input: EventInput = { ...value, playerPublicId: value.playerPublicId || null,
      relatedPlayerPublicId: this.substitution() ? value.relatedPlayerPublicId || null : null, note: value.note.trim() || null };
    this.busy.set(true);
    (this.editId() ? this.service.update(this.editId(), input) : this.service.create(input)).subscribe({
      next: () => { this.busy.set(false); void this.router.navigate(['/match-events', input.matchPublicId]); },
      error: err => { this.busy.set(false); this.error.set(apiErrorMessage(err, 'The event could not be saved.'));
        const errors = err.error?.errors;
        if (Array.isArray(errors)) this.fieldErrors.set(errors.map((e: { errorMessage: string }) => e.errorMessage));
      },
    });
  }
}
