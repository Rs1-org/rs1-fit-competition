import { registerLocaleData } from '@angular/common';
import localeBs from '@angular/common/locales/bs-Latn';
import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';

import { CompetitionDetail } from './competition-detail';
import { CompetitionService } from '../../core/services/competition.service';
import { SeasonService } from '../../core/services/season.service';
import { UserStateService } from '../../core/services/user-state.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { CompetitionDetailsDto } from '../../core/models/competition';
import { SeasonListDto } from '../../core/models/season';
import { emptyPage, PagedResult } from '../../core/models/paged-result';

registerLocaleData(localeBs);

function page<T>(items: T[]): PagedResult<T> {
  return { ...emptyPage<T>(), items, totalCount: items.length, totalPages: 1 };
}

const league: CompetitionDetailsDto = {
  publicId: 'c1', name: 'Premijer liga BiH', slug: 'premijer-liga', shortName: 'PL', description: 'Top flight.', countryPublicId: 'bih',
  countryName: 'Bosnia and Herzegovina',
};
const current: SeasonListDto = {
  publicId: 's1', name: '2026/27', slug: 'pl-2026-27', startDate: '2026-07-26', endDate: '2027-05-30', isCurrent: true,
  competitionName: 'Premijer liga BiH', roundCount: 33,
};

function render(competition: Observable<CompetitionDetailsDto> = of(league), manage = false) {
  TestBed.configureTestingModule({
    imports: [CompetitionDetail],
    providers: [
      provideRouter([]),
      { provide: ActivatedRoute, useValue: { paramMap: of(convertToParamMap({ slug: 'premijer-liga' })) } },
      { provide: CompetitionService, useValue: { getBySlug: () => competition } },
      { provide: SeasonService, useValue: { getAll: () => of(page([current])) } },
      { provide: UserStateService, useValue: { canManage: signal(manage), isAdmin: signal(false) } },
      { provide: ConfirmDialogService, useValue: { confirm: async () => false } },
    ],
  });
  const fixture = TestBed.createComponent(CompetitionDetail);
  fixture.detectChanges();
  return fixture.nativeElement as HTMLElement;
}

describe('CompetitionDetail', () => {
  it('shows the header, a link to the current season and the season cards', () => {
    const element = render();

    expect(element.querySelector('h1')?.textContent).toContain('Premijer liga BiH');
    expect(element.textContent).toContain('Bosnia and Herzegovina · PL');
    expect(element.querySelector('app-stat-tile a[href="/seasons/pl-2026-27"]')).not.toBeNull();
    const card = element.querySelector('a.entity-card-link[href="/seasons/pl-2026-27"]');
    expect(card?.textContent).toContain('Current');
    expect(card?.textContent).toContain('26.07.2026 – 30.05.2027 · 33');
  });

  it('shows the add season button to staff only', () => {
    expect(render().textContent).not.toContain('Add season');
    TestBed.resetTestingModule();
    expect(render(of(league), true).textContent).toContain('Add season');
  });

  it('shows the not found state', () => {
    const element = render(throwError(() => new HttpErrorResponse({ status: 404 })));

    expect(element.textContent).toContain('Competition not found');
  });
});
