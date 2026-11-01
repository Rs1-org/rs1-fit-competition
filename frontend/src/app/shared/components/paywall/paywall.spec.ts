import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { of, throwError } from 'rxjs';
import { PaymentService } from '../../../core/services/payment.service';
import { UserStateService } from '../../../core/services/user-state.service';
import { AuthModalService } from '../../../core/services/auth-modal.service';
import { Paywall } from './paywall';

describe('Paywall', () => {
  const checkout = vi.fn(); const isSignedIn = signal(true);
  const openLogin = vi.fn();

  function create(): Paywall {
    TestBed.configureTestingModule({ providers: [
      { provide: PaymentService, useValue: { checkout, getPricing: () => of({ monthlyPriceCents: 499, currency: 'eur' }) } },
      { provide: UserStateService, useValue: { isSignedIn } },
      { provide: AuthModalService, useValue: { openLogin } },
    ] });
    return TestBed.runInInjectionContext(() => new Paywall());
  }

  beforeEach(() => { isSignedIn.set(true); checkout.mockReset(); openLogin.mockReset(); });

  it('shows the price from the API, not a fixed one', () => {
    const paywall = create();
    expect(paywall.price(1)).toBe(4.99);
    expect(paywall.price(3)).toBeCloseTo(14.97);
    expect(paywall.currency()).toBe('EUR');
  });

  it('opens login for a visitor instead of starting checkout', () => {
    isSignedIn.set(false);
    const paywall = create();
    paywall.goPremium();
    expect(openLogin).toHaveBeenCalledOnce();
    expect(checkout).not.toHaveBeenCalled();
  });

  it('starts checkout with the selected plan and recovers from an error', () => {
    checkout.mockReturnValue(throwError(() => new Error('offline')));
    const paywall = create();
    paywall.months.set(12);
    paywall.goPremium();
    expect(checkout).toHaveBeenCalledWith(12);
    expect(paywall.starting()).toBe(false);
    expect(paywall.error()).toBeTruthy();
  });
});
