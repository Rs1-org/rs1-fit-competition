import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, forkJoin, map, of, startWith, switchMap } from 'rxjs';
import { MatchService } from '../../core/services/match.service';
import { StatisticService } from '../../core/services/statistic.service';
import { CompetitionService } from '../../core/services/competition.service';
import { SelectionOption, SelectionService } from '../../core/services/selection.service';
import { MatchListDto } from '../../core/models/match';
import { CompetitionListDto } from '../../core/models/competition';
import { StatisticCharts, TeamResults } from '../../core/models/statistic';
import { fetchAllPages, MAX_PAGE_SIZE } from '../../core/helpers/paging.helper';
import { apiErrorMessage } from '../../core/utils/api-error';
import { matchPhase } from '../../shared/match/match-display';
import { ChartSeries } from '../../shared/components/chart/chart';
import { AppDatePipe, AppDateTimePipe, AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { LiveCard } from '../../shared/components/live-card/live-card';
import { MatchRow } from '../../shared/components/match-row/match-row';
import { StatTile } from '../../shared/components/stat-tile/stat-tile';
import { SectionCard } from '../../shared/components/section-card/section-card';
import { Chart } from '../../shared/components/chart/chart';
import { Skeleton } from '../../shared/components/skeleton/skeleton';
import { EmptyState } from '../../shared/components/empty-state/empty-state';
import { ErrorBanner } from '../../shared/components/error-banner/error-banner';

export type HomeTab = 'today' | 'upcoming' | 'results';

interface HomePayload {
  matches: MatchListDto[];
  charts: StatisticCharts | null;
  competitions: CompetitionListDto[];
  selections: SelectionOption[];
}

interface HomeState { loading: boolean; data: HomePayload | null; error: string; }

const WEEKDAYS = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

export function startOfDay(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate());
}

export function isSameDay(left: Date, right: Date): boolean {
  return left.getFullYear() === right.getFullYear()
    && left.getMonth() === right.getMonth()
    && left.getDate() === right.getDate();
}

export function matchDay(value: string | null): Date | null {
  if (!value) return null;
  const parsed = new Date(value);
  return Number.isNaN(parsed.getTime()) ? null : startOfDay(parsed);
}

/** Tuesday to Saturday of the week that contains `anchor`. */
export function tuesdayToSaturday(anchor: Date): Date[] {
  const day = anchor.getDay();
  const monday = startOfDay(anchor);
  monday.setDate(anchor.getDate() + (day === 0 ? -6 : 1 - day));
  return [1, 2, 3, 4, 5].map((offset) => {
    const date = new Date(monday);
    date.setDate(monday.getDate() + offset);
    return date;
  });
}

export function toIsoDate(date: Date): string {
  const month = `${date.getMonth() + 1}`.padStart(2, '0');
  const day = `${date.getDate()}`.padStart(2, '0');
  return `${date.getFullYear()}-${month}-${day}`;
}

export function isOnPitch(match: MatchListDto): 'live' | 'half-time' | null {
  const phase = matchPhase(match.statusName, match.isPostponed);
  return phase === 'live' || phase === 'half-time' ? phase : null;
}

export function isFinished(match: MatchListDto): boolean {
  const phase = matchPhase(match.statusName, match.isPostponed);
  return phase === 'finished' || (match.homeScore != null && match.awayScore != null && phase !== 'live' && phase !== 'half-time');
}

@Component({
  selector: 'app-home',
  imports: [
    RouterLink, RouterLinkActive, AppDatePipe, AppDateTimePipe, AppNumberPipe, LiveCard, MatchRow, StatTile,
    SectionCard, Chart, Skeleton, EmptyState, ErrorBanner,
  ],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  private readonly matches = inject(MatchService);
  private readonly statistics = inject(StatisticService);
  private readonly competitions = inject(CompetitionService);
  private readonly selections = inject(SelectionService);
  private readonly reload$ = new BehaviorSubject<void>(undefined);

  readonly tab = signal<HomeTab>('today');
  readonly selected = signal(startOfDay(new Date()));
  readonly narrow = signal(false);
  readonly today = startOfDay(new Date());
  readonly weekday = (date: Date) => WEEKDAYS[date.getDay()];

  readonly state = toSignal(this.reload$.pipe(
    switchMap(() => forkJoin({
      matches: fetchAllPages((page) => this.matches.getAll(page, MAX_PAGE_SIZE)),
      charts: this.statistics.getCharts({}).pipe(catchError(() => of(null))),
      competitions: fetchAllPages((page) => this.competitions.getAll(page, MAX_PAGE_SIZE)).pipe(catchError(() => of([] as CompetitionListDto[]))),
      selections: fetchAllPages((page) => this.selections.getAll(page, MAX_PAGE_SIZE)).pipe(catchError(() => of([] as SelectionOption[]))),
    }).pipe(
      map((data): HomeState => ({ loading: false, data, error: '' })),
      startWith({ loading: true, data: null, error: '' } satisfies HomeState),
      catchError((err) => of({ loading: false, data: null, error: apiErrorMessage(err, 'The home page could not be loaded.') } satisfies HomeState)),
    )),
  ), { initialValue: { loading: true, data: null, error: '' } satisfies HomeState });

  readonly days = computed(() => tuesdayToSaturday(this.selected()));
  readonly allMatches = computed(() => this.state().data?.matches ?? []);
  readonly charts = computed(() => this.state().data?.charts ?? null);

  readonly liveMatches = computed(() => this.allMatches().filter((match) => isOnPitch(match)));
  readonly matchesToday = computed(() => this.allMatches().filter((match) => {
    const day = matchDay(match.date);
    return day != null && isSameDay(day, this.today);
  }));
  readonly upcomingWeek = computed(() => {
    const end = new Date(this.today);
    end.setDate(this.today.getDate() + 7);
    return this.allMatches().filter((match) => {
      const day = matchDay(match.date);
      return day != null && day >= this.today && day < end && !isFinished(match) && !isOnPitch(match);
    });
  });
  readonly rounds = computed(() => this.charts()?.goalsPerRound ?? []);
  readonly currentRound = computed(() => this.rounds().at(-1) ?? null);
  readonly played = computed(() => this.rounds().reduce((sum, round) => sum + round.matches, 0));
  readonly goals = computed(() => this.rounds().reduce((sum, round) => sum + round.goals, 0));
  readonly topFive = computed(() => (this.charts()?.teamResults ?? []).slice(0, 5));
  readonly goalLabels = computed(() => this.rounds().map((round) => round.roundName));
  readonly goalSeries = computed((): ChartSeries[] => [{
    label: 'Goals',
    data: this.rounds().map((round) => round.goals),
    color: '--ac',
  }]);

  readonly listed = computed(() => {
    const selected = this.selected();
    const next = new Date(selected);
    next.setDate(selected.getDate() + 1);
    return this.allMatches().filter((match) => {
      const day = matchDay(match.date);
      if (!day) return false;
      if (this.tab() === 'today') return isSameDay(day, selected);
      if (this.tab() === 'upcoming') return day >= next && !isFinished(match);
      return day < selected && isFinished(match);
    }).sort((left, right) => (left.date ?? '').localeCompare(right.date ?? ''));
  });

  readonly groups = computed(() => {
    const groups: { key: string; title: string; detail: string; slug: string | null; matches: MatchListDto[] }[] = [];
    for (const match of this.listed()) {
      const title = match.competitionName || 'Competition not set';
      const detail = [match.seasonName, match.roundName].filter(Boolean).join(' · ');
      const key = `${title}|${detail}`;
      const existing = groups.find((group) => group.key === key);
      if (existing) existing.matches.push(match);
      else groups.push({
        key,
        title,
        detail,
        slug: this.state().data?.competitions.find((item) => item.name === title)?.slug ?? null,
        matches: [match],
      });
    }
    return groups;
  });

  readonly matchdayLabel = computed(() => {
    const rounds = [...new Set(this.listed().map((match) => match.roundName).filter((name): name is string => !!name))];
    return rounds.length === 1 ? `Matchday · ${rounds[0]}` : 'Matchday';
  });

  constructor() {
    if (typeof matchMedia !== 'function') return;
    const query = matchMedia('(max-width: 768px)');
    this.narrow.set(query.matches);
    query.addEventListener('change', (event) => this.narrow.set(event.matches));
  }

  retry(): void {
    this.reload$.next();
  }

  selectDay(date: Date): void {
    this.selected.set(startOfDay(date));
    this.tab.set('today');
  }

  chooseDate(value: string): void {
    const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
    if (!match) return;
    this.selectDay(new Date(Number(match[1]), Number(match[2]) - 1, Number(match[3])));
  }

  selectionSlug(team: TeamResults): string | null {
    return this.state().data?.selections.find((item) => item.publicId === team.selectionPublicId)?.slug ?? null;
  }

  goalDifference(team: TeamResults): number {
    return team.goalsFor - team.goalsAgainst;
  }

  isSame(day: Date): boolean {
    return isSameDay(day, this.selected());
  }

  isRealToday(day: Date): boolean {
    return isSameDay(day, this.today);
  }

  iso(date: Date): string {
    return toIsoDate(date);
  }

  pitch(match: MatchListDto): 'live' | 'half-time' | null {
    return isOnPitch(match);
  }

  emptyTitle(): string {
    if (this.tab() === 'upcoming') return 'No upcoming matches';
    if (this.tab() === 'results') return 'No earlier results';
    return 'No matches on this day';
  }
}
