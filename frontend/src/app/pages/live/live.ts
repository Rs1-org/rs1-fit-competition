import { Component, DestroyRef, effect, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, EMPTY, catchError, combineLatest, distinctUntilChanged, filter, map, of, startWith, switchMap } from 'rxjs';
import { CompetitionService } from '../../core/services/competition.service';
import { SeasonService } from '../../core/services/season.service';
import { StatisticService } from '../../core/services/statistic.service';
import { SelectionOption, SelectionService } from '../../core/services/selection.service';
import { SeasonListDto } from '../../core/models/season';
import { Standings } from '../../core/models/statistic';
import { fetchAllPages, MAX_PAGE_SIZE } from '../../core/helpers/paging.helper';
import { apiErrorMessage } from '../../core/utils/api-error';
import { SignalRService } from '../../core/services/signalr.service';
import { MatchChangeNotice } from '../../core/services/match-live.service';
import { LeagueTable } from '../../shared/components/league-table/league-table';
import { Skeleton } from '../../shared/components/skeleton/skeleton';
import { EmptyState } from '../../shared/components/empty-state/empty-state';
import { ErrorBanner } from '../../shared/components/error-banner/error-banner';

interface TableState { loading: boolean; data: Standings | null; error: string; }

@Component({
  selector: 'app-live-table',
  imports: [ReactiveFormsModule, LeagueTable, Skeleton, EmptyState, ErrorBanner],
  templateUrl: './live.html',
  styleUrl: './live.css',
})
export class LiveTable {
  private readonly statistics = inject(StatisticService);
  private readonly competitionsApi = inject(CompetitionService);
  private readonly seasonsApi = inject(SeasonService);
  private readonly selectionsApi = inject(SelectionService);
  private readonly realtime = inject(SignalRService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly refresh$ = new BehaviorSubject<void>(undefined);
  private readonly watched$ = new BehaviorSubject('');

  readonly filters = inject(FormBuilder).nonNullable.group({ competitionPublicId: '', seasonPublicId: '' });

  readonly competitions = toSignal(
    fetchAllPages((page) => this.competitionsApi.getAll(page, MAX_PAGE_SIZE)).pipe(catchError(() => of([]))),
    { initialValue: [] },
  );
  readonly seasons = toSignal(this.filters.controls.competitionPublicId.valueChanges.pipe(
    startWith(this.filters.controls.competitionPublicId.value),
    distinctUntilChanged(),
    switchMap((id) => id
      ? fetchAllPages((page) => this.seasonsApi.getAll(page, MAX_PAGE_SIZE, id)).pipe(catchError(() => of([] as SeasonListDto[])))
      : of([] as SeasonListDto[])),
  ), { initialValue: [] as SeasonListDto[] });
  readonly selections = toSignal(
    fetchAllPages((page) => this.selectionsApi.getAll(page, MAX_PAGE_SIZE)).pipe(catchError(() => of([] as SelectionOption[]))),
    { initialValue: [] as SelectionOption[] },
  );

  readonly state = toSignal(combineLatest([
    this.filters.valueChanges.pipe(startWith(this.filters.getRawValue())),
    this.refresh$,
  ]).pipe(
    filter(([value]) => !!value.competitionPublicId),
    switchMap(([value]) => this.statistics.getStandings({
      competitionPublicId: value.competitionPublicId,
      seasonPublicId: value.seasonPublicId || undefined,
    }).pipe(
      map((data): TableState => {
        const key = [...data.matchPublicIds].sort().join(',');
        if (key !== this.watched$.value) this.watched$.next(key);
        return { loading: false, data, error: '' };
      }),
      startWith({ loading: true, data: null, error: '' } satisfies TableState),
      catchError((err) => of({ loading: false, data: null, error: apiErrorMessage(err, 'The table could not be loaded.') } satisfies TableState)),
    )),
  ), { initialValue: { loading: false, data: null, error: '' } satisfies TableState });

  constructor() {
    effect(() => {
      const list = this.competitions();
      if (list.length && !this.filters.controls.competitionPublicId.value) {
        this.filters.controls.competitionPublicId.setValue(list[0].publicId);
      }
    });
    effect(() => {
      const seasons = this.seasons();
      const current = seasons.find((season) => season.isCurrent) ?? seasons[0];
      if (current && !this.filters.controls.seasonPublicId.value) {
        this.filters.controls.seasonPublicId.setValue(current.publicId);
      }
    });
    this.filters.controls.competitionPublicId.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => {
      this.filters.controls.seasonPublicId.setValue('', { emitEvent: false });
    });
    this.watched$.pipe(
      distinctUntilChanged(),
      switchMap((key) => {
        const ids = key ? key.split(',') : [];
        if (!ids.length) return EMPTY;
        return this.realtime.watch<MatchChangeNotice>({
          hubPath: '/hubs/matches',
          eventName: 'MatchChanged',
          joinMethod: 'WatchMatches',
          args: [ids],
        });
      }),
      filter((event) => event.type === 'data'),
      takeUntilDestroyed(),
    ).subscribe(() => this.refresh$.next());
  }

  retry(): void {
    this.refresh$.next();
  }
}
