import { inject, Injectable } from '@angular/core';
import { defer, filter, finalize, Observable, shareReplay } from 'rxjs';
import { RealtimeEvent, SignalRService } from './signalr.service';

export interface MatchChangeNotice { matchPublicId: string; change: 'match' | 'events' | 'deleted' | 'polls'; changedAt: string; }
@Injectable({ providedIn: 'root' })
export class MatchLiveService {
  private readonly realtime = inject(SignalRService);
  private readonly streams = new Map<string, Observable<RealtimeEvent<MatchChangeNotice>>>();
  watch(matchPublicId: string): Observable<RealtimeEvent<MatchChangeNotice>> {
    const existing = this.streams.get(matchPublicId); if (existing) return existing;
    const stream = defer(() => this.realtime.watch<MatchChangeNotice>({ hubPath: '/hubs/matches', eventName: 'MatchChanged',
      joinMethod: 'WatchMatch', args: [matchPublicId] })).pipe(
      filter(event => event.type === 'state' || event.data.matchPublicId === matchPublicId),
      finalize(() => this.streams.delete(matchPublicId)),
      shareReplay({ bufferSize: 1, refCount: true }),
    );
    this.streams.set(matchPublicId, stream); return stream;
  }
}
