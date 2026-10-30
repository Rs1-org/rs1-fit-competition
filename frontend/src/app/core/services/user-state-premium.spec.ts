import { UserDto } from '../models/auth.models';
import { UserStateService } from './user-state.service';
import { TestBed } from '@angular/core/testing';

describe('UserStateService premium', () => {
  const user = (premiumUntil?: string | null): UserDto =>
    ({ id: '1', email: 'a@example.invalid', firstName: 'A', lastName: 'B', phoneNumber: '', premiumUntil });
  const inDays = (days: number) => new Date(Date.now() + days * 86_400_000).toISOString();
  let service: UserStateService;

  beforeEach(() => { localStorage.clear(); service = TestBed.inject(UserStateService); });
  afterEach(() => service.clearUser());

  it('is premium only until the expiry', () => {
    service.setUser(user(inDays(1))); expect(service.isPremium()).toBe(true);
    service.setUser(user(inDays(-1))); expect(service.isPremium()).toBe(false);
    service.setUser(user(null)); expect(service.isPremium()).toBe(false);
  });

  it('knows when nobody is signed in', () => {
    expect(service.isSignedIn()).toBe(false); expect(service.isPremium()).toBe(false);
    service.setUser(user()); expect(service.isSignedIn()).toBe(true);
  });
});
