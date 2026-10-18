import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, map, of, startWith, switchMap } from 'rxjs';
import { MatchReviewService } from '../../core/services/match-review.service';
import { ReviewMatch } from '../../core/models/match-review';
import { PagedResult } from '../../core/models/paged-result';
import { Pagination } from '../../shared/components/pagination/pagination';
import { AppDateTimePipe } from '../../shared/pipes/regional-format.pipes';
import { apiErrorMessage } from '../../core/utils/api-error';

interface MatchesState { loading: boolean; data: PagedResult<ReviewMatch> | null; error: string; }
@Component({ selector: 'app-review-matches', imports: [RouterLink, Pagination, AppDateTimePipe],
  templateUrl: './review-matches.html', styleUrl: './reviews.css' })
export class ReviewMatches {
  private readonly service = inject(MatchReviewService);
  private readonly page$ = new BehaviorSubject(1);
  readonly state = toSignal(this.page$.pipe(switchMap(page => this.service.getMatches(page).pipe(
    map((data): MatchesState => ({ loading: false, data, error: '' })),
    startWith({ loading: true, data: null, error: '' } as MatchesState),
    catchError(err => of({ loading: false, data: null, error: apiErrorMessage(err, 'Matches could not be loaded.') } as MatchesState)),
  ))), { initialValue: { loading: true, data: null, error: '' } as MatchesState });
  page(page: number): void { this.page$.next(page); }
  retry(): void { this.page$.next(this.page$.value); }
}
