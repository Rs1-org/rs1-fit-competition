import { Component, DOCUMENT, ViewChild, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, of } from 'rxjs';
import { premiumPlans, PremiumPlanMonths } from '../../../core/models/payment';
import { PaymentService } from '../../../core/services/payment.service';
import { UserStateService } from '../../../core/services/user-state.service';
import { apiErrorMessage } from '../../../core/utils/api-error';
import { AuthModalComponent } from '../../../features/auth/auth-modal/auth-modal.component';
import { AppNumberPipe } from '../../pipes/regional-format.pipes';
import { Button } from '../button/button';
import { Icon } from '../icon/icon';
import { PremiumCard } from '../premium-card/premium-card';

@Component({ selector: 'app-paywall', imports: [AuthModalComponent, AppNumberPipe, Button, Icon, PremiumCard], templateUrl: './paywall.html', styleUrl: './paywall.css' })
export class Paywall {
  private readonly payments = inject(PaymentService);
  private readonly document = inject(DOCUMENT);
  readonly userState = inject(UserStateService);
  @ViewChild(AuthModalComponent) authModal?: AuthModalComponent;

  readonly plans = premiumPlans;
  readonly benefits = ['Team form over recent matches', 'Head-to-head history between teams', 'Player stats: goals and cards'];
  readonly months = signal<PremiumPlanMonths>(1);
  readonly pricing = toSignal(this.payments.getPricing().pipe(catchError(() => of(null))), { initialValue: null });
  readonly currency = computed(() => this.pricing()?.currency.toUpperCase() ?? '');
  readonly starting = signal(false);
  readonly error = signal('');

  price(months: number): number | null {
    const pricing = this.pricing();
    return pricing ? pricing.monthlyPriceCents * months / 100 : null;
  }

  goPremium(): void {
    if (!this.userState.isSignedIn()) { this.authModal?.openLogin(); return; }
    this.starting.set(true); this.error.set('');
    this.payments.checkout(this.months()).subscribe({
      next: session => this.document.location.assign(session.url),
      error: err => { this.starting.set(false); this.error.set(apiErrorMessage(err, 'The payment could not be started.')); },
    });
  }
}
