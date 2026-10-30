import { Component, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { Observable, Subject, catchError, distinctUntilChanged, map, merge, of, startWith, switchMap, tap } from 'rxjs';
import { HeadToHead, PlayerStats, StatisticChartFilter, TeamForm } from '../../core/models/statistic';
import { SeasonListDto } from '../../core/models/season';
import { PagedResult } from '../../core/models/paged-result';
import { StatisticService } from '../../core/services/statistic.service';
import { CompetitionService } from '../../core/services/competition.service';
import { SeasonService } from '../../core/services/season.service';
import { SelectionService } from '../../core/services/selection.service';
import { UserStateService } from '../../core/services/user-state.service';
import { fetchAllPages, MAX_PAGE_SIZE } from '../../core/helpers/paging.helper';
import { apiErrorMessage } from '../../core/utils/api-error';
import { Chart, ChartSeries } from '../../shared/components/chart/chart';
import { Pagination } from '../../shared/components/pagination/pagination';
import { Paywall } from '../../shared/components/paywall/paywall';
import { AppDatePipe } from '../../shared/pipes/regional-format.pipes';

interface LoadState<T> { loading: boolean; data: T | null; error: string; }
const IDLE = { loading: false, data: null, error: '' };

@Component({ selector: 'app-advanced-statistics', imports: [ReactiveFormsModule, RouterLink, Chart, Pagination, Paywall, AppDatePipe],
  templateUrl: './advanced-statistics.html', styleUrls: ['../statistics/statistics.css', '../statistics/statistics-charts.css'] })
export class AdvancedStatistics {
  private readonly service = inject(StatisticService);
  private readonly competitionService = inject(CompetitionService);
  private readonly seasonService = inject(SeasonService);
  private readonly selectionService = inject(SelectionService);
  private readonly userState = inject(UserStateService);

  readonly lastOptions = [5, 10, 20];
  readonly filters = inject(FormBuilder).nonNullable.group({ competitionPublicId: [''], seasonPublicId: [''],
    firstSelectionPublicId: [''], secondSelectionPublicId: [''], last: [5] });

  /** The stored user can be out of date, so a 403 from the API also shows the paywall. */
  private readonly forbidden = signal(false);
  readonly unlocked = computed(() => this.userState.isPremium() && !this.forbidden());

  readonly competitions = toSignal(fetchAllPages(page => this.competitionService.getAll(page, MAX_PAGE_SIZE)).pipe(
    catchError(() => of([]))), { initialValue: [] });
  readonly selections = toSignal(fetchAllPages(page => this.selectionService.getAll(page, MAX_PAGE_SIZE)).pipe(
    catchError(() => of([]))), { initialValue: [] });
  readonly seasons = toSignal(this.filters.controls.competitionPublicId.valueChanges.pipe(
    distinctUntilChanged(),
    tap(() => this.filters.controls.seasonPublicId.setValue('', { emitEvent: false })),
    switchMap(id => id ? fetchAllPages(page => this.seasonService.getAll(page, MAX_PAGE_SIZE, id)).pipe(
      catchError(() => of([] as SeasonListDto[]))) : of([] as SeasonListDto[])),
  ), { initialValue: [] as SeasonListDto[] });

  private readonly values$ = toObservable(this.unlocked).pipe(
    switchMap(unlocked => unlocked ? this.filters.valueChanges.pipe(startWith(null), map(() => this.filters.getRawValue())) : []),
  );

  private load<T>(request: Observable<T>, fallback: string): Observable<LoadState<T>> {
    return request.pipe(
      map((data): LoadState<T> => ({ loading: false, data, error: '' })),
      startWith({ loading: true, data: null, error: '' } as LoadState<T>),
      catchError(err => {
        if (err instanceof HttpErrorResponse && err.status === 403) this.forbidden.set(true);
        return of({ loading: false, data: null, error: apiErrorMessage(err, fallback) } as LoadState<T>);
      }),
    );
  }

  private scope(value: StatisticChartFilter): StatisticChartFilter {
    return { competitionPublicId: value.competitionPublicId, seasonPublicId: value.seasonPublicId };
  }

  readonly form = toSignal(this.values$.pipe(switchMap(value => value.firstSelectionPublicId
    ? this.load(this.service.getTeamForm(value.firstSelectionPublicId, value.last, this.scope(value)), 'Team form could not be loaded.')
    : of(IDLE as LoadState<TeamForm>))), { initialValue: IDLE as LoadState<TeamForm> });

  readonly headToHead = toSignal(this.values$.pipe(switchMap(value =>
    value.firstSelectionPublicId && value.secondSelectionPublicId && value.firstSelectionPublicId !== value.secondSelectionPublicId
      ? this.load(this.service.getHeadToHead(value.firstSelectionPublicId, value.secondSelectionPublicId, this.scope(value)), 'Head-to-head could not be loaded.')
      : of(IDLE as LoadState<HeadToHead>))), { initialValue: IDLE as LoadState<HeadToHead> });

  private readonly page$ = new Subject<number>();
  readonly players = toSignal(merge(
    this.values$.pipe(map(() => 1)),
    this.page$,
  ).pipe(switchMap(page => {
    const value = this.filters.getRawValue();
    return this.load(this.service.getPlayerStats({ ...this.scope(value), selectionPublicId: value.firstSelectionPublicId }, page), 'Player statistics could not be loaded.');
  })), { initialValue: IDLE as LoadState<PagedResult<PlayerStats>> });

  readonly formChart = computed(() => {
    const matches = [...(this.form().data?.matches ?? [])].reverse();
    return { labels: matches.map(m => m.opponentName), series: [
      { label: 'Goals scored', data: matches.map(m => m.goalsFor), color: '--accent' },
      { label: 'Goals conceded', data: matches.map(m => m.goalsAgainst), color: '--danger' },
    ] as ChartSeries[] };
  });

  readonly headToHeadChart = computed(() => {
    const data = this.headToHead().data;
    return { labels: data ? [data.firstName, 'Draws', data.secondName] : [],
      series: [{ label: 'Results', data: data ? [data.firstWins, data.draws, data.secondWins] : [], color: '--accent' }] as ChartSeries[] };
  });

  readonly playersChart = computed(() => {
    const players = this.players().data?.items ?? [];
    return { labels: players.map(p => p.playerName), series: [
      { label: 'Goals', data: players.map(p => p.goals), color: '--accent' },
      { label: 'Yellow cards', data: players.map(p => p.yellowCards), color: '--text-muted' },
      { label: 'Red cards', data: players.map(p => p.redCards), color: '--danger' },
    ] as ChartSeries[] };
  });

  page(page: number): void { this.page$.next(page); }
}
