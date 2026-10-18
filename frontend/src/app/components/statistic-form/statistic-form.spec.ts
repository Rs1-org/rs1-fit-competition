import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { BehaviorSubject, of, Subject, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { StatisticForm } from './statistic-form';
import { StatisticService } from '../../core/services/statistic.service';
import { MatchReviewService } from '../../core/services/match-review.service';
import { Statistic, TeamStatisticValues } from '../../core/models/statistic';
import { ReviewMatchDetails } from '../../core/models/match-review';

describe('Statistic entry and editing', () => {
  const id = 'a'.repeat(32);
  const match: ReviewMatchDetails = { publicId: id, date: null, homeSelectionName: 'Home', awaySelectionName: 'Away',
    homeSelectionPublicId: 'b'.repeat(32), awaySelectionPublicId: 'c'.repeat(32), homeScore: null, awayScore: null, stadiumName: null, statusName: 'Finished' };
  const values: TeamStatisticValues = { totalShots: 10, shotsOnTarget: 5, totalRedCards: 0, totalYellowCards: 2, corners: 4, bigChances: 3, possession: 50 };
  const create = vi.fn(), update = vi.fn(), getForMatch = vi.fn(), navigate = vi.fn();
  let component: StatisticForm;
  beforeEach(() => {
    create.mockReset().mockReturnValue(of({ publicId: id })); update.mockReset().mockReturnValue(of({ publicId: id }));
    getForMatch.mockReset().mockReturnValue(throwError(() => new HttpErrorResponse({ status: 404 })));
    navigate.mockReset().mockResolvedValue(true);
    TestBed.configureTestingModule({ providers: [
      { provide: ActivatedRoute, useValue: { paramMap: new BehaviorSubject(convertToParamMap({ matchPublicId: id })) } },
      { provide: Router, useValue: { navigate } },
      { provide: MatchReviewService, useValue: { getMatch: () => of(match) } },
      { provide: StatisticService, useValue: { getForMatch, create, update } },
    ] });
    component = TestBed.runInInjectionContext(() => new StatisticForm());
  });
  it('requires all numbers without inventing zero-valued statistics', () => {
    component.save(); expect(create).not.toHaveBeenCalled(); expect(component.form.invalid).toBe(true);
    expect(component.form.controls.home.controls.totalShots.value).toBeNull();
  });
  it.each([-1, 1.5, 2147483648])('rejects invalid counter %s before sending', value => {
    component.form.setValue({ home: { ...values, totalShots: value }, away: values }); component.save();
    expect(create).not.toHaveBeenCalled();
  });
  it('blocks shots above total and possession not summing to 100', () => {
    component.form.setValue({ home: { ...values, shotsOnTarget: 11 }, away: values }); component.save();
    expect(component.form.controls.home.hasError('shotsWithinTotal')).toBe(true);
    component.form.setValue({ home: { ...values, possession: 60 }, away: values }); component.save();
    expect(component.form.hasError('possessionTotal')).toBe(true); expect(create).not.toHaveBeenCalled();
  });
  it('creates both teams atomically using the public match identifier', () => {
    component.form.setValue({ home: values, away: values }); component.save();
    expect(create).toHaveBeenCalledExactlyOnceWith(id, { home: values, away: values });
    expect(navigate).toHaveBeenCalledExactlyOnceWith(['/match-statistics', id]); expect(update).not.toHaveBeenCalled();
  });
  it('loads existing values and updates by statistic public ID', () => {
    const saved: Statistic = { publicId: 'd'.repeat(32), matchPublicId: id, matchDate: '', matchSlug: null,
      home: { ...values, publicId: 'e'.repeat(32), selectionPublicId: match.homeSelectionPublicId!, selectionName: 'Home' },
      away: { ...values, publicId: 'f'.repeat(32), selectionPublicId: match.awaySelectionPublicId!, selectionName: 'Away' } };
    getForMatch.mockReturnValue(of(saved)); component.retry(); component.form.controls.home.controls.totalShots.setValue(12); component.save();
    expect(update).toHaveBeenCalledExactlyOnceWith(saved.publicId, { home: { ...values, totalShots: 12 }, away: values });
    expect(create).not.toHaveBeenCalled();
  });
  it('prevents duplicate saves and keeps values with backend field errors', () => {
    const response = new Subject<Statistic>(); create.mockReturnValue(response);
    component.form.setValue({ home: values, away: values }); component.save(); component.save(); expect(create).toHaveBeenCalledOnce();
    response.error(new HttpErrorResponse({ status: 400, error: { errors: [{ propertyName: 'Home.TotalShots', errorMessage: 'Check home shots.' }] } }));
    expect(component.form.controls.home.controls.totalShots.value).toBe(10);
    expect(component.fieldErrors()['home.totalshots']).toBe('Check home shots.'); expect(component.busy()).toBe(false);
    expect(navigate).not.toHaveBeenCalled();
  });
  it('does not treat a failed API as permission to create a new statistic', () => {
    getForMatch.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 503 }))); component.retry();
    component.form.setValue({ home: values, away: values }); component.save();
    expect(component.loadError()).toBeTruthy(); expect(create).not.toHaveBeenCalled();
  });
});
