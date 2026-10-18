import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, map, of, startWith, switchMap } from 'rxjs';
import { MatchReviewService } from '../../core/services/match-review.service';
import { ReviewMatch } from '../../core/models/match-review';
import { MatchLiveSummary } from '../../components/match-live-summary/match-live-summary';
import { MatchReviewPanel } from '../../components/match-review-panel/match-review-panel';
import { MatchStatisticsPanel } from '../../components/match-statistics-panel/match-statistics-panel';
import { apiErrorMessage } from '../../core/utils/api-error';

interface MatchState { loading: boolean; data: ReviewMatch | null; error: string; }
@Component({ selector: 'app-match-reviews', imports: [RouterLink, MatchLiveSummary, MatchReviewPanel, MatchStatisticsPanel],
  templateUrl: './match-reviews.html', styleUrl: './reviews.css' })
export class MatchReviews {
  private readonly service = inject(MatchReviewService);
  private readonly route = inject(ActivatedRoute);
  private readonly refresh$ = new BehaviorSubject<void>(undefined);
  readonly state = toSignal(combineLatest([this.route.paramMap, this.refresh$]).pipe(
    switchMap(([params]) => this.service.getMatch(params.get('publicId') ?? '').pipe(
      map((data): MatchState => ({ loading: false, data, error: '' })),
      startWith({ loading: true, data: null, error: '' } as MatchState),
      catchError(err => of({ loading: false, data: null, error: apiErrorMessage(err, 'The match could not be loaded.') } as MatchState)),
    )),
  ), { initialValue: { loading: true, data: null, error: '' } as MatchState });
  retry(): void { this.refresh$.next(); }
}
