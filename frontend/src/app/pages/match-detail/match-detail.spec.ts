import { TestBed } from '@angular/core/testing';
import { registerLocaleData } from '@angular/common';
import localeBs from '@angular/common/locales/bs-Latn';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { signal } from '@angular/core';
import { MatchDetail } from './match-detail';
import { MatchService } from '../../core/services/match.service';
import { UserStateService } from '../../core/services/user-state.service';
import { MatchDetailsDto } from '../../core/models/match';

registerLocaleData(localeBs);

const derby: MatchDetailsDto = {
  publicId: 'm1', slug: 'derby', date: '2026-10-30T16:00:00Z', followersCount: 1200,
  homeScore: 2, awayScore: 1, halfHomeScore: 1, halfAwayScore: 0, isPostponed: false,
  statusPublicId: 's1', statusName: 'Finished',
  stadiumPublicId: 'st1', stadiumName: 'Grbavica', stadiumSlug: 'grbavica',
  homeSelectionPublicId: 'h', homeSelectionName: 'Zeljeznicar', homeSelectionSlug: 'zeljeznicar',
  awaySelectionPublicId: 'a', awaySelectionName: 'Sarajevo', awaySelectionSlug: 'fk-sarajevo',
  roundPublicId: 'r1', roundName: 'Round 1', roundSlug: 'round-1', seasonPublicId: 'se1', competitionPublicId: 'c1',
};

function render(getBySlug: () => unknown, canManage = false) {
  TestBed.configureTestingModule({
    providers: [
      provideRouter([]),
      { provide: ActivatedRoute, useValue: { paramMap: of(convertToParamMap({ slug: 'derby' })) } },
      { provide: MatchService, useValue: { getBySlug } },
      { provide: UserStateService, useValue: { canManage: signal(canManage) } },
    ],
  });
  const fixture = TestBed.createComponent(MatchDetail);
  fixture.detectChanges();
  return fixture.nativeElement as HTMLElement;
}

describe('MatchDetail', () => {
  it('shows the score and links to stadium, teams, round and match pages', () => {
    const page = render(() => of(derby));
    expect(page.textContent).toContain('2 : 1');
    expect(page.textContent).toContain('Half-time 1 : 0');
    expect(page.querySelector('a[href="/stadium/grbavica"]')?.textContent).toContain('Grbavica');
    expect(page.querySelector('a[href="/selection/zeljeznicar"]')).not.toBeNull();
    expect(page.querySelector('a[href="/rounds/round-1"]')).not.toBeNull();
    expect(page.querySelector('a[href="/match-statistics/m1"]')).not.toBeNull();
    expect(page.textContent).not.toContain('Edit match');
  });

  it('shows the edit link to staff and a not-found message for an unknown slug', () => {
    expect(render(() => of(derby), true).querySelector('a[href="/manage-matches/derby"]')).not.toBeNull();
    TestBed.resetTestingModule();
    expect(render(() => throwError(() => new Error('404'))).textContent).toContain('Match not found.');
  });
});
