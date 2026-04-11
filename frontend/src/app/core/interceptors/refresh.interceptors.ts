import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const refreshInterceptor: HttpInterceptorFn = (req, next) => {

  const authService = inject(AuthService);

  return next(req).pipe(
    catchError(err => {

      if (err.status !== 401) {
        return throwError(() => err);
      }

      return authService.refreshToken().pipe(
        switchMap(() => next(req))
      );
    })
  );
};