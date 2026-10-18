import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AdminModeratorGuard } from './admin-moderator.guard';
import { TokenService } from '../services/token.service';
import { UserStateService } from '../services/user-state.service';
import { routes } from '../../app.routes';

describe('Statistic form access', () => {
  it.each([[null, false, false], ['token', false, false], ['token', true, true]] as const)(
    'requires authentication and a management role (%s, %s)', (token, canManage, expected) => {
      const navigate = vi.fn();
      TestBed.configureTestingModule({ providers: [AdminModeratorGuard,
        { provide: TokenService, useValue: { getAccessToken: () => token } },
        { provide: UserStateService, useValue: { canManagePersons: () => canManage } },
        { provide: Router, useValue: { navigate } },
      ] });
      expect(routes.find(route => route.path === 'manage-statistics/:matchPublicId')?.canActivate).toContain(AdminModeratorGuard);
      expect(TestBed.inject(AdminModeratorGuard).canActivate()).toBe(expected);
      if (!expected) expect(navigate).toHaveBeenCalledWith(['/']);
    },
  );
});
