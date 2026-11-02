import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { MatchService } from '../../core/services/match.service';

/** Old match pages carried the publicId; sends them to the right tab of the match page. */
export function redirectToMatchTab(tab = ''): CanActivateFn {
  return (route) => {
    const router = inject(Router);
    return inject(MatchService).getByPublicId(route.paramMap.get('publicId') ?? '').pipe(
      map((match) => router.createUrlTree(['/matches', match.slug, ...(tab ? [tab] : [])])),
      catchError(() => of(router.createUrlTree(['/matches']))),
    );
  };
}
