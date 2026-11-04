import { registerLocaleData } from '@angular/common';
import localeBs from '@angular/common/locales/bs-Latn';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { PremiumStatus } from '../../core/models/payment';
import { AuthService } from '../../core/services/auth.service';
import { PaymentService } from '../../core/services/payment.service';
import { POLL_ATTEMPTS, POLL_INTERVAL_MS, PremiumResult } from './premium-result';

// The success card shows the end date through the regional appDate pipe.
registerLocaleData(localeBs);

describe('PremiumResult', () => {
  const getStatus = vi.fn();
  const status = (latestPaymentStatus: PremiumStatus['latestPaymentStatus']): PremiumStatus =>
    ({ isPremium: latestPaymentStatus === 'Paid', premiumUntil: '2026-12-01T00:00:00Z', latestPaymentStatus });

  function render() {
    TestBed.configureTestingModule({ providers: [provideRouter([]),
      { provide: PaymentService, useValue: { getStatus } },
      { provide: AuthService, useValue: { refreshToken: () => of(null) } },
    ] });
    const fixture = TestBed.createComponent(PremiumResult);
    vi.advanceTimersByTime(POLL_INTERVAL_MS * POLL_ATTEMPTS);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  }

  beforeEach(() => { vi.useFakeTimers(); getStatus.mockReset(); });
  afterEach(() => vi.useRealTimers());

  it('shows the success card once the payment is paid', () => {
    getStatus.mockReturnValue(of(status('Paid')));
    const element = render();
    expect(element.querySelector('h2')?.textContent).toContain('Payment received');
    expect(element.querySelector('.tone-success')).toBeTruthy();
  });

  it('shows the failure card in the same card style', () => {
    getStatus.mockReturnValue(of(status('Expired')));
    const element = render();
    expect(element.querySelector('h2')?.textContent).toContain('Payment not completed');
    expect(element.querySelector('.tone-danger')).toBeTruthy();
    expect(element.querySelector('[role="alert"]')).toBeTruthy();
  });

  it('tells the user to check back when confirmation times out', () => {
    getStatus.mockReturnValue(of(status('Pending')));
    expect(render().querySelector('h2')?.textContent).toContain('Still confirming');
  });
});
