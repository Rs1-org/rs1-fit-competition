import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, forkJoin, map, of, startWith, switchMap } from 'rxjs';
import { SelectionOption, SelectionService } from '../../core/services/selection.service';
import { UserStateService } from '../../core/services/user-state.service';
import { apiErrorMessage } from '../../core/utils/api-error';
import { AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { FormChips } from '../../shared/components/form-chips/form-chips';
import { Breadcrumb } from '../../shared/components/breadcrumb/breadcrumb';
import { Crest } from '../../shared/components/crest/crest';
import { StatTile } from '../../shared/components/stat-tile/stat-tile';
import { SectionCard } from '../../shared/components/section-card/section-card';
import { EmptyState } from '../../shared/components/empty-state/empty-state';
import { Skeleton } from '../../shared/components/skeleton/skeleton';
import { ErrorBanner } from '../../shared/components/error-banner/error-banner';
import { PlayerCard } from '../../shared/components/player-card/player-card';
import { MatchRow } from '../../shared/components/match-row/match-row';
import { crestColour } from '../../shared/match/match-display';
import { FormResult, matchPhase } from '../../shared/match/match-display';
import { HistoryService } from '../../core/services/history.service';
import { PlayerService } from '../../core/services/player.service';
import { ManagerService } from '../../core/services/manager.service';
import { MatchService } from '../../core/services/match.service';
import { MatchListDto } from '../../core/models/match';
import { Player } from '../../core/models/person.models';
import { pictureToDataUrl } from '../../core/models/person.models';
import { emptyPage } from '../../core/models/paged-result';
import { PlayerSelectionHistory, SelectionManagerHistory } from '../../core/models/history.models';

export interface SquadPlayer {
  id: string;
  name: string;
  position: string;
  number: number | null;
  age: number | null;
  value: string | null;
  picture: string | null;
}

export interface SquadGroup {
  position: string;
  players: SquadPlayer[];
}

export interface ManagerLine {
  id: string;
  name: string;
  wins: number | null;
  draws: number | null;
  losses: number | null;
}

interface DetailState {
  data: SelectionOption | null;
  loading: boolean;
  error: string;
  squad: SquadGroup[];
  goals: number | null;
  managers: ManagerLine[];
  matches: MatchListDto[];
  form: FormResult[];
}

const emptyDetail = (loading: boolean, error = '', data: SelectionOption | null = null): DetailState => ({
  data,
  loading,
  error,
  squad: [],
  goals: null,
  managers: [],
  matches: [],
  form: [],
});

@Component({
  selector: 'app-selection-detail',
  imports: [
    RouterLink,
    AppNumberPipe,
    FormChips,
    Breadcrumb,
    Crest,
    StatTile,
    SectionCard,
    EmptyState,
    Skeleton,
    ErrorBanner,
    PlayerCard,
    MatchRow,
  ],
  templateUrl: './selection-detail.html',
  styleUrl: './selection-detail.css',
})
export class SelectionDetail {
  private readonly service = inject(SelectionService);
  private readonly history = inject(HistoryService);
  private readonly players = inject(PlayerService);
  private readonly managers = inject(ManagerService);
  private readonly matches = inject(MatchService);
  private readonly route = inject(ActivatedRoute);
  private readonly refresh$ = new BehaviorSubject<void>(undefined);
  readonly user = inject(UserStateService);
  readonly state = toSignal(
    combineLatest([this.route.paramMap, this.refresh$]).pipe(
      switchMap(([params]) =>
        this.service.getBySlug(params.get('slug') ?? '').pipe(
          switchMap((data) => this.loadRelated(data)),
          startWith(emptyDetail(true)),
          catchError((err) => of(emptyDetail(false, apiErrorMessage(err, 'Selection could not be loaded.')))),
        ),
      ),
    ),
    { initialValue: emptyDetail(true) },
  );

  retry(): void {
    this.refresh$.next();
  }

  heroColour(selection: SelectionOption): string {
    return crestColour(selection.abbreviation || selection.name);
  }

  private loadRelated(data: SelectionOption) {
    if (!data.publicId) return of(emptyDetail(false, '', data));
    return forkJoin({
      players: this.history.playerSelections(data.publicId).pipe(catchError(() => of(emptyPage<PlayerSelectionHistory>()))),
      managers: this.history.managerSelections(data.publicId).pipe(catchError(() => of(emptyPage<SelectionManagerHistory>()))),
      matches: data.publicId
        ? this.matches.getAll(1, 100, { selectionPublicId: data.publicId }).pipe(catchError(() => of(emptyPage<MatchListDto>())))
        : of(emptyPage<MatchListDto>()),
    }).pipe(
      switchMap(({ players, managers, matches }) => {
        const playerIds = [...new Set(players.items.map((row) => row.playerPublicId).filter((id): id is string => !!id))];
        const managerIds = [...new Set(managers.items.map((row) => row.managerPublicId).filter((id): id is string => !!id))];
        return forkJoin({
          people: playerIds.length
            ? forkJoin(playerIds.map((id) => this.players.getById(id).pipe(catchError(() => of(null)))))
            : of([] as (Player | null)[]),
          staff: managerIds.length
            ? forkJoin(managerIds.map((id) => this.managers.getById(id).pipe(catchError(() => of(null)))))
            : of([] as ({ publicId?: string; firstName: string; lastName: string } | null)[]),
        }).pipe(
          map(({ people, staff }) => {
            const byPlayer = new Map(people.filter((person): person is Player => !!person?.publicId).map((person) => [person.publicId!, person]));
            const byManager = new Map(
              staff.filter((person): person is { publicId?: string; firstName: string; lastName: string } => !!person?.publicId).map((person) => [person.publicId!, person]),
            );
            const squadRows = players.items.flatMap((row) => {
              const person = row.playerPublicId ? byPlayer.get(row.playerPublicId) : undefined;
              if (!person?.publicId) return [];
              return [{
                id: person.publicId,
                name: `${person.firstName} ${person.lastName}`.trim(),
                position: person.positionName || 'Unassigned',
                number: row.shirtNumber ?? person.shirtNumber ?? null,
                age: ageFrom(person.dateOfBirth),
                value: person.transferValue == null ? null : null,
                rawValue: person.transferValue,
                picture: pictureToDataUrl(person.picture),
              }];
            });
            const groups = new Map<string, SquadPlayer[]>();
            for (const row of squadRows) {
              const player: SquadPlayer = {
                id: row.id,
                name: row.name,
                position: row.position,
                number: row.number,
                age: row.age,
                value: null,
                picture: row.picture,
              };
              player.value = row.rawValue == null ? null : String(row.rawValue);
              const list = groups.get(row.position) ?? [];
              list.push(player);
              groups.set(row.position, list);
            }
            const knownGoals = players.items.map((row) => row.goals).filter((goals): goals is number => goals != null);
            const managerLines = managers.items.flatMap((row) => {
              const person = row.managerPublicId ? byManager.get(row.managerPublicId) : undefined;
              if (!person?.publicId) return [];
              return [{
                id: person.publicId,
                name: `${person.firstName} ${person.lastName}`.trim(),
                wins: row.wins,
                draws: row.draw,
                losses: row.loses,
              }];
            });
            return {
              data,
              loading: false,
              error: '',
              squad: [...groups.entries()]
                .sort(([left], [right]) => positionRank(left) - positionRank(right) || left.localeCompare(right))
                .map(([position, players]) => ({ position, players })),
              goals: knownGoals.length ? knownGoals.reduce((sum, goals) => sum + goals, 0) : null,
              managers: managerLines,
              matches: [...matches.items].reverse(),
              form: recentForm(matches.items, data.name),
            } satisfies DetailState;
          }),
        );
      }),
    );
  }
}

function ageFrom(date: string | null | undefined): number | null {
  if (!date) return null;
  const born = new Date(date);
  if (Number.isNaN(born.getTime())) return null;
  const now = new Date();
  let age = now.getFullYear() - born.getFullYear();
  const month = now.getMonth() - born.getMonth();
  if (month < 0 || (month === 0 && now.getDate() < born.getDate())) age -= 1;
  return age >= 0 ? age : null;
}

function positionRank(position: string): number {
  const value = position.toLowerCase();
  if (value.includes('goal')) return 0;
  if (value.includes('def')) return 1;
  if (value.includes('mid')) return 2;
  if (value.includes('forw') || value.includes('attack') || value.includes('strik')) return 3;
  if (value === 'unassigned') return 9;
  return 4;
}

function recentForm(matches: MatchListDto[], teamName: string): FormResult[] {
  const finished = matches.filter(
    (match) =>
      matchPhase(match.statusName, match.isPostponed) === 'finished' &&
      match.homeScore !== null &&
      match.awayScore !== null &&
      (match.homeSelectionName === teamName || match.awaySelectionName === teamName),
  );
  return finished.slice(-5).map((match) => {
    const home = match.homeSelectionName === teamName;
    const ours = home ? match.homeScore! : match.awayScore!;
    const theirs = home ? match.awayScore! : match.homeScore!;
    if (ours > theirs) return 'W';
    if (ours < theirs) return 'L';
    return 'D';
  });
}
