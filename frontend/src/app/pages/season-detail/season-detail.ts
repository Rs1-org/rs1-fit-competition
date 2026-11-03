import { Component, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed, toObservable, toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, EMPTY, catchError, combineLatest, distinctUntilChanged, filter, map, of, startWith, switchMap } from 'rxjs';
import { SeasonService } from '../../core/services/season.service';
import { RoundService } from '../../core/services/round.service';
import { MatchService } from '../../core/services/match.service';
import { StatisticService } from '../../core/services/statistic.service';
import { SelectionOption, SelectionService } from '../../core/services/selection.service';
import { SignalRService } from '../../core/services/signalr.service';
import { MatchChangeNotice } from '../../core/services/match-live.service';
import { UserStateService } from '../../core/services/user-state.service';
import { ToastService } from '../../core/services/toast.service';
import { SeasonDetailsDto } from '../../core/models/season';
import { RoundListDto } from '../../core/models/round';
import { MatchListDto } from '../../core/models/match';
import { PlayerStats, Standings } from '../../core/models/statistic';
import { fetchAllPages, MAX_PAGE_SIZE } from '../../core/helpers/paging.helper';
import { formByTeam, roundWindow, RoundSummary, summariseRounds } from '../../core/helpers/season-matches.helper';
import { apiErrorMessage } from '../../core/utils/api-error';
import { ConfirmDialogService, deleteConfirm } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { AppDatePipe, AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { Breadcrumb, Crumb } from '../../shared/components/breadcrumb/breadcrumb';
import { EntityHeader } from '../../shared/components/entity-header/entity-header';
import { StatTile } from '../../shared/components/stat-tile/stat-tile';
import { LeagueTable } from '../../shared/components/league-table/league-table';
import { Skeleton } from '../../shared/components/skeleton/skeleton';
import { EmptyState } from '../../shared/components/empty-state/empty-state';
import { ErrorBanner } from '../../shared/components/error-banner/error-banner';
import { Button } from '../../shared/components/button/button';
import { Icon } from '../../shared/components/icon/icon';
import { Crest } from '../../shared/components/crest/crest';
import { RoundForm } from '../../components/round-form/round-form';
import { FormResult, competitionCode } from '../../shared/match/match-display';

interface Loadable<T> { data: T | null; loading: boolean; error: string; }
interface SeasonState extends Loadable<SeasonDetailsDto> { notFound: boolean; }
type ScorersState = Loadable<PlayerStats[]> & { locked: boolean };

const TOP_SCORERS = 5;

@Component({
  selector: 'app-season-detail',
  standalone: true,
  imports: [
    RouterLink, AppNumberPipe, Breadcrumb, EntityHeader, StatTile, LeagueTable, Skeleton, EmptyState, ErrorBanner, Button, Icon,
    Crest, RoundForm,
  ],
  templateUrl: './season-detail.html',
  styleUrl: './season-detail.css',
})
export class SeasonDetail {
  readonly code = competitionCode;
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private seasonService = inject(SeasonService);
  private roundService = inject(RoundService);
  private matchService = inject(MatchService);
  private statistics = inject(StatisticService);
  private selectionService = inject(SelectionService);
  private realtime = inject(SignalRService);
  private confirmDialog = inject(ConfirmDialogService);
  private toast = inject(ToastService);
  private userState = inject(UserStateService);
  private datePipe = new AppDatePipe();

  canManage = this.userState.canManage;
  canDelete = this.userState.isAdmin;

  private reload$ = new BehaviorSubject<void>(undefined);
  private roundsRefresh$ = new BehaviorSubject<void>(undefined);
  private tableRefresh$ = new BehaviorSubject<void>(undefined);
  private watched$ = new BehaviorSubject('');

  state = toSignal(
    combineLatest([this.route.paramMap.pipe(map((params) => params.get('slug') ?? '')), this.reload$]).pipe(
      switchMap(([slug]) => this.seasonService.getBySlug(slug).pipe(
        map((data): SeasonState => ({ data, loading: false, error: '', notFound: false })),
        startWith<SeasonState>({ data: null, loading: true, error: '', notFound: false }),
        catchError((err) => of<SeasonState>(err instanceof HttpErrorResponse && err.status === 404
          ? { data: null, loading: false, error: '', notFound: true }
          : { data: null, loading: false, error: apiErrorMessage(err, 'The season could not be loaded.'), notFound: false })),
      )),
    ),
    { initialValue: { data: null, loading: true, error: '', notFound: false } as SeasonState },
  );

  season = computed(() => this.state().data);
  private season$ = toObservable(this.season).pipe(
    filter((s): s is SeasonDetailsDto => !!s),
    distinctUntilChanged((a, b) => a.publicId === b.publicId && a.startDate === b.startDate && a.endDate === b.endDate),
  );

  standings = toSignal(
    combineLatest([this.season$, this.tableRefresh$]).pipe(
      switchMap(([s]) => this.statistics.getStandings({ competitionPublicId: s.competitionPublicId, seasonPublicId: s.publicId }).pipe(
        map((data): Loadable<Standings> => {
          const key = [...data.matchPublicIds].sort().join(',');
          if (key !== this.watched$.value) this.watched$.next(key);
          return { data, loading: false, error: '' };
        }),
        startWith<Loadable<Standings>>({ data: null, loading: true, error: '' }),
        catchError((err) => of<Loadable<Standings>>({ data: null, loading: false, error: apiErrorMessage(err, 'The table could not be loaded.') })),
      )),
    ),
    { initialValue: { data: null, loading: true, error: '' } as Loadable<Standings> },
  );

  /** Every match of this season; the match list has no season filter, so competition + dates + season name. */
  private seasonMatches = toSignal(
    combineLatest([this.season$, this.tableRefresh$]).pipe(
      switchMap(([s]) => fetchAllPages((page) => this.matchService.getAll(page, MAX_PAGE_SIZE, {
        competitionPublicId: s.competitionPublicId,
        dateFrom: s.startDate,
        dateTo: `${s.endDate}T23:59:59`,
        sortBy: 'date',
        sortDir: 'asc',
      })).pipe(
        map((items) => items.filter((match) => match.seasonName === s.name)),
        catchError(() => of(null)),
      )),
    ),
    { initialValue: [] as MatchListDto[] | null },
  );

  rounds = toSignal(
    combineLatest([this.season$, this.roundsRefresh$]).pipe(
      switchMap(([s]) => fetchAllPages((page) => this.roundService.getAll(page, MAX_PAGE_SIZE, s.publicId)).pipe(
        map((data): Loadable<RoundListDto[]> => ({ data, loading: false, error: '' })),
        startWith<Loadable<RoundListDto[]>>({ data: null, loading: true, error: '' }),
        catchError((err) => of<Loadable<RoundListDto[]>>({ data: null, loading: false, error: apiErrorMessage(err, 'Rounds could not be loaded.') })),
      )),
    ),
    { initialValue: { data: null, loading: true, error: '' } as Loadable<RoundListDto[]> },
  );

  /** Top scorers come from a premium endpoint; everyone else sees a labelled card instead. */
  scorers = toSignal(
    combineLatest([this.season$, toObservable(this.userState.isPremium)]).pipe(
      switchMap(([s, premium]) => !premium
        ? of<ScorersState>({ data: null, loading: false, error: '', locked: true })
        : this.statistics.getPlayerStats({ competitionPublicId: s.competitionPublicId, seasonPublicId: s.publicId }).pipe(
          map((page): ScorersState => ({
            data: page.items.filter((row) => row.goals > 0).slice(0, TOP_SCORERS), loading: false, error: '', locked: false,
          })),
          startWith<ScorersState>({ data: null, loading: true, error: '', locked: false }),
          catchError((err) => of<ScorersState>(err instanceof HttpErrorResponse && (err.status === 401 || err.status === 403)
            ? { data: null, loading: false, error: '', locked: true }
            : { data: null, loading: false, error: apiErrorMessage(err, 'Top scorers could not be loaded.'), locked: false })),
        )),
    ),
    { initialValue: { data: null, loading: true, error: '', locked: false } as ScorersState },
  );

  private selections = toSignal(
    fetchAllPages((page) => this.selectionService.getAll(page, MAX_PAGE_SIZE)).pipe(catchError(() => of([] as SelectionOption[]))),
    { initialValue: [] as SelectionOption[] },
  );
  teams = computed(() => this.selections().map((s) => ({ publicId: s.publicId, slug: s.slug, abbreviation: s.abbreviation })));

  /** Form chips keyed by team publicId, as the table expects. */
  form = computed<Record<string, FormResult[]> | null>(() => {
    const matches = this.seasonMatches();
    const table = this.standings().data?.table;
    if (!matches || !table) return null;
    const byName = formByTeam(matches);
    return Object.fromEntries(table.map((row) => [row.selectionPublicId, byName[row.selectionName] ?? []]));
  });

  roundSummaries = computed(() => summariseRounds(this.rounds().data ?? [], this.seasonMatches() ?? []));
  showAllRounds = signal(false);
  visibleRounds = computed(() => this.showAllRounds() ? this.roundSummaries() : roundWindow(this.roundSummaries()));

  roundsTile = computed(() => {
    const all = this.roundSummaries();
    if (!all.length) return '0';
    return `${all.filter((s) => s.state === 'done').length} / ${all.length}`;
  });
  teamsCount = computed(() => this.standings().data?.table.length ?? 0);
  goalsPerMatch = computed(() => {
    const table = this.standings().data?.table ?? [];
    const matches = table.reduce((sum, row) => sum + row.played, 0) / 2;
    const goals = table.reduce((sum, row) => sum + row.goalsFor, 0);
    return matches ? goals / matches : null;
  });

  crumbs = computed<Crumb[]>(() => {
    const s = this.season();
    return [
      { label: 'Competitions', route: '/competitions' },
      ...(s ? [{ label: s.competitionName, route: `/competitions/${s.competitionSlug}` }] : []),
      { label: s?.name ?? 'Season' },
    ];
  });

  subtitle = computed(() => {
    const s = this.season();
    return s ? `Season ${s.name} · ${this.datePipe.transform(s.startDate)} – ${this.datePipe.transform(s.endDate)}` : '';
  });

  isFormOpen = signal(false);
  selected = signal<RoundListDto | null>(null);

  /** Suggest the next round number for the add form (highest known number + 1, so gaps don't collide). */
  nextNumber = computed(() => Math.max(0, ...(this.rounds().data ?? []).map((r) => r.number)) + 1);

  constructor() {
    // Score changes in this season's matches refresh the table, the form and the round labels.
    this.watched$.pipe(
      distinctUntilChanged(),
      switchMap((key) => {
        const ids = key ? key.split(',') : [];
        if (!ids.length) return EMPTY;
        return this.realtime.watch<MatchChangeNotice>({ hubPath: '/hubs/matches', eventName: 'MatchChanged', joinMethod: 'WatchMatches', args: [ids] });
      }),
      filter((event) => event.type === 'data'),
      takeUntilDestroyed(),
    ).subscribe(() => this.tableRefresh$.next());
  }

  roundMeta(summary: RoundSummary): string {
    if (summary.state === 'live') {
      const parts = [`${summary.live} live`];
      if (summary.toPlay) parts.push(`${summary.toPlay} to play`);
      if (summary.postponed) parts.push(`${summary.postponed} postponed`);
      return parts.join(' · ');
    }
    if (summary.state === 'done') return `${plural(summary.matches, 'match', 'matches')} · ${plural(summary.goals, 'goal', 'goals')}`;
    if (summary.firstDate) {
      const first = this.datePipe.transform(summary.firstDate);
      const last = this.datePipe.transform(summary.lastDate);
      return first === last ? first : `${first} – ${last}`;
    }
    return summary.round.matchCount ? plural(summary.round.matchCount, 'match', 'matches') : 'No matches yet';
  }

  stateLabel(summary: RoundSummary): string {
    return { done: 'Done', live: 'Live', next: 'Next', upcoming: '' }[summary.state];
  }

  retry(): void {
    this.reload$.next();
  }

  retryTable(): void {
    this.tableRefresh$.next();
  }

  retryRounds(): void {
    this.roundsRefresh$.next();
  }

  openRound(round: RoundListDto): void {
    this.router.navigate(['/rounds', round.slug]);
  }

  onAdd(): void {
    if (!this.canManage()) return;
    this.selected.set(null);
    this.isFormOpen.set(true);
  }

  onEdit(round: RoundListDto): void {
    if (!this.canManage()) return;
    this.selected.set(round);
    this.isFormOpen.set(true);
  }

  async onDelete(round: RoundListDto): Promise<void> {
    if (!this.canDelete()) return;
    const confirmed = await this.confirmDialog.confirm(deleteConfirm(`${round.name}`));
    if (!confirmed) return;
    this.roundService.delete(round.publicId).subscribe({
      next: () => {
        this.toast.success('Round deleted');
        this.roundsRefresh$.next();
      },
      error: (err) => this.toast.error(apiErrorMessage(err, 'The round could not be deleted.')),
    });
  }

  onFormSaved(): void {
    this.isFormOpen.set(false);
    this.selected.set(null);
    this.roundsRefresh$.next();
  }

  onFormClosed(): void {
    this.isFormOpen.set(false);
    this.selected.set(null);
  }
}

function plural(count: number, one: string, many: string): string {
  return `${count} ${count === 1 ? one : many}`;
}
