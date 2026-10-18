import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, provideRouter, RouterStateSnapshot } from '@angular/router';
import { adminGuard } from './admin.guard';
import { TokenService } from '../services/token.service';
import { UserStateService } from '../services/user-state.service';

describe('Admin route access', () => {
  it.each([[null, false], ['token', false], ['token', true]] as const)(
    'requires a token and the Admin role (%s, %s)', (token, isAdmin) => {
      TestBed.configureTestingModule({ providers: [provideRouter([]),
        { provide: TokenService, useValue: { getAccessToken: () => token } },
        { provide: UserStateService, useValue: { isAdmin: () => isAdmin } },
      ] });
      const result = TestBed.runInInjectionContext(() => adminGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot));
      if (token && isAdmin) expect(result).toBe(true);
      else expect(result.toString()).toBe('/');
    });
});
