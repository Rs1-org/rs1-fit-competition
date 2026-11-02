import { computed, inject, Injectable, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { catchError, EMPTY, filter, map, Observable, of, startWith, switchMap, tap } from 'rxjs';
import { emptyLineup, MatchLineup } from '../../core/models/lineup';
import { MatchDetailsDto } from '../../core/models/match';
import { MatchEvent } from '../../core/models/match-event';
import { fetchAllPages } from '../../core/helpers/paging.helper';
import { LineupService } from '../../core/services/lineup.service';
import { MatchEventService } from '../../core/services/match-event.service';
import { MatchLiveService } from '../../core/services/match-live.service';
import { MatchService } from '../../core/services/match.service';
import { RealtimeState } from '../../core/services/signalr.service';

/** One match shared by the page header and its tabs, kept current over the match hub. */
@Injectable()
export class MatchPageStore {
  private route = inject(ActivatedRoute);
  private matches = inject(MatchService);
  private lineups = inject(LineupService);
  private events = inject(MatchEventService);
  private live = inject(MatchLiveService);

  readonly connection = signal<RealtimeState>('connecting');
  /** Goes up on every change from the hub, so tabs can reload their own data. */
  readonly version = signal(0);

  /** undefined = loading, null = not found. */
  readonly match = toSignal(
    this.route.paramMap.pipe(
      map((params) => params.get('slug') ?? ''),
      switchMap((slug) =>
        this.matches.getBySlug(slug).pipe(
          catchError(() => of(null)),
          switchMap((first): Observable<MatchDetailsDto | null> => {
            if (!first) return of(null);
            // A failed refresh keeps the last known match on screen.
            return this.changes(first.publicId).pipe(
              switchMap(() => this.matches.getBySlug(slug).pipe(catchError(() => EMPTY))),
              startWith(first),
            );
          }),
        ),
      ),
    ),
    { initialValue: undefined },
  );

  private loadKey = computed(() => {
    const match = this.match();
    return match ? `${match.publicId}|${this.version()}` : '';
  });
  private loaded$ = toObservable(this.loadKey).pipe(map((key) => key.split('|')[0]));

  readonly lineup = toSignal(
    this.loaded$.pipe(
      switchMap((id): Observable<MatchLineup | null> =>
        id ? this.lineups.get(id).pipe(catchError(() => of(emptyLineup(id)))) : of(null),
      ),
    ),
    { initialValue: null },
  );

  readonly matchEvents = toSignal(
    this.loaded$.pipe(
      switchMap((id): Observable<MatchEvent[]> =>
        id ? fetchAllPages((page) => this.events.getPage({ matchPublicId: id }, page)).pipe(catchError(() => of([]))) : of([]),
      ),
    ),
    { initialValue: [] as MatchEvent[] },
  );

  private changes(publicId: string): Observable<unknown> {
    this.connection.set('connecting');
    let wasConnected = false;
    return this.live.watch(publicId).pipe(
      map((event) => {
        // Polls refresh themselves; only a reconnect can have missed changes.
        const reconnected = event.type === 'state' && event.state === 'connected' && wasConnected;
        const changed = event.type === 'data' ? event.data.change !== 'polls' : reconnected;
        this.connection.set(event.type === 'state' ? event.state : 'connected');
        if (event.type === 'state' && event.state === 'connected') wasConnected = true;
        return changed;
      }),
      filter(Boolean),
      tap(() => this.version.update((version) => version + 1)),
    );
  }
}
