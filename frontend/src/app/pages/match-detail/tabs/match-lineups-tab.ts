import { NgTemplateOutlet } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { LineupPlayer } from '../../../core/models/lineup';
import { AppNumberPipe } from '../../../shared/pipes/regional-format.pipes';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { SectionCard } from '../../../shared/components/section-card/section-card';
import { Skeleton } from '../../../shared/components/skeleton/skeleton';
import { MatchPageStore } from '../match-page.store';

@Component({
  selector: 'app-match-lineups-tab',
  imports: [NgTemplateOutlet, AppNumberPipe, EmptyState, SectionCard, Skeleton],
  templateUrl: './match-lineups-tab.html',
  styleUrl: './match-lineups-tab.css',
})
export class MatchLineupsTab {
  private store = inject(MatchPageStore);
  match = this.store.match;
  lineup = this.store.lineup;
  announced = computed(() => {
    const sheet = this.lineup();
    return !!sheet && sheet.home.length + sheet.away.length > 0;
  });

  starters(players: LineupPlayer[]): LineupPlayer[] {
    return players.filter((player) => player.isStarter);
  }

  substitutes(players: LineupPlayer[]): LineupPlayer[] {
    return players.filter((player) => !player.isStarter);
  }
}
