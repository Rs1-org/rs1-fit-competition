import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, map, of, startWith, switchMap } from 'rxjs';
import { ReviewMatchDetails } from '../../core/models/match-review';
import { MatchReviewService } from '../../core/services/match-review.service';
import { MatchStatisticsPanel } from '../../components/match-statistics-panel/match-statistics-panel';
import { AppDateTimePipe } from '../../shared/pipes/regional-format.pipes';
import { apiErrorMessage } from '../../core/utils/api-error';

interface MatchState { loading: boolean; data: ReviewMatchDetails | null; error: string; }
@Component({ selector: 'app-match-statistics', imports: [RouterLink, MatchStatisticsPanel, AppDateTimePipe],
  templateUrl: './match-statistics.html', styleUrl: './statistics.css' })
export class MatchStatistics {
  private readonly service = inject(MatchReviewService);
  private readonly route = inject(ActivatedRoute);
  private readonly refresh$ = new BehaviorSubject(0);
  readonly state = toSignal(combineLatest([this.route.paramMap, this.refresh$]).pipe(
    switchMap(([params]) => this.service.getMatch(params.get('publicId') ?? '').pipe(
      map((data): MatchState => ({ loading: false, data, error: '' })),
      startWith({ loading: true, data: null, error: '' } as MatchState),
      catchError(err => of({ loading: false, data: null, error: apiErrorMessage(err, 'The match could not be loaded.') } as MatchState)),
    )),
  ), { initialValue: { loading: true, data: null, error: '' } as MatchState });
  retry(): void { this.refresh$.next(this.refresh$.value + 1); }
}
