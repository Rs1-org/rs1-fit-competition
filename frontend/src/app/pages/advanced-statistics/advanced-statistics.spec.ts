import { TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { of, throwError } from 'rxjs';
import { emptyPage } from '../../core/models/paged-result';
import { CompetitionService } from '../../core/services/competition.service';
import { SeasonService } from '../../core/services/season.service';
import { SelectionService } from '../../core/services/selection.service';
import { StatisticService } from '../../core/services/statistic.service';
import { UserStateService } from '../../core/services/user-state.service';
import { AdvancedStatistics } from './advanced-statistics';

describe('Advanced statistics access', () => {
  const getPlayerStats = vi.fn(); const getTeamForm = vi.fn(); const getHeadToHead = vi.fn();
  const isPremium = signal(true);

  function create(): AdvancedStatistics {
    TestBed.configureTestingModule({ providers: [
      { provide: StatisticService, useValue: { getPlayerStats, getTeamForm, getHeadToHead } },
      { provide: CompetitionService, useValue: { getAll: () => of(emptyPage()) } },
      { provide: SeasonService, useValue: { getAll: () => of(emptyPage()) } },
      { provide: SelectionService, useValue: { getAll: () => of(emptyPage()) } },
      { provide: UserStateService, useValue: { isPremium } },
    ] });
    const component = TestBed.runInInjectionContext(() => new AdvancedStatistics());
    TestBed.tick();
    return component;
  }

  beforeEach(() => {
    isPremium.set(true);
    getPlayerStats.mockReset().mockReturnValue(of(emptyPage()));
    getTeamForm.mockReset(); getHeadToHead.mockReset();
  });

  it('does not call premium endpoints for a non-premium user', () => {
    isPremium.set(false);
    const component = create();
    expect(component.unlocked()).toBe(false);
    expect(getPlayerStats).not.toHaveBeenCalled();
  });

  it('loads player statistics for a premium user', () => {
    expect(create().unlocked()).toBe(true);
    expect(getPlayerStats).toHaveBeenCalled();
  });

  it('falls back to the paywall when the API answers 403', () => {
    getPlayerStats.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 403 })));
    const component = create();
    expect(component.players().error).toBeTruthy();
    expect(component.unlocked()).toBe(false);
  });

  it('only requests head-to-head for two different teams', () => {
    getTeamForm.mockReturnValue(of({ matches: [] }));
    getHeadToHead.mockReturnValue(of({ matches: [] }));
    const component = create();
    const team = 'a'.repeat(32);
    component.filters.patchValue({ firstSelectionPublicId: team, secondSelectionPublicId: team });
    expect(getHeadToHead).not.toHaveBeenCalled();
    component.filters.patchValue({ secondSelectionPublicId: 'b'.repeat(32) });
    expect(getHeadToHead).toHaveBeenCalledOnce();
  });
});
