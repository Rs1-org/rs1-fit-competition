import { TestBed } from '@angular/core/testing';
import { TOUR_STEPS, TourService } from './tour.service';

describe('TourService', () => {
  const fresh = () => {
    TestBed.resetTestingModule();
    return TestBed.inject(TourService);
  };

  beforeEach(() => localStorage.clear());
  afterEach(() => localStorage.clear());

  it('starts on the first visit at the first step', () => {
    const tour = fresh();
    tour.startIfFirstVisit();
    expect(tour.active()).toBe(true);
    expect(tour.step().target).toBe('live');
  });

  it('covers Live, Matches, Competitions, Statistics and the account menu', () => {
    expect(TOUR_STEPS.map(step => step.target)).toEqual(['live', 'matches', 'competitions', 'statistics', 'account']);
  });

  it('does not show again after it is finished', () => {
    const tour = fresh();
    tour.startIfFirstVisit();
    TOUR_STEPS.forEach(() => tour.next());
    expect(tour.active()).toBe(false);

    const afterRefresh = fresh();
    afterRefresh.startIfFirstVisit();
    expect(afterRefresh.active()).toBe(false);
  });

  it('does not show again after it is skipped', () => {
    const tour = fresh();
    tour.startIfFirstVisit();
    tour.skip();

    const afterRefresh = fresh();
    afterRefresh.startIfFirstVisit();
    expect(afterRefresh.active()).toBe(false);
  });

  it('resumes the current step after a refresh', () => {
    const tour = fresh();
    tour.startIfFirstVisit();
    tour.next();
    tour.next();

    const afterRefresh = fresh();
    afterRefresh.startIfFirstVisit();
    expect(afterRefresh.active()).toBe(true);
    expect(afterRefresh.step().target).toBe('competitions');
  });

  it('keeps back and next inside the steps', () => {
    const tour = fresh();
    tour.startIfFirstVisit();
    tour.back();
    expect(tour.index()).toBe(0);
    tour.next();
    tour.back();
    expect(tour.index()).toBe(0);
    for (let i = 0; i < TOUR_STEPS.length - 1; i++) tour.next();
    expect(tour.isLast()).toBe(true);
  });

  it('restarts from the first step after it was finished', () => {
    const tour = fresh();
    tour.startIfFirstVisit();
    tour.skip();
    tour.restart();
    expect(tour.active()).toBe(true);
    expect(tour.index()).toBe(0);
  });

  it('ignores a saved step that is out of range', () => {
    localStorage.setItem('tour_step', '42');
    const tour = fresh();
    tour.startIfFirstVisit();
    expect(tour.index()).toBe(0);
  });
});
