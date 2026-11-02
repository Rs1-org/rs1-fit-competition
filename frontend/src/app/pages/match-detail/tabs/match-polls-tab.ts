import { Component, computed, inject } from '@angular/core';
import { MatchPolls } from '../../../components/match-polls/match-polls';
import { MatchPageStore } from '../match-page.store';

@Component({
  selector: 'app-match-polls-tab',
  imports: [MatchPolls],
  template: `@if (publicId()) { <app-match-polls [matchPublicId]="publicId()" /> }`,
})
export class MatchPollsTab {
  private store = inject(MatchPageStore);
  publicId = computed(() => this.store.match()?.publicId ?? '');
}
