import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { catchError, map, Observable, of, startWith, switchMap } from 'rxjs';
import { HeadToHead } from '../../../core/models/statistic';
import { StatisticService } from '../../../core/services/statistic.service';
import { UserStateService } from '../../../core/services/user-state.service';
import { apiErrorMessage } from '../../../core/utils/api-error';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { ErrorBanner } from '../../../shared/components/error-banner/error-banner';
import { Paywall } from '../../../shared/components/paywall/paywall';
import { SectionCard } from '../../../shared/components/section-card/section-card';
import { Skeleton } from '../../../shared/components/skeleton/skeleton';
import { AppDatePipe } from '../../../shared/pipes/regional-format.pipes';
import { MatchPageStore } from '../match-page.store';

interface H2hState { loading: boolean; data: HeadToHead | null; error: string; }

@Component({
  selector: 'app-match-head-to-head-tab',
  imports: [AppDatePipe, EmptyState, ErrorBanner, Paywall, SectionCard, Skeleton],
  templateUrl: './match-head-to-head-tab.html',
  styleUrl: './match-head-to-head-tab.css',
})
export class MatchHeadToHeadTab {
  private store = inject(MatchPageStore);
  private statistics = inject(StatisticService);
  private userState = inject(UserStateService);

  // The backend has the last word: an expired premium answers 403.
  private forbidden = signal(false);
  unlocked = computed(() => this.userState.isPremium() && !this.forbidden());
  private retries = signal(0);

  private request = computed(() => {
    const match = this.store.match();
    const ready = this.unlocked() && match?.homeSelectionPublicId && match.awaySelectionPublicId;
    return { home: ready ? match.homeSelectionPublicId : null, away: ready ? match.awaySelectionPublicId : null, retry: this.retries() };
  });
  teamsKnown = computed(() => {
    const match = this.store.match();
    return !!match?.homeSelectionPublicId && !!match.awaySelectionPublicId;
  });

  state = toSignal(
    toObservable(this.request).pipe(
      switchMap(({ home, away }): Observable<H2hState> => {
        if (!home || !away) return of({ loading: false, data: null, error: '' });
        return this.statistics.getHeadToHead(home, away, {}).pipe(
          map((data) => ({ loading: false, data, error: '' })),
          startWith({ loading: true, data: null, error: '' }),
          catchError((err) => {
            if (err instanceof HttpErrorResponse && err.status === 403) this.forbidden.set(true);
            return of({ loading: false, data: null, error: apiErrorMessage(err, 'Head-to-head could not be loaded.') });
          }),
        );
      }),
    ),
    { initialValue: { loading: true, data: null, error: '' } as H2hState },
  );

  retry(): void {
    this.retries.update((value) => value + 1);
  }
}
