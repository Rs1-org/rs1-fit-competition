import { Component, computed, DestroyRef, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, distinctUntilChanged, filter, map, of, startWith, switchMap } from 'rxjs';
import { AppDateTimePipe, AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { RoundService } from '../../core/services/round.service';
import { MatchService } from '../../core/services/match.service';
import { RoundDetailsDto } from '../../core/models/round';
import { MatchListDto } from '../../core/models/match';
import { fetchAllPages, MAX_PAGE_SIZE } from '../../core/helpers/paging.helper';
import { apiErrorMessage } from '../../core/utils/api-error';
import { matchPhase, competitionCode } from '../../shared/match/match-display';
import { Breadcrumb, Crumb } from '../../shared/components/breadcrumb/breadcrumb';
import { EntityHeader } from '../../shared/components/entity-header/entity-header';
import { StatTile } from '../../shared/components/stat-tile/stat-tile';
import { MatchRow } from '../../shared/components/match-row/match-row';
import { Skeleton } from '../../shared/components/skeleton/skeleton';
import { EmptyState } from '../../shared/components/empty-state/empty-state';
import { ErrorBanner } from '../../shared/components/error-banner/error-banner';

interface RoundState { data: RoundDetailsDto | null; loading: boolean; error: string; notFound: boolean; }
interface MatchesState { data: MatchListDto[] | null; loading: boolean; error: string; }

@Component({
  selector: 'app-round-detail',
  standalone: true,
  imports: [RouterLink, AppNumberPipe, Breadcrumb, EntityHeader, StatTile, MatchRow, Skeleton, EmptyState, ErrorBanner],
  templateUrl: './round-detail.html',
  styleUrl: './round-detail.css',
})
export class RoundDetail {
  readonly code = competitionCode;
  private route = inject(ActivatedRoute);
  private roundService = inject(RoundService);
  private matchService = inject(MatchService);

  private reload$ = new BehaviorSubject<void>(undefined);
  private matchesRefresh$ = new BehaviorSubject<void>(undefined);
  readonly narrow = signal(false);

  state = toSignal(
    combineLatest([this.route.paramMap.pipe(map((params) => params.get('slug') ?? '')), this.reload$]).pipe(
      switchMap(([slug]) => this.roundService.getBySlug(slug).pipe(
        map((data): RoundState => ({ data, loading: false, error: '', notFound: false })),
        startWith<RoundState>({ data: null, loading: true, error: '', notFound: false }),
        catchError((err) => of<RoundState>(err instanceof HttpErrorResponse && err.status === 404
          ? { data: null, loading: false, error: '', notFound: true }
          : { data: null, loading: false, error: apiErrorMessage(err, 'The round could not be loaded.'), notFound: false })),
      )),
    ),
    { initialValue: { data: null, loading: true, error: '', notFound: false } as RoundState },
  );

  round = computed(() => this.state().data);
  private round$ = toObservable(this.round).pipe(
    filter((r): r is RoundDetailsDto => !!r),
    distinctUntilChanged((a, b) => a.publicId === b.publicId),
  );

  /** A round holds a handful of matches, so all of them are loaded, in kick-off order. */
  matches = toSignal(
    combineLatest([this.round$, this.matchesRefresh$]).pipe(
      switchMap(([r]) => fetchAllPages((page) =>
        this.matchService.getAll(page, MAX_PAGE_SIZE, { roundPublicId: r.publicId, sortBy: 'date', sortDir: 'asc' })).pipe(
        map((data): MatchesState => ({ data, loading: false, error: '' })),
        startWith<MatchesState>({ data: null, loading: true, error: '' }),
        catchError((err) => of<MatchesState>({ data: null, loading: false, error: apiErrorMessage(err, 'Matches could not be loaded.') })),
      )),
    ),
    { initialValue: { data: null, loading: true, error: '' } as MatchesState },
  );

  crumbs = computed<Crumb[]>(() => {
    const r = this.round();
    return [
      { label: 'Competitions', route: '/competitions' },
      ...(r ? [
        { label: r.competitionName, route: `/competitions/${r.competitionSlug}` },
        { label: r.seasonName, route: `/seasons/${r.seasonSlug}` },
      ] : []),
      { label: r?.name ?? 'Round' },
    ];
  });

  played = computed(() => (this.matches().data ?? []).filter((m) => matchPhase(m.statusName, m.isPostponed) === 'finished').length);
  goals = computed(() => (this.matches().data ?? [])
    .filter((m) => matchPhase(m.statusName, m.isPostponed) === 'finished')
    .reduce((sum, m) => sum + (m.homeScore ?? 0) + (m.awayScore ?? 0), 0));
  liveCount = computed(() => (this.matches().data ?? []).filter((m) => {
    const phase = matchPhase(m.statusName, m.isPostponed);
    return phase === 'live' || phase === 'half-time';
  }).length);

  constructor() {
    if (typeof matchMedia !== 'function') return;
    const query = matchMedia('(max-width: 768px)');
    this.narrow.set(query.matches);
    const listener = (event: MediaQueryListEvent) => this.narrow.set(event.matches);
    query.addEventListener('change', listener);
    inject(DestroyRef).onDestroy(() => query.removeEventListener('change', listener));
  }

  private dateTimePipe = new AppDateTimePipe();

  meta(match: MatchListDto): string {
    const date = match.date ? this.dateTimePipe.transform(match.date) : 'Date to be confirmed';
    return match.stadiumName ? `${date} · ${match.stadiumName}` : date;
  }

  retry(): void {
    this.reload$.next();
  }

  retryMatches(): void {
    this.matchesRefresh$.next();
  }
}
