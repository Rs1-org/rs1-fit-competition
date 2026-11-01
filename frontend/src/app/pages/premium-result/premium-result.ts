import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { catchError, concatMap, first, of, switchMap, take, throwError, timer } from 'rxjs';
import { PremiumStatus } from '../../core/models/payment';
import { AuthService } from '../../core/services/auth.service';
import { PaymentService } from '../../core/services/payment.service';
import { AppDatePipe } from '../../shared/pipes/regional-format.pipes';

export const POLL_INTERVAL_MS = 2000;
export const POLL_ATTEMPTS = 15;

type ResultState = 'waiting' | 'paid' | 'failed' | 'timeout';

/** Stripe redirects here before the webhook may have landed, so wait for it. */
@Component({ selector: 'app-premium-result', imports: [RouterLink, AppDatePipe], templateUrl: './premium-result.html', styleUrl: '../statistics/statistics.css' })
export class PremiumResult {
  private readonly payments = inject(PaymentService);
  private readonly auth = inject(AuthService);
  readonly state = signal<ResultState>('waiting');
  readonly premiumUntil = signal<string | null>(null);

  constructor() {
    timer(0, POLL_INTERVAL_MS).pipe(
      take(POLL_ATTEMPTS),
      concatMap(() => this.payments.getStatus().pipe(catchError(() => of(null)))),
      first((status): status is PremiumStatus => !!status && status.latestPaymentStatus !== 'Pending'),
      switchMap(status => status.latestPaymentStatus === 'Paid'
        // A new token carries the premium claim and refreshes the stored user.
        ? this.auth.refreshToken().pipe(switchMap(() => of(status)))
        : throwError(() => new Error('not paid'))),
      takeUntilDestroyed(inject(DestroyRef)),
    ).subscribe({
      next: status => { this.premiumUntil.set(status.premiumUntil); this.state.set('paid'); },
      error: err => this.state.set(err?.name === 'EmptyError' ? 'timeout' : 'failed'),
    });
  }
}
