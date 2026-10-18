import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, map, of, startWith, switchMap } from 'rxjs';
import { Statistic, StatisticFilter } from '../../core/models/statistic';
import { ReviewMatchDetails } from '../../core/models/match-review';
import { PagedResult } from '../../core/models/paged-result';
import { StatisticService } from '../../core/services/statistic.service';
import { MatchReviewService } from '../../core/services/match-review.service';
import { UserStateService } from '../../core/services/user-state.service';
import { EventMatchPicker } from '../../components/event-match-picker/event-match-picker';
import { Pagination } from '../../shared/components/pagination/pagination';
import { AppDateTimePipe } from '../../shared/pipes/regional-format.pipes';
import { apiErrorMessage } from '../../core/utils/api-error';

interface StatisticsState { loading: boolean; data: PagedResult<Statistic> | null; error: string; }
@Component({ selector: 'app-statistics', imports: [ReactiveFormsModule, RouterLink, EventMatchPicker, Pagination, AppDateTimePipe],
  templateUrl: './statistics.html', styleUrl: './statistics.css' })
export class Statistics {
  private readonly service = inject(StatisticService);
  private readonly matches = inject(MatchReviewService);
  private readonly fb = inject(FormBuilder);
  readonly canManage = inject(UserStateService).canManage;
  readonly matchId = signal('');
  readonly filterError = signal('');
  readonly filters = this.fb.nonNullable.group({ selectionPublicId: [''], dateFrom: [''], dateTo: [''],
    minTotalShots: this.fb.control<number | null>(null, [Validators.min(0), Validators.max(2147483647), Validators.pattern(/^\d+$/)]),
  });
  private readonly query$ = new BehaviorSubject<{ filter: StatisticFilter; page: number }>({ filter: {}, page: 1 });
  private readonly match$ = new BehaviorSubject('');
  readonly context = toSignal(this.match$.pipe(switchMap(id => id ? this.matches.getMatch(id).pipe(
    map(data => ({ data, error: '' })), startWith({ data: null as ReviewMatchDetails | null, error: '' }),
    catchError(err => of({ data: null, error: apiErrorMessage(err, 'The match teams could not be loaded.') })),
  ) : of({ data: null, error: '' }))), { initialValue: { data: null, error: '' } });
  readonly state = toSignal(this.query$.pipe(switchMap(query => this.service.getPage(query.filter, query.page).pipe(
    map((data): StatisticsState => ({ loading: false, data, error: '' })),
    startWith({ loading: true, data: null, error: '' } as StatisticsState),
    catchError(err => of({ loading: false, data: null, error: apiErrorMessage(err, 'Statistics could not be loaded.') } as StatisticsState)),
  ))), { initialValue: { loading: true, data: null, error: '' } as StatisticsState });
  chooseMatch(id: string): void { this.matchId.set(id); this.match$.next(id); this.filters.controls.selectionPublicId.setValue(''); this.apply(); }
  apply(): void {
    this.filters.markAllAsTouched(); const value = this.filters.getRawValue();
    const from = value.dateFrom ? new Date(value.dateFrom) : null;
    const to = value.dateTo ? new Date(value.dateTo) : null;
    if (this.filters.invalid || from && !Number.isFinite(from.getTime()) || to && !Number.isFinite(to.getTime()) || from && to && from > to) {
      this.filterError.set('Use a valid date range and a non-negative whole number of shots.'); return;
    }
    this.filterError.set('');
    this.query$.next({ page: 1, filter: { matchPublicId: this.matchId() || undefined,
      selectionPublicId: value.selectionPublicId || undefined, dateFrom: from?.toISOString(), dateTo: to?.toISOString(), minTotalShots: value.minTotalShots } });
  }
  reset(): void { this.filters.reset(); this.chooseMatch(''); }
  page(page: number): void { this.query$.next({ ...this.query$.value, page }); }
  retry(): void { this.page(this.query$.value.page); this.match$.next(this.matchId()); }
}
