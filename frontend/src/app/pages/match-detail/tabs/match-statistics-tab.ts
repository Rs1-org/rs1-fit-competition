import { Component, computed, inject } from '@angular/core';
import { MatchStatisticsPanel } from '../../../components/match-statistics-panel/match-statistics-panel';
import { MatchPageStore } from '../match-page.store';

@Component({
  selector: 'app-match-statistics-tab',
  imports: [MatchStatisticsPanel],
  template: `@if (publicId()) { <app-match-statistics-panel [matchPublicId]="publicId()" /> }`,
})
export class MatchStatisticsTab {
  private store = inject(MatchPageStore);
  publicId = computed(() => this.store.match()?.publicId ?? '');
}
