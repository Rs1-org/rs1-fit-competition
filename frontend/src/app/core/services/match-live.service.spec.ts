import { TestBed } from '@angular/core/testing';
import { finalize, Subject } from 'rxjs';
import { MatchChangeNotice, MatchLiveService } from './match-live.service';
import { RealtimeEvent, SignalRService } from './signalr.service';

describe('Shared match subscription', () => {
  it('shares a match connection across consumers and stops only when the last one leaves', () => {
    const source = new Subject<RealtimeEvent<MatchChangeNotice>>(), stopped = vi.fn();
    const watch = vi.fn().mockReturnValue(source.pipe(finalize(stopped)));
    TestBed.configureTestingModule({ providers: [{ provide: SignalRService, useValue: { watch } }] });
    const live = TestBed.inject(MatchLiveService), id = 'a'.repeat(32), received: RealtimeEvent<MatchChangeNotice>[] = [];
    const first = live.watch(id).subscribe(), second = live.watch(id).subscribe(value => received.push(value));
    expect(watch).toHaveBeenCalledOnce(); expect(watch.mock.calls[0][0].args).toEqual([id]);
    source.next({ type: 'data', data: { matchPublicId: 'b'.repeat(32), change: 'events', changedAt: '' } }); expect(received).toEqual([]);
    source.next({ type: 'data', data: { matchPublicId: id, change: 'events', changedAt: '' } }); expect(received).toHaveLength(1);
    first.unsubscribe(); expect(stopped).not.toHaveBeenCalled(); second.unsubscribe(); expect(stopped).toHaveBeenCalledOnce();
    const next = live.watch(id).subscribe(); expect(watch).toHaveBeenCalledTimes(2); next.unsubscribe();
  });
});
