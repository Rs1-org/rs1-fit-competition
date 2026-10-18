import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, map, of, startWith, switchMap } from 'rxjs';
import { EventFilter, MatchEvent } from '../../core/models/match-event';
import { PagedResult } from '../../core/models/paged-result';
import { MatchEventService } from '../../core/services/match-event.service';
import { UserStateService } from '../../core/services/user-state.service';
import { MatchEventTimeline } from '../../components/match-event-timeline/match-event-timeline';
import { EventMatchPicker } from '../../components/event-match-picker/event-match-picker';
import { Pagination } from '../../shared/components/pagination/pagination';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { apiErrorMessage } from '../../core/utils/api-error';
import { MatchLiveSummary, MatchRefreshReason } from '../../components/match-live-summary/match-live-summary';
import { MatchPolls } from '../../components/match-polls/match-polls';

interface EventsState { loading: boolean; data: PagedResult<MatchEvent> | null; error: string; }
@Component({ selector: 'app-match-events', imports: [ReactiveFormsModule, RouterLink, EventMatchPicker, MatchEventTimeline, Pagination, MatchLiveSummary, MatchPolls],
  templateUrl: './match-events.html', styleUrl: './events.css' })
export class MatchEvents {
  private readonly service = inject(MatchEventService);
  private readonly route = inject(ActivatedRoute);
  private readonly confirm = inject(ConfirmDialogService);
  readonly canManage = inject(UserStateService).canManage;
  private readonly fb = inject(FormBuilder);
  readonly filters = this.fb.nonNullable.group({ eventTypePublicId: [''], selectionPublicId: [''], playerPublicId: [''],
    minuteFrom: this.fb.control<number | null>(null, [Validators.min(0), Validators.max(130), Validators.pattern(/^\d+$/)]),
    minuteTo: this.fb.control<number | null>(null, [Validators.min(0), Validators.max(130), Validators.pattern(/^\d+$/)]),
  });
  readonly matchId = signal('');
  readonly fixedMatch = signal(false);
  readonly actionError = signal('');
  readonly busy = signal(false);
  readonly filterError = signal('');
  private readonly match$ = new BehaviorSubject('');
  private readonly query$ = new BehaviorSubject<{ filter: EventFilter; page: number }>({ filter: {}, page: 1 });
  private readonly retry$ = new BehaviorSubject(0);
  readonly lookups = toSignal(this.retry$.pipe(switchMap(() => this.service.lookups().pipe(
    map(data => ({ data, error: '' })), catchError(err => of({ data: null, error: apiErrorMessage(err, 'Event types could not be loaded.') })),
  ))), { initialValue: { data: null, error: '' } });
  readonly context = toSignal(combineLatest([this.match$, this.retry$]).pipe(switchMap(([id]) => id ? this.service.context(id).pipe(
    map(data => ({ data, loading: false, error: '' })), startWith({ data: null, loading: true, error: '' }),
    catchError(err => of({ data: null, loading: false, error: apiErrorMessage(err, 'Match could not be loaded.') })),
  ) : of({ data: null, loading: false, error: '' }))), { initialValue: { data: null, loading: false, error: '' } });
  readonly selectionId = toSignal(this.filters.controls.selectionPublicId.valueChanges, { initialValue: '' });
  readonly players = computed(() => this.context().data?.selections
    .filter(s => !this.selectionId() || s.publicId === this.selectionId()).flatMap(s => s.players)
    .filter((p, index, all) => all.findIndex(x => x.publicId === p.publicId) === index) ?? []);
  readonly state = toSignal(this.query$.pipe(switchMap(query => this.service.getPage(query.filter, query.page).pipe(
    switchMap(data => data.page > 1 && !data.items.length
      ? this.service.getPage(query.filter, Math.max(1, data.totalPages)) : of(data)),
    map((data): EventsState => ({ loading: false, data, error: '' })),
    startWith({ loading: true, data: null, error: '' } as EventsState),
    catchError(err => of({ loading: false, data: null, error: apiErrorMessage(err, 'Events could not be loaded.') } as EventsState)),
  ))), { initialValue: { loading: true, data: null, error: '' } as EventsState });
  constructor() {
    this.route.paramMap.pipe(takeUntilDestroyed()).subscribe(params => {
      const id = params.get('publicId') ?? '';
      this.fixedMatch.set(!!id); this.chooseMatch(id);
    });
    this.filters.controls.selectionPublicId.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => this.filters.controls.playerPublicId.setValue(''));
  }
  chooseMatch(id: string): void { this.matchId.set(id); this.match$.next(id); this.filters.reset(); this.apply(); }
  apply(): void {
    this.filters.markAllAsTouched();
    const value = this.filters.getRawValue();
    if (this.filters.invalid || value.minuteFrom !== null && value.minuteTo !== null && value.minuteFrom > value.minuteTo) {
      this.filterError.set('Use whole minutes from 0 to 130, with the end at or after the start.'); return;
    }
    this.filterError.set('');
    this.query$.next({ page: 1, filter: { ...value, matchPublicId: this.matchId(),
      eventTypePublicId: value.eventTypePublicId || undefined, selectionPublicId: value.selectionPublicId || undefined, playerPublicId: value.playerPublicId || undefined } });
  }
  page(page: number): void { this.query$.next({ ...this.query$.value, page }); }
  retry(): void { this.page(this.query$.value.page); this.retry$.next(this.retry$.value + 1); }
  liveChanged(reason: MatchRefreshReason): void {
    this.page(this.state().data?.page ?? this.query$.value.page);
    if (reason !== 'events') this.match$.next(this.matchId());
  }
  reset(): void { this.filters.reset(); if (!this.fixedMatch()) this.chooseMatch(''); else this.apply(); }
  async remove(event: MatchEvent): Promise<void> {
    if (!this.canManage() || this.busy()) return;
    this.busy.set(true); this.actionError.set('');
    if (!await this.confirm.confirm({ title: 'Delete event', message: `Delete ${event.eventType.name} for ${event.selection.name} in ${event.matchPart.name}?`, confirmLabel: 'Delete event' })) { this.busy.set(false); return; }
    this.service.delete(event.publicId).subscribe({ next: () => { this.busy.set(false); this.page(1); },
      error: err => { this.busy.set(false); this.actionError.set(apiErrorMessage(err, 'The event could not be deleted.')); } });
  }
}
