import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { TokenService } from '../services/token.service';
import { UserStateService } from '../services/user-state.service';

@Injectable({ providedIn: 'root' })
export class AdminModeratorGuard implements CanActivate {
  constructor(
    private tokenService: TokenService,
    private userState: UserStateService,
    private router: Router,
  ) {}

  canActivate(): boolean {
    if (!this.tokenService.getAccessToken()) {
      this.router.navigate(['/']);
      return false;
    }

    if (!this.userState.canManagePersons()) {
      this.router.navigate(['/']);
      return false;
    }

    return true;
  }
}
