import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TokenService } from '../services/token.service';
import { UserStateService } from '../services/user-state.service';

export const adminGuard: CanActivateFn = () => {
  const token = inject(TokenService).getAccessToken();
  const user = inject(UserStateService);
  const router = inject(Router);
  return token && user.isAdmin() ? true : router.createUrlTree(['/']);
};
