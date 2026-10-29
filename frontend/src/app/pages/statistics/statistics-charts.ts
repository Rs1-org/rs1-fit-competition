import { Component, computed, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { Subject, catchError, distinctUntilChanged, map, merge, of, startWith, switchMap, tap } from 'rxjs';
import { StatisticCharts } from '../../core/models/statistic';
import { SeasonListDto } from '../../core/models/season';
import { StatisticService } from '../../core/services/statistic.service';
import { CompetitionService } from '../../core/services/competition.service';
import { SeasonService } from '../../core/services/season.service';
import { fetchAllPages, MAX_PAGE_SIZE } from '../../core/helpers/paging.helper';
import { apiErrorMessage } from '../../core/utils/api-error';
import { Chart, ChartSeries } from '../../shared/components/chart/chart';

interface ChartsState { loading: boolean; data: StatisticCharts | null; error: string; }

@Component({ selector: 'app-statistics-charts', imports: [ReactiveFormsModule, RouterLink, Chart],
  templateUrl: './statistics-charts.html', styleUrls: ['./statistics.css', './statistics-charts.css'] })
export class StatisticsCharts {
  private readonly service = inject(StatisticService);
  private readonly competitionService = inject(CompetitionService);
  private readonly seasonService = inject(SeasonService);
  readonly filters = inject(FormBuilder).nonNullable.group({ competitionPublicId: [''], seasonPublicId: [''] });

  readonly competitions = toSignal(fetchAllPages(page => this.competitionService.getAll(page, MAX_PAGE_SIZE)).pipe(
    catchError(() => of([]))), { initialValue: [] });

  readonly seasons = toSignal(this.filters.controls.competitionPublicId.valueChanges.pipe(
    distinctUntilChanged(),
    tap(() => this.filters.controls.seasonPublicId.setValue('', { emitEvent: false })),
    switchMap(id => id ? fetchAllPages(page => this.seasonService.getAll(page, MAX_PAGE_SIZE, id)).pipe(
      catchError(() => of([] as SeasonListDto[]))) : of([] as SeasonListDto[])),
  ), { initialValue: [] as SeasonListDto[] });

  private readonly reload$ = new Subject<void>();
  readonly state = toSignal(merge(this.filters.valueChanges, this.reload$).pipe(
    startWith(this.filters.getRawValue()),
    map(() => this.filters.getRawValue()),
    switchMap(filter => this.service.getCharts(filter).pipe(
      map((data): ChartsState => ({ loading: false, data, error: '' })),
      startWith({ loading: true, data: null, error: '' } as ChartsState),
      catchError(err => of({ loading: false, data: null, error: apiErrorMessage(err, 'Charts could not be loaded.') } as ChartsState)),
    )),
  ), { initialValue: { loading: true, data: null, error: '' } as ChartsState });

  readonly goals = computed(() => {
    const rounds = this.state().data?.goalsPerRound ?? [];
    return { labels: rounds.map(r => r.roundName), series: [
      { label: 'Goals', data: rounds.map(r => r.goals), color: '--accent' },
      { label: 'Goals per match', data: rounds.map(r => +(r.goals / r.matches).toFixed(2)), color: '--danger', type: 'line' },
    ] as ChartSeries[] };
  });

  readonly results = computed(() => {
    const teams = this.state().data?.teamResults ?? [];
    return { labels: teams.map(t => t.selectionName), series: [
      { label: 'Wins', data: teams.map(t => t.wins), color: '--accent' },
      { label: 'Draws', data: teams.map(t => t.draws), color: '--text-muted' },
      { label: 'Losses', data: teams.map(t => t.losses), color: '--danger' },
    ] as ChartSeries[] };
  });

  readonly shots = computed(() => {
    const teams = this.state().data?.teamShots ?? [];
    return { labels: teams.map(t => t.selectionName), series: [
      { label: 'Shots per match', data: teams.map(t => t.averageShots), color: '--text-secondary' },
      { label: 'On target per match', data: teams.map(t => t.averageShotsOnTarget), color: '--accent' },
    ] as ChartSeries[] };
  });

  reset(): void { this.filters.reset(); }
  retry(): void { this.reload$.next(); }
}
