import { NgTemplateOutlet } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { forkJoin, of } from 'rxjs';
import { catchError, map, startWith, switchMap } from 'rxjs/operators';
import { MatchService } from '../../core/services/match.service';
import { LineupService } from '../../core/services/lineup.service';
import { PlayerService } from '../../core/services/player.service';
import { FormationService } from '../../core/services/formation.service';
import { PositionService } from '../../core/services/position.service';
import { SubPositionService } from '../../core/services/subposition.service';
import { RefereeService } from '../../core/services/referee.service';
import { HistoryService } from '../../core/services/history.service';
import { fetchAllPages } from '../../core/helpers/paging.helper';
import { emptyPage } from '../../core/models/paged-result';
import { MatchDetailsDto } from '../../core/models/match';
import { LineupPlayer, SaveLineupPlayer } from '../../core/models/lineup';
import { PlayerSelectionHistory } from '../../core/models/history.models';
import { Formation, Position, SubPosition } from '../../core/models/formation.models';
import { Player, Referee } from '../../core/models/person.models';
import { apiErrorMessage } from '../../core/utils/api-error';
import { Breadcrumb, Crumb } from '../../shared/components/breadcrumb/breadcrumb';
import { Button } from '../../shared/components/button/button';
import { EmptyState } from '../../shared/components/empty-state/empty-state';
import { ErrorBanner } from '../../shared/components/error-banner/error-banner';
import { SectionCard } from '../../shared/components/section-card/section-card';

export interface LineupDraft {
  playerPublicId: string;
  name: string;
  selectionPublicId: string;
  included: boolean;
  isStarter: boolean;
  shirtNumber: number | null;
  positionPublicId: string;
  subPositionPublicId: string;
}

export interface LineupFormModel {
  match: MatchDetailsDto;
  refereePublicId: string;
  homeFormationPublicId: string;
  awayFormationPublicId: string;
  home: LineupDraft[];
  away: LineupDraft[];
  referees: Referee[];
  formations: Formation[];
  positions: Position[];
  subPositions: SubPosition[];
}

export function lineupErrors(rows: LineupDraft[]): string[] {
  const chosen = rows.filter((row) => row.included);
  const errors: string[] = [];
  if (new Set(chosen.map((row) => row.playerPublicId)).size !== chosen.length) {
    errors.push('A player can appear only once in the match.');
  }
  if (chosen.some((row) => row.shirtNumber != null && (row.shirtNumber < 1 || row.shirtNumber > 99))) {
    errors.push('Shirt numbers must be between 1 and 99.');
  }
  const shirts = new Set<string>();
  for (const row of chosen) {
    if (row.shirtNumber == null) continue;
    const key = `${row.selectionPublicId}:${row.shirtNumber}`;
    if (shirts.has(key)) errors.push('Shirt numbers must be unique for each team.');
    shirts.add(key);
  }
  const starters = new Map<string, number>();
  for (const row of chosen.filter((item) => item.isStarter)) {
    starters.set(row.selectionPublicId, (starters.get(row.selectionPublicId) ?? 0) + 1);
  }
  if ([...starters.values()].some((count) => count > 11)) {
    errors.push('A team can have at most 11 starters.');
  }
  return [...new Set(errors)];
}

@Component({
  selector: 'app-manage-lineup',
  imports: [NgTemplateOutlet, RouterLink, Breadcrumb, Button, EmptyState, ErrorBanner, SectionCard],
  templateUrl: './manage-lineup.html',
  styleUrls: ['../staff-form.css', './manage-lineup.css'],
})
export class ManageLineup {
  private route = inject(ActivatedRoute);
  private matches = inject(MatchService);
  private lineups = inject(LineupService);
  private players = inject(PlayerService);
  private formations = inject(FormationService);
  private positions = inject(PositionService);
  private subPositions = inject(SubPositionService);
  private referees = inject(RefereeService);
  private history = inject(HistoryService);

  saving = signal(false);
  error = signal<string | null>(null);
  saved = signal(false);
  form = signal<LineupFormModel | null>(null);

  /** undefined = loading, null = match not found. */
  readonly state = toSignal(
    this.route.paramMap.pipe(
      map((params) => params.get('publicId') ?? ''),
      switchMap((publicId) => this.matches.getByPublicId(publicId).pipe(
        switchMap((match) => forkJoin({
          match: of(match),
          lineup: this.lineups.get(match.publicId),
          players: fetchAllPages((page) => this.players.getAll(page, 100)),
          formations: fetchAllPages((page) => this.formations.getAll(page, 100)),
          positions: fetchAllPages((page) => this.positions.getAll(page, 100)),
          subPositions: fetchAllPages((page) => this.subPositions.getAll(page, 100)),
          referees: fetchAllPages((page) => this.referees.getAll(page, 100)),
          homeHistory: match.homeSelectionPublicId
            ? this.history.playerSelections(match.homeSelectionPublicId)
            : of(emptyPage<PlayerSelectionHistory>()),
          awayHistory: match.awaySelectionPublicId
            ? this.history.playerSelections(match.awaySelectionPublicId)
            : of(emptyPage<PlayerSelectionHistory>()),
        })),
        map((data) => {
          const saved = [...data.lineup.home, ...data.lineup.away];
          const model: LineupFormModel = {
            match: data.match,
            refereePublicId: data.lineup.refereePublicId ?? '',
            homeFormationPublicId: data.lineup.homeFormationPublicId ?? '',
            awayFormationPublicId: data.lineup.awayFormationPublicId ?? '',
            home: this.rows(data.homeHistory.items.map((item) => item.playerPublicId), data.match.homeSelectionPublicId, data.players, saved),
            away: this.rows(data.awayHistory.items.map((item) => item.playerPublicId), data.match.awaySelectionPublicId, data.players, saved),
            referees: data.referees,
            formations: data.formations,
            positions: data.positions,
            subPositions: data.subPositions,
          };
          this.form.set(model);
          return model;
        }),
        catchError(() => of(null as LineupFormModel | null)),
      )),
      startWith(undefined as LineupFormModel | null | undefined),
    ),
  );

  crumbs(match: MatchDetailsDto): Crumb[] {
    return [
      { label: 'Matches', route: '/matches' },
      { label: `${match.homeSelectionName ?? 'TBD'} vs ${match.awaySelectionName ?? 'TBD'}`, route: match.slug ? `/matches/${match.slug}` : undefined },
      { label: 'Lineup' },
    ];
  }

  subsFor(positionPublicId: string, all: SubPosition[]): SubPosition[] {
    return positionPublicId ? all.filter((item) => item.positionPublicId === positionPublicId) : [];
  }

  save(): void {
    const current = this.form();
    if (!current) return;
    const problems = lineupErrors([...current.home, ...current.away]);
    if (problems.length) {
      this.error.set(problems.join(' '));
      this.saved.set(false);
      return;
    }
    this.saving.set(true);
    this.error.set(null);
    const players = [...current.home, ...current.away].filter((row) => row.included).map((row) => this.toSave(row));
    this.lineups.assignReferee(current.match.publicId, current.refereePublicId || null).pipe(
      switchMap(() => this.lineups.save(current.match.publicId, {
        homeFormationPublicId: current.homeFormationPublicId || null,
        awayFormationPublicId: current.awayFormationPublicId || null,
        players,
      })),
    ).subscribe({
      next: () => {
        this.saving.set(false);
        this.saved.set(true);
      },
      error: (err) => {
        this.saving.set(false);
        this.saved.set(false);
        this.error.set(apiErrorMessage(err));
      },
    });
  }

  private rows(playerIds: (string | undefined)[], selectionPublicId: string | null, players: Player[], saved: LineupPlayer[]): LineupDraft[] {
    const names = new Map(players.filter((player) => player.publicId).map((player) => [player.publicId!, `${player.firstName} ${player.lastName}`]));
    const unique = [...new Set(playerIds.filter((id): id is string => !!id))];
    return unique.map((playerPublicId) => {
      const existing = saved.find((row) => row.playerPublicId === playerPublicId);
      return {
        playerPublicId,
        name: names.get(playerPublicId) ?? playerPublicId,
        selectionPublicId: selectionPublicId ?? '',
        included: !!existing,
        isStarter: existing?.isStarter ?? false,
        shirtNumber: existing?.shirtNumber ?? null,
        positionPublicId: existing?.positionPublicId ?? '',
        subPositionPublicId: existing?.subPositionPublicId ?? '',
      };
    });
  }

  private toSave(row: LineupDraft): SaveLineupPlayer {
    return {
      playerPublicId: row.playerPublicId,
      selectionPublicId: row.selectionPublicId,
      positionPublicId: row.positionPublicId || null,
      subPositionPublicId: row.subPositionPublicId || null,
      isStarter: row.isStarter,
      shirtNumber: row.shirtNumber,
    };
  }
}
