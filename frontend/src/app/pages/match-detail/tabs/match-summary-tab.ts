import { Component, computed, inject } from '@angular/core';
import { MatchEvents } from '../../match-events/match-events';
import { MatchPageStore } from '../match-page.store';

@Component({
  selector: 'app-match-summary-tab',
  imports: [MatchEvents],
  template: `@if (publicId()) { <app-match-events [matchPublicId]="publicId()" [refresh]="store.version()" /> }`,
})
export class MatchSummaryTab {
  store = inject(MatchPageStore);
  publicId = computed(() => this.store.match()?.publicId ?? '');
}
