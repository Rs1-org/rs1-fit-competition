import { Component, DestroyRef, inject, Input, OnChanges, signal, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, distinctUntilChanged, filter, map, of, startWith, switchMap, tap } from 'rxjs';
import { Poll } from '../../core/models/poll';
import { PagedResult } from '../../core/models/paged-result';
import { PollService } from '../../core/services/poll.service';
import { MatchLiveService } from '../../core/services/match-live.service';
import { RealtimeState } from '../../core/services/signalr.service';
import { UserStateService } from '../../core/services/user-state.service';
import { AuthModalComponent } from '../../features/auth/auth-modal/auth-modal.component';
import { Pagination } from '../../shared/components/pagination/pagination';
import { AppDateTimePipe, AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { apiErrorMessage } from '../../core/utils/api-error';

interface PollState { loading: boolean; data: PagedResult<Poll> | null; error: string; }
@Component({ selector: 'app-match-polls', imports: [ReactiveFormsModule, RouterLink, AuthModalComponent, Pagination, AppDateTimePipe, AppNumberPipe],
  templateUrl: './match-polls.html', styleUrl: './polls.css' })
export class MatchPolls implements OnChanges {
  @Input({ required: true }) matchPublicId = '';
  @ViewChild(AuthModalComponent) authModal?: AuthModalComponent;
  private readonly service = inject(PollService);
  private readonly live = inject(MatchLiveService);
  private readonly user = inject(UserStateService);
  private readonly fb = inject(FormBuilder);
  private readonly query$ = new BehaviorSubject({ id: '', page: 1 });
  private readonly match$ = new BehaviorSubject('');
  private readonly displayed = signal<PagedResult<Poll> | null>(null);
  private readonly forms = new Map<string, FormGroup<{ optionPublicId: FormControl<string> }>>();
  private boundaryTimer: ReturnType<typeof setTimeout> | undefined;
  readonly viewer = toSignal(this.user.user$, { initialValue: null });
  readonly canManage = this.user.canManage;
  readonly connection = signal<RealtimeState>('connecting');
  readonly busy = signal<string | null>(null);
  readonly errors = signal<Record<string, string>>({});
  readonly confirmedChoices = signal<Record<string, string>>({});
  readonly state = toSignal(combineLatest([this.query$, this.user.user$.pipe(distinctUntilChanged((a, b) => a?.id === b?.id), tap(() => {
    this.forms.clear(); this.displayed.set(null); this.confirmedChoices.set({}); this.errors.set({});
  }))]).pipe(filter(([query]) => !!query.id), switchMap(([query]) => this.service.getPage(query.id, query.page).pipe(
    tap(data => { this.displayed.set(data); this.scheduleBoundary(data); }),
    map((data): PollState => ({ loading: false, data, error: '' })),
    startWith({ loading: true, data: this.displayed(), error: '' } as PollState),
    catchError(err => of({ loading: false, data: this.displayed(), error: apiErrorMessage(err, 'Polls could not be loaded.') } as PollState)),
  ))), { initialValue: { loading: true, data: null, error: '' } as PollState });
  constructor() {
    this.match$.pipe(filter(Boolean), switchMap(id => this.live.watch(id)), takeUntilDestroyed()).subscribe(event => {
      if (event.type === 'state') {
        this.connection.set(event.state); if (event.state === 'connected') this.retry();
      } else if (event.data.change === 'polls' || event.data.change === 'deleted') this.retry();
    });
    inject(DestroyRef).onDestroy(() => { if (this.boundaryTimer !== undefined) clearTimeout(this.boundaryTimer); });
  }
  ngOnChanges(): void {
    this.forms.clear(); this.displayed.set(null); this.confirmedChoices.set({}); this.errors.set({});
    this.query$.next({ id: this.matchPublicId, page: 1 }); this.match$.next(this.matchPublicId);
  }
  voteForm(poll: Poll): FormGroup<{ optionPublicId: FormControl<string> }> {
    let form = this.forms.get(poll.publicId);
    if (!form) { form = this.fb.nonNullable.group({ optionPublicId: ['', [Validators.required, Validators.pattern(/^[0-9a-f]{32}$/)]] }); this.forms.set(poll.publicId, form); }
    return form;
  }
  choice(poll: Poll): string | null { return poll.myOptionPublicId || this.confirmedChoices()[poll.publicId] || null; }
  page(page: number): void { this.displayed.set(null); this.query$.next({ id: this.matchPublicId, page }); }
  retry(): void { this.query$.next({ ...this.query$.value }); }
  login(): void { this.authModal?.openLogin(); }
  vote(poll: Poll): void {
    if (!this.viewer() || !poll.isOpen || this.choice(poll) || this.busy() || this.state().error) return;
    const form = this.voteForm(poll); form.markAllAsTouched();
    if (form.invalid) return;
    const option = form.controls.optionPublicId.value;
    if (!poll.options.some(x => x.publicId === option)) return;
    this.busy.set(poll.publicId); this.errors.update(errors => ({ ...errors, [poll.publicId]: '' }));
    this.service.vote(poll.publicId, option).subscribe({
      next: () => {
        this.confirmedChoices.update(choices => ({ ...choices, [poll.publicId]: option }));
        this.busy.set(null); this.retry();
      },
      error: err => { this.busy.set(null); this.errors.update(errors => ({ ...errors, [poll.publicId]: apiErrorMessage(err, 'Your vote could not be saved.') })); this.retry(); },
    });
  }
  private scheduleBoundary(page: PagedResult<Poll>): void {
    if (this.boundaryTimer !== undefined) clearTimeout(this.boundaryTimer);
    const delays = page.items.flatMap(poll => [Date.parse(poll.opensAt) - Date.parse(poll.serverNow), Date.parse(poll.closesAt) - Date.parse(poll.serverNow)])
      .filter(delay => Number.isFinite(delay) && delay > 0);
    if (delays.length) this.boundaryTimer = setTimeout(() => this.retry(), Math.min(2147483647, Math.min(...delays) + 100));
  }
}
