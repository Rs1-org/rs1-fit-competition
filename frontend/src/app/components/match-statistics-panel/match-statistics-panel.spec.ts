import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { MatchStatisticsPanel } from './match-statistics-panel';
import { StatisticService } from '../../core/services/statistic.service';
import { UserStateService } from '../../core/services/user-state.service';

describe('Match statistics loading', () => {
  it('distinguishes no recorded statistics from an API failure and supports retry', () => {
    const getForMatch = vi.fn().mockReturnValue(throwError(() => new HttpErrorResponse({ status: 404 })));
    TestBed.configureTestingModule({ providers: [
      { provide: StatisticService, useValue: { getForMatch } }, { provide: UserStateService, useValue: { canManage: signal(false) } },
    ] });
    const panel = TestBed.runInInjectionContext(() => new MatchStatisticsPanel());
    panel.matchPublicId = 'a'.repeat(32); panel.ngOnChanges();
    expect(panel.state()).toEqual({ loading: false, data: null, error: '' }); expect(panel.canManage()).toBe(false);
    getForMatch.mockReturnValueOnce(throwError(() => new HttpErrorResponse({ status: 503 }))); panel.retry();
    expect(panel.state().error).toBeTruthy();
    getForMatch.mockReturnValue(of({ publicId: 'b'.repeat(32) })); panel.retry(); expect(panel.state().data?.publicId).toBe('b'.repeat(32));
  });
});
