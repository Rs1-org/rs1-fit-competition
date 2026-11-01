import { Component, input } from '@angular/core';
import { Crest } from '../crest/crest';
import { ScoreDisplay } from '../score-display/score-display';
import { StatusBadge } from '../status-badge/status-badge';

@Component({
  selector: 'app-match-row',
  imports: [Crest, ScoreDisplay, StatusBadge],
  templateUrl: './match-row.html',
  styleUrl: './match-row.css',
})
export class MatchRow {
  status = input<string | null>(null);
  postponed = input(false);
  minute = input<string | number | null>(null);
  homeName = input<string | null>(null);
  awayName = input<string | null>(null);
  homeAbbreviation = input<string | null | undefined>(null);
  awayAbbreviation = input<string | null | undefined>(null);
  homeScore = input<number | null>(null);
  awayScore = input<number | null>(null);
  meta = input<string | null>(null);
  variant = input<'compact' | 'expanded' | 'mobile'>('compact');
  showFollow = input(false);

  homeWins(): boolean {
    const home = this.homeScore();
    const away = this.awayScore();
    return home !== null && away !== null && home > away;
  }

  awayWins(): boolean {
    const home = this.homeScore();
    const away = this.awayScore();
    return home !== null && away !== null && away > home;
  }
}
