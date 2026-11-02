import { Component, computed, inject } from '@angular/core';
import { MatchReviewPanel } from '../../../components/match-review-panel/match-review-panel';
import { MatchPageStore } from '../match-page.store';

@Component({
  selector: 'app-match-reviews-tab',
  imports: [MatchReviewPanel],
  template: `@if (publicId()) { <app-match-review-panel [matchPublicId]="publicId()" /> }`,
})
export class MatchReviewsTab {
  private store = inject(MatchPageStore);
  publicId = computed(() => this.store.match()?.publicId ?? '');
}
