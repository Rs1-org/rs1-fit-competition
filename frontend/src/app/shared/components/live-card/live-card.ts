import { Component, input } from '@angular/core';
import { Crest } from '../crest/crest';

@Component({
  selector: 'app-live-card',
  imports: [Crest],
  templateUrl: './live-card.html',
  styleUrl: './live-card.css',
})
export class LiveCard {
  competition = input<string | null>(null);
  minute = input<string | number | null>(null);
  homeName = input<string | null>(null);
  awayName = input<string | null>(null);
  homeAbbreviation = input<string | null | undefined>(null);
  awayAbbreviation = input<string | null | undefined>(null);
  homeScore = input<number | null>(null);
  awayScore = input<number | null>(null);
  lastEvent = input<string | null>(null);
  halfHome = input<number | null>(null);
  halfAway = input<number | null>(null);
}
