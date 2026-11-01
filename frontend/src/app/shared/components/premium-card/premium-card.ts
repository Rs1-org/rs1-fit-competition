import { Component, input } from '@angular/core';
import { Icon, IconName } from '../icon/icon';

export type PremiumCardTone = 'premium' | 'success' | 'danger';

/** Card shared by the paywall and the payment result page. */
@Component({
  selector: 'app-premium-card',
  imports: [Icon],
  templateUrl: './premium-card.html',
  styleUrl: './premium-card.css',
})
export class PremiumCard {
  heading = input.required<string>();
  icon = input.required<IconName>();
  tone = input<PremiumCardTone>('premium');
}
