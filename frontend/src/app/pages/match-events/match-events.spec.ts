import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { signal } from '@angular/core';
import { BehaviorSubject, of, throwError } from 'rxjs';
import { MatchEvents } from './match-events';
import { MatchEventService } from '../../core/services/match-event.service';
import { UserStateService } from '../../core/services/user-state.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { emptyPage } from '../../core/models/paged-result';
import { MatchEvent } from '../../core/models/match-event';

describe('Event filters and retries', () => {
  const getPage = vi.fn(); let component: MatchEvents;
  const event: MatchEvent = { publicId: 'b'.repeat(32), matchPublicId: 'a'.repeat(32), matchName: 'Home vs Away',
    eventType: { publicId: 'c'.repeat(32), code: 'goal', name: 'Goal', sortOrder: 0 },
    matchPart: { publicId: 'd'.repeat(32), code: 'first-half', name: '1st half', sortOrder: 0 },
    selection: { publicId: 'e'.repeat(32), name: 'Home' }, player: null, relatedPlayer: null, minute: 20, addedMinute: 0, note: null };
  beforeEach(() => {
    getPage.mockReset().mockReturnValue(of(emptyPage<MatchEvent>()));
    TestBed.configureTestingModule({ providers: [
      { provide: ActivatedRoute, useValue: { paramMap: new BehaviorSubject(convertToParamMap({})) } },
      { provide: UserStateService, useValue: { canManage: signal(false) } },
      { provide: ConfirmDialogService, useValue: { confirm: vi.fn() } },
      { provide: MatchEventService, useValue: { getPage, lookups: () => of({ eventTypes: [], matchParts: [] }), context: () => of({ matchPublicId: 'a'.repeat(32), homeName: 'Home', awayName: 'Away', selections: [] }) } },
    ] });
    component = TestBed.runInInjectionContext(() => new MatchEvents());
  });
  it('sends filters to the backend and resets paging when filters or match change', () => {
    component.page(3); component.chooseMatch('a'.repeat(32));
    component.filters.patchValue({ eventTypePublicId: 'b'.repeat(32), selectionPublicId: 'c'.repeat(32), playerPublicId: 'd'.repeat(32), minuteFrom: 20, minuteTo: 50 });
    component.apply();
    expect(getPage).toHaveBeenLastCalledWith({ matchPublicId: 'a'.repeat(32), eventTypePublicId: 'b'.repeat(32), selectionPublicId: 'c'.repeat(32), playerPublicId: 'd'.repeat(32), minuteFrom: 20, minuteTo: 50 }, 1);
    component.page(2); expect(getPage.mock.calls.at(-1)?.[1]).toBe(2);
    component.reset(); expect(getPage.mock.calls.at(-1)?.[1]).toBe(1);
    expect(component.canManage()).toBe(false);
  });
  it('rejects reversed ranges and recovers from failed reads', () => {
    getPage.mockClear(); component.filters.patchValue({ minuteFrom: 80, minuteTo: 20 }); component.apply();
    expect(getPage).not.toHaveBeenCalled(); expect(component.filterError()).toBeTruthy();
    getPage.mockReturnValueOnce(throwError(() => new Error('offline'))); component.retry();
    expect(component.state().data).toBeNull(); expect(component.state().error).toBeTruthy();
    component.retry(); expect(component.state().error).toBe('');
  });
  it('refreshes events after a live notice without discarding filters or the current page', () => {
    component.chooseMatch('a'.repeat(32)); component.filters.patchValue({ minuteFrom: 20 }); component.apply();
    getPage.mockReturnValue(of({ ...emptyPage<MatchEvent>(), items: [event], page: 2, totalCount: 21, totalPages: 2 })); component.page(2);
    getPage.mockClear(); component.liveChanged('events');
    expect(getPage.mock.calls.at(-1)?.[0].minuteFrom).toBe(20); expect(getPage.mock.calls.at(-1)?.[1]).toBe(2);
    expect(component.filters.controls.minuteFrom.value).toBe(20);
  });
  it('moves to the last valid page if a live deletion empties the current page', () => {
    getPage.mockReturnValueOnce(of({ ...emptyPage<MatchEvent>(), page: 2, totalCount: 1, totalPages: 1 }))
      .mockReturnValueOnce(of({ ...emptyPage<MatchEvent>(), items: [event], page: 1, totalCount: 1, totalPages: 1 }));
    component.page(2); expect(getPage.mock.calls.at(-1)?.[1]).toBe(1); expect(component.state().data?.page).toBe(1);
  });
});
