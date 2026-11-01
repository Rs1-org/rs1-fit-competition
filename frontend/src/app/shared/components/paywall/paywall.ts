import { Component, DOCUMENT, ViewChild, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, of } from 'rxjs';
import { premiumPlans, PremiumPlanMonths } from '../../../core/models/payment';
import { PaymentService } from '../../../core/services/payment.service';
import { UserStateService } from '../../../core/services/user-state.service';
import { apiErrorMessage } from '../../../core/utils/api-error';
import { AuthModalComponent } from '../../../features/auth/auth-modal/auth-modal.component';
import { AppNumberPipe } from '../../pipes/regional-format.pipes';

@Component({ selector: 'app-paywall', imports: [AuthModalComponent, AppNumberPipe], templateUrl: './paywall.html', styleUrl: './paywall.css' })
export class Paywall {
  private readonly payments = inject(PaymentService);
  private readonly document = inject(DOCUMENT);
  readonly userState = inject(UserStateService);
  @ViewChild(AuthModalComponent) authModal?: AuthModalComponent;

  readonly plans = premiumPlans;
  readonly months = signal<PremiumPlanMonths>(1);
  readonly pricing = toSignal(this.payments.getPricing().pipe(catchError(() => of(null))), { initialValue: null });
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
