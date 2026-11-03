import { registerLocaleData } from '@angular/common';
import localeBs from '@angular/common/locales/bs-Latn';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { RoundDetail } from './round-detail';
import { RoundService } from '../../core/services/round.service';
import { MatchService } from '../../core/services/match.service';
import { RoundDetailsDto } from '../../core/models/round';
import { MatchListDto } from '../../core/models/match';
import { emptyPage, PagedResult } from '../../core/models/paged-result';

registerLocaleData(localeBs);

function page<T>(items: T[]): PagedResult<T> {
  return { ...emptyPage<T>(), items, totalCount: items.length, totalPages: 1 };
}

const round: RoundDetailsDto = {
  publicId: 'r1', name: 'Round 1', slug: 'round-1', number: 1, seasonPublicId: 's1', seasonName: '2026/27', seasonSlug: 'pl-2026-27',
  competitionName: 'Premijer liga BiH', competitionSlug: 'premijer-liga',
};

const derby: MatchListDto = {
  publicId: 'm1', slug: 'derby', date: '2026-08-01T18:00:00Z', homeScore: 2, awayScore: 1, isPostponed: false, statusName: 'Finished',
  stadiumName: 'Koševo', homeSelectionName: 'Sarajevo', awaySelectionName: 'Željezničar', roundPublicId: 'r1', roundName: 'Round 1',
  seasonName: '2026/27', competitionName: 'Premijer liga BiH',
};

function render(getAll = vi.fn(() => of(page([derby])))) {
  TestBed.configureTestingModule({
    imports: [RoundDetail],
    providers: [
      provideRouter([]),
      { provide: ActivatedRoute, useValue: { paramMap: of(convertToParamMap({ slug: 'round-1' })) } },
      { provide: RoundService, useValue: { getBySlug: () => of(round) } },
      { provide: MatchService, useValue: { getAll } },
    ],
  });
  const fixture = TestBed.createComponent(RoundDetail);
  fixture.detectChanges();
  return { element: fixture.nativeElement as HTMLElement, getAll };
}

describe('RoundDetail', () => {
  it('shows the breadcrumb back to the season and the round tiles', () => {
    const { element } = render();

    expect(element.querySelector('app-breadcrumb a[href="/seasons/pl-2026-27"]')).not.toBeNull();
    expect(element.querySelector('h1')?.textContent).toContain('Round 1');
    expect(element.querySelector('app-entity-header')?.textContent).toContain('3');
  });

  it('lists the matches as shared rows linking to the match page, in kick-off order', () => {
    const { element, getAll } = render();

    expect(getAll).toHaveBeenCalledWith(1, 100, { roundPublicId: 'r1', sortBy: 'date', sortDir: 'asc' });
    const link = element.querySelector('a[href="/matches/derby"]');
    expect(link?.querySelector('app-match-row')).not.toBeNull();
    expect(link?.textContent).toContain('Koševo');
  });

  it('shows the empty and error states', () => {
    expect(render(vi.fn(() => of(page<MatchListDto>([])))).element.textContent).toContain('No matches in this round yet');

    TestBed.resetTestingModule();
    expect(render(vi.fn(() => throwError(() => new Error('down')))).element.querySelector('app-error-banner')).not.toBeNull();
  });
});
