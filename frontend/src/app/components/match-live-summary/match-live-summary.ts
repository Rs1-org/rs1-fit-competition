import { Component, inject, Input, OnChanges, output, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, filter, map, merge, of, switchMap, tap, startWith } from 'rxjs';
import { ReviewMatchDetails } from '../../core/models/match-review';
import { MatchReviewService } from '../../core/services/match-review.service';
import { MatchLiveService } from '../../core/services/match-live.service';
import { RealtimeState } from '../../core/services/signalr.service';
import { AppDateTimePipe, AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { apiErrorMessage } from '../../core/utils/api-error';

export type MatchRefreshReason = 'match' | 'events' | 'deleted' | 'resync';
interface SummaryState { loading: boolean; data: ReviewMatchDetails | null; error: string; }
@Component({ selector: 'app-match-live-summary', imports: [AppDateTimePipe, AppNumberPipe],
  templateUrl: './match-live-summary.html', styleUrl: './match-live-summary.css' })
export class MatchLiveSummary implements OnChanges {
  @Input({ required: true }) matchPublicId = '';
  readonly changed = output<MatchRefreshReason>();
  private readonly matches = inject(MatchReviewService);
  private readonly live = inject(MatchLiveService);
  private readonly match$ = new BehaviorSubject('');
  private readonly retry$ = new BehaviorSubject(0);
  readonly connection = signal<RealtimeState>('connecting');
  readonly state = toSignal(this.match$.pipe(filter(Boolean), switchMap(id => {
    this.connection.set('connecting');
    const changes = this.live.watch(id).pipe(tap(event => {
      if (event.type === 'state') {
        this.connection.set(event.state);
        if (event.state === 'connected') this.changed.emit('resync');
      } else {
        this.connection.set('connected'); this.changed.emit(event.data.change);
      }
    }), filter(event => event.type === 'data' || event.state === 'connected'));
    return merge(this.retry$, changes).pipe(switchMap(() => this.matches.getMatch(id).pipe(
      map((data): SummaryState => ({ loading: false, data, error: '' })),
      catchError(err => of({ loading: false, data: null, error: apiErrorMessage(err, 'The current score could not be loaded.') } as SummaryState)),
    )), startWith({ loading: true, data: null, error: '' } as SummaryState));
  })), { initialValue: { loading: true, data: null, error: '' } as SummaryState });
  ngOnChanges(): void { this.match$.next(this.matchPublicId); }
  retry(): void { this.retry$.next(this.retry$.value + 1); }
}
