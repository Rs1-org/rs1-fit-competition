import { TestBed } from '@angular/core/testing';
import { registerLocaleData } from '@angular/common';
import localeBs from '@angular/common/locales/bs-Latn';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { NEVER, of, Subject, throwError } from 'rxjs';
import { signal } from '@angular/core';
import { MatchDetail } from './match-detail';
import { redirectToMatchTab } from './match-redirect.guard';
import { MatchPageStore } from './match-page.store';
import { MatchLineupsTab } from './tabs/match-lineups-tab';
import { MatchHeadToHeadTab } from './tabs/match-head-to-head-tab';
import { MatchService } from '../../core/services/match.service';
import { LineupService } from '../../core/services/lineup.service';
import { MatchEventService } from '../../core/services/match-event.service';
import { MatchLiveService } from '../../core/services/match-live.service';
import { StatisticService } from '../../core/services/statistic.service';
import { emptyLineup } from '../../core/models/lineup';
import { emptyPage } from '../../core/models/paged-result';
import { UserStateService } from '../../core/services/user-state.service';
import { MatchDetailsDto } from '../../core/models/match';
import { goalscorers, kickoffCountdown, latestMinute, statusNote } from '../../shared/match/match-display';

registerLocaleData(localeBs);

const derby: MatchDetailsDto = {
  publicId: 'm1', slug: 'derby', date: '2026-10-30T16:00:00Z', followersCount: 1200,
  homeScore: 2, awayScore: 1, halfHomeScore: 1, halfAwayScore: 0, isPostponed: false,
  statusPublicId: 's1', statusName: 'Finished',
  stadiumPublicId: 'st1', stadiumName: 'Grbavica', stadiumSlug: 'grbavica',
  homeSelectionPublicId: 'h', homeSelectionName: 'Zeljeznicar', homeSelectionSlug: 'zeljeznicar',
  awaySelectionPublicId: 'a', awaySelectionName: 'Sarajevo', awaySelectionSlug: 'fk-sarajevo',
  roundPublicId: 'r1', roundName: 'Round 1', roundSlug: 'round-1',
  seasonPublicId: 'se1', seasonName: '2026/27', seasonSlug: '2026-27',
  competitionPublicId: 'c1', competitionName: 'Premijer liga', competitionSlug: 'premijer-liga',
};

const lineup = {
  ...emptyLineup('m1'),
  refereeName: 'Ana Kovac',
  homeFormationCode: '4-3-3',
  home: [{ publicId: 'l1', playerPublicId: 'p1', playerName: 'Edin Smailagic', selectionPublicId: 'h', positionPublicId: null, positionName: 'Midfielder', subPositionPublicId: null, subPositionName: null, isStarter: true, shirtNumber: 6 }],
  away: [],
};

function setup(getBySlug: () => unknown, options: { canManage?: boolean; premium?: boolean; watch?: () => unknown; sheet?: unknown } = {}) {
  TestBed.configureTestingModule({
    providers: [
      provideRouter([]),
      { provide: ActivatedRoute, useValue: { paramMap: of(convertToParamMap({ slug: 'derby' })) } },
      { provide: MatchService, useValue: { getBySlug } },
      { provide: LineupService, useValue: { get: () => of(options.sheet ?? lineup) } },
      { provide: MatchEventService, useValue: { getPage: () => of(emptyPage()) } },
      { provide: MatchLiveService, useValue: { watch: options.watch ?? (() => NEVER) } },
      { provide: StatisticService, useValue: { getHeadToHead: () => of({
        firstName: 'Zeljeznicar', secondName: 'Sarajevo', firstWins: 2, draws: 1, secondWins: 0, firstGoals: 5, secondGoals: 2,
        matches: [{ date: '2026-01-01', homeName: 'Zeljeznicar', homeScore: 2, awayName: 'Sarajevo', awayScore: 0 }],
      }) } },
      { provide: UserStateService, useValue: {
        canManage: signal(!!options.canManage),
        isPremium: signal(!!options.premium),
        isLoggedIn: signal(false),
        roles$: of(options.canManage ? ['Moderator'] : []),
        hasAnyRole: () => !!options.canManage,
      } },
    ],
  });
}

async function render(getBySlug: () => unknown, options: Parameters<typeof setup>[1] = {}) {
  setup(getBySlug, options);
  const fixture = TestBed.createComponent(MatchDetail);
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
  return fixture.nativeElement as HTMLElement;
}

describe('MatchDetail', () => {
  it('shows the header with score, breadcrumb, links and referee', async () => {
    const page = await render(() => of(derby));

    expect(page.textContent).toContain('2 – 1');
    expect(page.textContent).toContain('HT 1 – 0');
    expect(page.textContent).toContain('Full time');
    expect(page.textContent).toContain('Referee: Ana Kovac');
    expect(page.querySelector('a[href="/stadium/grbavica"]')).not.toBeNull();
    expect(page.querySelector('a[href="/selection/zeljeznicar"]')).not.toBeNull();
    expect(page.querySelectorAll('a[href="/competitions/premijer-liga"]').length).toBe(2);
    expect(page.querySelectorAll('a[href="/rounds/round-1"]').length).toBe(2);
    expect(page.textContent).not.toContain('Edit match');
  });

  it('links every tab under the match slug and locks head-to-head without premium', async () => {
    const page = await render(() => of(derby));

    for (const tab of ['lineups', 'statistics', 'polls', 'reviews', 'head-to-head']) {
      expect(page.querySelector(`a[href="/matches/derby/${tab}"]`)).not.toBeNull();
    }
    expect(page.querySelector('a[href="/matches/derby/head-to-head"] app-icon')).not.toBeNull();
  });

  it('labels a missing referee', async () => {
    const page = await render(() => of(derby), { sheet: emptyLineup('m1') });

    expect(page.textContent).toContain('Referee: Not assigned');
  });

  it('shows staff links and a not-found message for an unknown slug', async () => {
    const staff = await render(() => of(derby), { canManage: true });
    expect(staff.querySelector('a[href="/manage-matches/derby"]')).not.toBeNull();
    expect(staff.querySelector('a[href="/manage-lineup/m1"]')).not.toBeNull();
    TestBed.resetTestingModule();
    expect((await render(() => throwError(() => new Error('404')))).textContent).toContain('Match not found.');
  });

  it('refetches the match on a live change and keeps it when a refresh fails', async () => {
    const changes = new Subject<unknown>();
    const getBySlug = vi.fn()
      .mockReturnValueOnce(of(derby))
      .mockReturnValueOnce(of({ ...derby, homeScore: 3 }))
      .mockReturnValueOnce(throwError(() => new Error('down')));
    setup(getBySlug, { watch: () => changes });
    const fixture = TestBed.createComponent(MatchDetail);
    fixture.detectChanges();

    changes.next({ type: 'data', data: { change: 'match' } });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('3 – 1');

    changes.next({ type: 'data', data: { change: 'polls' } });
    expect(getBySlug).toHaveBeenCalledTimes(2);

    changes.next({ type: 'data', data: { change: 'events' } });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('3 – 1');
  });

  it('reloads after a reconnect but not on the first connection', () => {
    const changes = new Subject<unknown>();
    const getBySlug = vi.fn(() => of(derby));
    setup(getBySlug, { watch: () => changes });
    const fixture = TestBed.createComponent(MatchDetail);
    fixture.detectChanges();

    changes.next({ type: 'state', state: 'connected' });
    expect(getBySlug).toHaveBeenCalledTimes(1);

    changes.next({ type: 'state', state: 'reconnecting' });
    changes.next({ type: 'state', state: 'connected' });
    expect(getBySlug).toHaveBeenCalledTimes(2);
  });

  it('warns when the live connection drops during a live match', async () => {
    const page = await render(() => of({ ...derby, statusName: 'Live' }), { watch: () => of({ type: 'state', state: 'reconnecting' }) });

    expect(page.textContent).toContain('Reconnecting');
  });
});

describe('match tabs', () => {
  function tab<T>(component: new () => T, options: Parameters<typeof setup>[1] = {}, match: MatchDetailsDto = derby) {
    setup(() => of(match), options);
    TestBed.overrideComponent(component as never, { add: { providers: [MatchPageStore] } });
    const fixture = TestBed.createComponent(component);
    fixture.detectChanges();
    return fixture;
  }

  it('lists the players, or a labelled empty state before line-ups exist', async () => {
    const fixture = tab(MatchLineupsTab);
    await fixture.whenStable();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Edin Smailagic');
    expect(fixture.nativeElement.textContent).toContain('4-3-3');

    TestBed.resetTestingModule();
    const empty = tab(MatchLineupsTab, { sheet: emptyLineup('m1') });
    await empty.whenStable();
    empty.detectChanges();
    expect(empty.nativeElement.textContent).toContain('Line-ups not announced');
  });

  it('shows head-to-head to premium users only', async () => {
    const premium = tab(MatchHeadToHeadTab, { premium: true });
    await premium.whenStable();
    premium.detectChanges();
    expect(premium.nativeElement.textContent).toContain('Zeljeznicar wins');
    expect(premium.nativeElement.querySelector('app-paywall')).toBeNull();
  });

  it('labels head-to-head until both teams are known', async () => {
    const fixture = tab(MatchHeadToHeadTab, { premium: true }, { ...derby, awaySelectionPublicId: null });
    await fixture.whenStable();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Head-to-head not available yet');
  });
});

describe('redirectToMatchTab', () => {
  function run(tab: string | undefined, getByPublicId: () => unknown) {
    TestBed.configureTestingModule({ providers: [provideRouter([]), { provide: MatchService, useValue: { getByPublicId } }] });
    const route = { paramMap: convertToParamMap({ publicId: 'm1' }) } as never;
    let target = '';
    TestBed.runInInjectionContext(() => (redirectToMatchTab(tab)(route, {} as never) as ReturnType<typeof of>)
      .subscribe((tree) => (target = TestBed.inject(Router).serializeUrl(tree as never))));
    return target;
  }

  it('sends old links to the right tab', () => {
    expect(run('statistics', () => of(derby))).toBe('/matches/derby/statistics');
    TestBed.resetTestingModule();
    expect(run(undefined, () => of(derby))).toBe('/matches/derby');
  });

  it('falls back to the match list for an unknown match', () => {
    expect(run('reviews', () => throwError(() => new Error('404')))).toBe('/matches');
  });
});

describe('match display helpers', () => {
  const event = (code: string, team: string, name: string, minute: number, addedMinute = 0) => ({
    eventType: { code }, selection: { publicId: team }, player: { name }, minute, addedMinute,
  });

  it('groups goals by scorer in minute order and credits own goals to the opponent', () => {
    const events = [event('goal', 'h', 'Hadzic', 63), event('goal', 'h', 'Hadzic', 23), event('penalty', 'a', 'Petrovic', 51),
      event('own-goal', 'a', 'Ramic', 70), event('yellow-card', 'h', 'Delic', 12)];

    expect(goalscorers(events, 'h', 'a')).toEqual([{ name: 'Hadzic', minutes: ["23'", "63'"] }, { name: 'Ramic', minutes: ["70' (OG)"] }]);
    expect(goalscorers(events, 'a', 'h')).toEqual([{ name: 'Petrovic', minutes: ["51' (P)"] }]);
  });

  it('counts down to kick-off', () => {
    const now = Date.parse('2026-11-02T12:00:00Z');

    expect(kickoffCountdown('2026-11-04T15:00:00Z', now)).toBe('Kick-off in 2 d 3 h');
    expect(kickoffCountdown('2026-11-02T14:20:00Z', now)).toBe('Kick-off in 2 h 20 min');
    expect(kickoffCountdown('2026-11-02T12:05:00Z', now)).toBe('Kick-off in 5 min');
    expect(kickoffCountdown('2026-11-02T11:00:00Z', now)).toBe('Kick-off soon');
  });

  it('names every finished state and the furthest minute', () => {
    expect(statusNote('finished')).toBe('Full time');
    expect(statusNote('postponed')).toContain('Postponed');
    expect(statusNote('cancelled')).toBe('Cancelled');
    expect(latestMinute([event('goal', 'h', 'A', 45, 2), event('goal', 'h', 'B', 45)])).toBe('45+2');
    expect(latestMinute([])).toBeNull();
  });
});
