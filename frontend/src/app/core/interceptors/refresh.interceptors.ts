import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { TokenService } from '../services/token.service';
import { environment } from '../../../environments/environment';

export const refreshInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const tokenService = inject(TokenService);

  return next(req).pipe(
    catchError((err) => {
      if (
        err.status !== 401 ||
        !tokenService.getRefreshToken() ||
        req.url.startsWith(`${environment.apiUrl}/api/auth/`)
      ) {
        return throwError(() => err);
      }

      return authService.refreshToken().pipe(
        switchMap(() => {
          const token = tokenService.getAccessToken();
          if (!token) return throwError(() => err);
          return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
        }),
      );
    }),
  );
};
