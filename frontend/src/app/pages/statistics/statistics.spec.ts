import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { of, throwError } from 'rxjs';
import { StatisticService } from '../../core/services/statistic.service';
import { MatchReviewService } from '../../core/services/match-review.service';
import { UserStateService } from '../../core/services/user-state.service';
import { emptyPage } from '../../core/models/paged-result';
import { Statistic } from '../../core/models/statistic';
import { Statistics } from './statistics';

describe('Statistics list filters', () => {
  const getPage = vi.fn(); let component: Statistics;
  beforeEach(() => {
    getPage.mockReset().mockReturnValue(of(emptyPage<Statistic>()));
    TestBed.configureTestingModule({ providers: [
      { provide: StatisticService, useValue: { getPage } }, { provide: MatchReviewService, useValue: { getMatch: () => of({}) } },
      { provide: UserStateService, useValue: { canManage: signal(false) } },
    ] });
    component = TestBed.runInInjectionContext(() => new Statistics());
  });
  it('sends all five filters to the server and resets page on filter changes', () => {
    component.page(2); component.chooseMatch('a'.repeat(32));
    component.filters.setValue({ selectionPublicId: 'b'.repeat(32), dateFrom: '2026-10-01T12:00', dateTo: '2026-10-18T12:00', minTotalShots: 0 }); component.apply();
    expect(getPage).toHaveBeenLastCalledWith({ matchPublicId: 'a'.repeat(32), selectionPublicId: 'b'.repeat(32),
      dateFrom: new Date('2026-10-01T12:00').toISOString(), dateTo: new Date('2026-10-18T12:00').toISOString(), minTotalShots: 0 }, 1);
    component.page(3); expect(getPage.mock.calls.at(-1)?.[1]).toBe(3);
    component.reset(); expect(getPage.mock.calls.at(-1)?.[1]).toBe(1); expect(component.matchId()).toBe('');
  });
  it('rejects invalid ranges and recovers from errors without showing a fake empty page', () => {
    getPage.mockClear(); component.filters.patchValue({ dateFrom: '2026-10-18T12:00', dateTo: '2026-10-01T12:00' }); component.apply();
    expect(getPage).not.toHaveBeenCalled(); expect(component.filterError()).toBeTruthy();
    getPage.mockReturnValueOnce(throwError(() => new Error('offline'))); component.retry();
    expect(component.state().data).toBeNull(); expect(component.state().error).toBeTruthy();
    component.retry(); expect(component.state().error).toBe('');
  });
});
