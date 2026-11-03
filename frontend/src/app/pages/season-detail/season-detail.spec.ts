import { registerLocaleData } from '@angular/common';
import localeBs from '@angular/common/locales/bs-Latn';
import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { NEVER, Observable, of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { SeasonDetail } from './season-detail';
import { SeasonService } from '../../core/services/season.service';
import { RoundService } from '../../core/services/round.service';
import { MatchService } from '../../core/services/match.service';
import { StatisticService } from '../../core/services/statistic.service';
import { SelectionService } from '../../core/services/selection.service';
import { SignalRService } from '../../core/services/signalr.service';
import { UserStateService } from '../../core/services/user-state.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { SeasonDetailsDto } from '../../core/models/season';
import { MatchListDto } from '../../core/models/match';
import { emptyPage, PagedResult } from '../../core/models/paged-result';
import { TeamResults } from '../../core/models/statistic';

registerLocaleData(localeBs);

function page<T>(items: T[]): PagedResult<T> {
  return { ...emptyPage<T>(), items, totalCount: items.length, totalPages: 1 };
}

const season: SeasonDetailsDto = {
  publicId: 's1', name: '2026/27', slug: 'pl-2026-27', startDate: '2026-07-26', endDate: '2027-05-30', isCurrent: true,
  competitionPublicId: 'c1', competitionName: 'Premijer liga BiH', competitionSlug: 'premijer-liga',
};

const table: TeamResults[] = [
  { selectionPublicId: 't1', selectionName: 'Zrinjski', played: 2, wins: 2, draws: 0, losses: 0, goalsFor: 5, goalsAgainst: 1, points: 6 },
  { selectionPublicId: 't2', selectionName: 'Sarajevo', played: 2, wins: 0, draws: 0, losses: 2, goalsFor: 1, goalsAgainst: 5, points: 0 },
];

function finished(home: string, away: string, homeScore: number, awayScore: number, date: string): MatchListDto {
  return {
    publicId: `${home}-${away}`, slug: 'm', date, homeScore, awayScore, isPostponed: false, statusName: 'Finished', stadiumName: null,
    homeSelectionName: home, awaySelectionName: away, roundPublicId: 'r1', roundName: 'Round 1', seasonName: '2026/27', competitionName: null,
  };
}

interface Options { season?: Observable<SeasonDetailsDto>; premium?: boolean; manage?: boolean; scorers?: Observable<unknown>; }

function render(options: Options = {}) {
  const getPlayerStats = vi.fn(() => options.scorers ?? of(page([
    { playerPublicId: 'p1', playerName: 'Amar Hadžić', selectionName: 'Sarajevo', goals: 3, yellowCards: 0, redCards: 0 },
    { playerPublicId: 'p2', playerName: 'No Goals', selectionName: 'Sarajevo', goals: 0, yellowCards: 1, redCards: 0 },
  ])));
  const matchGetAll = vi.fn(() => of(page([
    finished('Zrinjski', 'Sarajevo', 3, 1, '2026-08-01T18:00:00Z'),
    finished('Sarajevo', 'Zrinjski', 0, 2, '2026-08-08T18:00:00Z'),
    { ...finished('Other', 'Team', 1, 0, '2026-08-08T18:00:00Z'), seasonName: '2025/26' },
  ])));
  TestBed.configureTestingModule({
    imports: [SeasonDetail],
    providers: [
      provideRouter([]),
      { provide: ActivatedRoute, useValue: { paramMap: of(convertToParamMap({ slug: season.slug })) } },
      { provide: SeasonService, useValue: { getBySlug: () => options.season ?? of(season) } },
      { provide: RoundService, useValue: { getAll: () => of(page([{ publicId: 'r1', name: 'Round 1', slug: 'round-1', number: 1, matchCount: 2 }])) } },
      { provide: MatchService, useValue: { getAll: matchGetAll } },
      { provide: StatisticService, useValue: { getStandings: () => of({ table, matchPublicIds: ['m1'] }), getPlayerStats } },
      { provide: SelectionService, useValue: { getAll: () => of(page([{ publicId: 't1', name: 'Zrinjski', abbreviation: 'ZRI', slug: 'zrinjski' }])) } },
      { provide: SignalRService, useValue: { watch: () => NEVER } },
      { provide: UserStateService, useValue: {
        canManage: signal(options.manage ?? false), isAdmin: signal(false), isPremium: signal(options.premium ?? false),
      } },
      { provide: ConfirmDialogService, useValue: { confirm: async () => false } },
    ],
  });
  const fixture = TestBed.createComponent(SeasonDetail);
  fixture.detectChanges();
  return { fixture, component: fixture.componentInstance, element: fixture.nativeElement as HTMLElement, getPlayerStats, matchGetAll };
}

describe('SeasonDetail', () => {
  it('fills the header tiles from the table and rounds', () => {
    const { component, element } = render();

    expect(element.querySelector('h1')?.textContent).toContain('Premijer liga BiH');
    expect(element.textContent).toContain('Current season');
    expect(component.roundsTile()).toBe('1 / 1');
    expect(component.teamsCount()).toBe(2);
    expect(component.goalsPerMatch()).toBe(3);
  });

  it('loads the season matches by competition and dates, then keeps this season only', () => {
    const { component, matchGetAll } = render();

    expect(matchGetAll).toHaveBeenCalledWith(1, 100, expect.objectContaining({
      competitionPublicId: 'c1', dateFrom: '2026-07-26', dateTo: '2027-05-30T23:59:59',
    }));
    expect(component.form()).toEqual({ t1: ['W', 'W'], t2: ['L', 'L'] });
  });

  it('links team rows to the team page and round rows to the round', () => {
    const { element } = render();

    expect(element.querySelector('app-league-table a[href="/selection/zrinjski"]')).not.toBeNull();
    const roundLink = element.querySelector('a.round-link[href="/rounds/round-1"]');
    expect(roundLink?.textContent).toContain('Done');
    expect(roundLink?.textContent).toContain('2 matches · 6 goals');
  });

  it('shows a labelled Premium card instead of top scorers for free users', () => {
    const { element, getPlayerStats } = render();

    expect(getPlayerStats).not.toHaveBeenCalled();
    expect(element.querySelector('.locked')?.textContent).toContain('Top scorers are part of Premium');
  });

  it('lists scorers with goals for premium users', () => {
    const { element, getPlayerStats } = render({ premium: true });

    expect(getPlayerStats).toHaveBeenCalledWith({ competitionPublicId: 'c1', seasonPublicId: 's1' });
    const rows = element.querySelectorAll('.scorers li');
    expect(rows.length).toBe(1);
    expect(rows[0].textContent).toContain('Amar Hadžić');
  });

  it('falls back to the Premium card when the backend says premium has expired', () => {
    const { element } = render({ premium: true, scorers: throwError(() => new HttpErrorResponse({ status: 403 })) });

    expect(element.querySelector('.locked')).not.toBeNull();
  });

  it('shows the add round button to staff only', () => {
    expect(render().element.textContent).not.toContain('Add round');
    TestBed.resetTestingModule();
    expect(render({ manage: true }).element.textContent).toContain('Add round');
  });

  it('shows not found and error states', () => {
    const missing = render({ season: throwError(() => new HttpErrorResponse({ status: 404 })) });
    expect(missing.element.textContent).toContain('Season not found');

    TestBed.resetTestingModule();
    const failed = render({ season: throwError(() => new HttpErrorResponse({ status: 500 })) });
    expect(failed.element.querySelector('app-error-banner')).not.toBeNull();
  });
});
