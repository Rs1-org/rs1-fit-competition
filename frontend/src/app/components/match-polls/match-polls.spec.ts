import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { BehaviorSubject, of, Subject } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { MatchPolls } from './match-polls';
import { PollService } from '../../core/services/poll.service';
import { MatchLiveService, MatchChangeNotice } from '../../core/services/match-live.service';
import { RealtimeEvent } from '../../core/services/signalr.service';
import { UserStateService } from '../../core/services/user-state.service';
import { Poll } from '../../core/models/poll';
import { PagedResult } from '../../core/models/paged-result';

describe('Live match polling', () => {
  const id = 'a'.repeat(32), option = 'b'.repeat(32);
  const poll: Poll = { publicId: id, matchPublicId: 'c'.repeat(32), question: 'Who wins?', opensAt: '2026-10-18T11:00:00Z',
    closesAt: '2026-10-18T13:00:00Z', serverNow: '2026-10-18T12:00:00Z', isOpen: true, totalVotes: 0, myOptionPublicId: null,
    options: [{ publicId: option, label: 'Home', votes: 0, percentage: 0 }, { publicId: 'd'.repeat(32), label: 'Away', votes: 0, percentage: 0 }] };
  const page: PagedResult<Poll> = { items: [poll], totalCount: 1, page: 1, pageSize: 10, totalPages: 1, hasNextPage: false, hasPreviousPage: false };
  const getPage = vi.fn(), vote = vi.fn(); let component: MatchPolls;
  let user: BehaviorSubject<{ id: string; email: string } | null>, live: Subject<RealtimeEvent<MatchChangeNotice>>;
  beforeEach(() => {
    vi.useFakeTimers(); vi.setSystemTime(new Date('2026-10-18T12:00:00Z'));
    user = new BehaviorSubject<{ id: string; email: string } | null>({ id: 'test-user', email: 'dev@example.invalid' }); live = new Subject();
    getPage.mockReset().mockReturnValue(of(page)); vote.mockReset().mockReturnValue(of({ publicId: id }));
    TestBed.configureTestingModule({ providers: [
      { provide: PollService, useValue: { getPage, vote } }, { provide: MatchLiveService, useValue: { watch: () => live } },
      { provide: UserStateService, useValue: { user$: user, canManage: signal(false) } },
    ] });
    component = TestBed.runInInjectionContext(() => new MatchPolls()); component.matchPublicId = poll.matchPublicId; component.ngOnChanges();
  });
  afterEach(() => { vi.clearAllTimers(); vi.useRealTimers(); });
  it('requires a selected option and prevents repeat votes after success', () => {
    component.vote(poll); expect(vote).not.toHaveBeenCalled();
    component.voteForm(poll).controls.optionPublicId.setValue(option); component.vote(poll); component.vote(poll);
    expect(vote).toHaveBeenCalledExactlyOnceWith(id, option); expect(component.choice(poll)).toBe(option);
  });
  it('prevents duplicate requests and retains the selected option after a backend error', () => {
    const response = new Subject<{ publicId: string }>(); vote.mockReturnValue(response);
    component.voteForm(poll).controls.optionPublicId.setValue(option); component.vote(poll); component.vote(poll); expect(vote).toHaveBeenCalledOnce();
    response.error(new HttpErrorResponse({ status: 400, error: { errors: [{ propertyName: 'PollPublicId', errorMessage: 'Voting closed.' }] } }));
    expect(component.voteForm(poll).controls.optionPublicId.value).toBe(option); expect(component.errors()[id]).toContain('Voting closed.');
    expect(component.busy()).toBeNull();
  });
  it('keeps the same voting form visible while another spectator updates results', () => {
    const form = component.voteForm(poll); form.controls.optionPublicId.setValue(option);
    const response = new Subject<PagedResult<Poll>>(); getPage.mockReturnValue(response);
    live.next({ type: 'data', data: { matchPublicId: poll.matchPublicId, change: 'polls', changedAt: '' } });
    expect(component.state().loading).toBe(true); expect(component.state().data).not.toBeNull(); expect(component.voteForm(poll)).toBe(form);
    response.next({ ...page, items: [{ ...poll, totalVotes: 1 }] }); response.complete();
    expect(component.voteForm(poll).controls.optionPublicId.value).toBe(option); expect(component.state().data?.items[0].totalVotes).toBe(1);
  });
  it('blocks anonymous and closed-poll submissions and resets private choices between users', () => {
    component.voteForm(poll).controls.optionPublicId.setValue(option); component.vote({ ...poll, isOpen: false }); expect(vote).not.toHaveBeenCalled();
    user.next(null); component.vote(poll); expect(vote).not.toHaveBeenCalled(); expect(component.choice(poll)).toBeNull();
    expect(component.voteForm(poll).controls.optionPublicId.value).toBe('');
  });
  it('preserves a draft when the same user session refreshes, and resyncs on reconnect', () => {
    component.voteForm(poll).controls.optionPublicId.setValue(option);
    user.next({ id: 'test-user', email: 'new-metadata@example.invalid' }); expect(component.voteForm(poll).controls.optionPublicId.value).toBe(option);
    getPage.mockClear(); live.next({ type: 'state', state: 'connected' }); expect(getPage).toHaveBeenCalledExactlyOnceWith(poll.matchPublicId, 1);
  });
  it('refreshes opening/closing boundaries relative to server time despite browser clock skew', async () => {
    getPage.mockReturnValue(of({ ...page, items: [{ ...poll, isOpen: false, opensAt: '2026-10-18T12:00:01Z' }] })); component.retry();
    vi.setSystemTime(new Date('2030-01-01T00:00:00Z')); getPage.mockClear();
    await vi.advanceTimersByTimeAsync(1100); expect(getPage).toHaveBeenCalledOnce();
  });
});
