import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TeamResults } from '../../../core/models/statistic';
import { AppNumberPipe } from '../../pipes/regional-format.pipes';
import { FormResult } from '../../match/match-display';
import { Crest } from '../crest/crest';
import { FormChips } from '../form-chips/form-chips';

/** What the table needs to know about a team besides its results. */
export interface LeagueTeam { publicId: string; slug: string; abbreviation?: string | null; }

export type PositionZone = 'champions' | 'conference' | 'relegation';

/** Zones need a real league; smaller tables get no colours. */
const MIN_TEAMS_FOR_ZONES = 6;

/** Default zones (as in the mockup): 1st Champions League, 2nd–3rd Conference League, last two relegated. */
export function positionZone(index: number, total: number): PositionZone | null {
  if (total < MIN_TEAMS_FOR_ZONES) return null;
  if (index === 0) return 'champions';
  if (index <= 2) return 'conference';
  if (index >= total - 2) return 'relegation';
  return null;
}

@Component({
  selector: 'app-league-table',
  imports: [RouterLink, AppNumberPipe, Crest, FormChips],
  templateUrl: './league-table.html',
  styleUrl: './league-table.css',
})
export class LeagueTable {
  /** Rows in table order (the backend sorts by points, goal difference, goals scored). */
  rows = input.required<TeamResults[]>();
  /** Used for team links and crests; a team missing here is shown without a link. */
  teams = input<LeagueTeam[]>([]);
  /** Last results per team publicId, oldest first. Leave null to hide the Form column. */
  form = input<Record<string, FormResult[]> | null>(null);
  /** Colour the qualification and relegation places and show the legend. */
  showZones = input(false);
  /** Team to highlight, e.g. the one the user follows. */
  highlight = input<string | null>(null);

  private teamsById = computed(() => new Map(this.teams().map((team) => [team.publicId, team])));
  hasZones = computed(() => this.showZones() && this.rows().length >= MIN_TEAMS_FOR_ZONES);

  team(row: TeamResults): LeagueTeam | undefined {
    return this.teamsById().get(row.selectionPublicId);
  }

  zone(index: number): PositionZone | null {
    return this.hasZones() ? positionZone(index, this.rows().length) : null;
  }

  goalDifference(row: TeamResults): number {
    return row.goalsFor - row.goalsAgainst;
  }

  results(row: TeamResults): FormResult[] {
    return this.form()?.[row.selectionPublicId] ?? [];
  }
}
