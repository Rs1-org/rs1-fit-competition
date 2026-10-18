import { TestBed } from '@angular/core/testing';
import { of, Subject } from 'rxjs';
import { MatchLiveSummary } from './match-live-summary';
import { MatchChangeNotice, MatchLiveService } from '../../core/services/match-live.service';
import { MatchReviewService } from '../../core/services/match-review.service';
import { RealtimeEvent } from '../../core/services/signalr.service';

describe('Live match summary snapshots', () => {
  it('fetches current scores after join, server changes and reconnect without a page reload', () => {
    const notices = new Subject<RealtimeEvent<MatchChangeNotice>>();
    const getMatch = vi.fn().mockReturnValue(of({ publicId: 'a'.repeat(32), homeScore: 0, awayScore: 0 }));
    TestBed.configureTestingModule({ providers: [
      { provide: MatchReviewService, useValue: { getMatch } }, { provide: MatchLiveService, useValue: { watch: () => notices } },
    ] });
    const summary = TestBed.runInInjectionContext(() => new MatchLiveSummary());
    const changed = vi.fn(); summary.changed.subscribe(changed);
    summary.matchPublicId = 'a'.repeat(32); summary.ngOnChanges(); expect(summary.state().data?.homeScore).toBe(0);
    notices.next({ type: 'state', state: 'connected' }); expect(changed).toHaveBeenLastCalledWith('resync');
    getMatch.mockReturnValue(of({ publicId: summary.matchPublicId, homeScore: 1, awayScore: 0 }));
    notices.next({ type: 'data', data: { matchPublicId: summary.matchPublicId, change: 'events', changedAt: '' } });
    expect(summary.state().data?.homeScore).toBe(1); expect(changed).toHaveBeenLastCalledWith('events');
    notices.next({ type: 'state', state: 'reconnecting' }); expect(summary.connection()).toBe('reconnecting');
    expect(summary.state().data?.homeScore).toBe(1);
    getMatch.mockReturnValue(of({ publicId: summary.matchPublicId, homeScore: 2, awayScore: 0 }));
    notices.next({ type: 'state', state: 'connected' }); expect(summary.state().data?.homeScore).toBe(2);
    expect(changed).toHaveBeenLastCalledWith('resync');
    getMatch.mockClear(); changed.mockClear();
    notices.next({ type: 'data', data: { matchPublicId: summary.matchPublicId, change: 'polls', changedAt: '' } });
    expect(getMatch).not.toHaveBeenCalled(); expect(changed).not.toHaveBeenCalled();
  });
});
