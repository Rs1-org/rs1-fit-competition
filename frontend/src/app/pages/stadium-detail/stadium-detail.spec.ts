import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA, signal } from '@angular/core';
import { registerLocaleData } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import localeBs from '@angular/common/locales/bs-Latn';
import { AppDayLabelPipe, AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { ActivatedRoute, Router, RouterLink, convertToParamMap, provideRouter } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';

import { StadiumDetail } from './stadium-detail';
import { StadiumService } from '../../core/services/stadium.service';
import { MatchService } from '../../core/services/match.service';
import { UserStateService } from '../../core/services/user-state.service';
import { StadiumDetailsDto } from '../../core/models/stadium';
import { MatchListDto, MatchListFilter } from '../../core/models/match';
import { PagedResult } from '../../core/models/paged-result';

registerLocaleData(localeBs);

describe('StadiumDetail', () => {
  const kosevo: StadiumDetailsDto = {
    publicId: 's1',
    name: 'Asim Ferhatović Hase',
    capacity: 34500,
    dateOfConstruction: '1947-01-01',
    slug: 'kosevo',
    pictureUrl: '',
    cityPublicId: 'c1',
    cityName: 'Sarajevo',
    latitude: 43.8686,
    longitude: 18.4093,
    geoDescription: 'Patriotske lige 35, main entrance from the north stand.',
    cityLatitude: 43.8563,
    cityLongitude: 18.4131,
  };

  const derby: MatchListDto = {
    publicId: 'm1', slug: 'derby', date: '2026-10-30T16:00:00Z', homeScore: 2, awayScore: 1, isPostponed: false,
    statusName: 'Finished', stadiumName: 'Asim Ferhatović Hase', homeSelectionName: 'Sarajevo', awaySelectionName: 'Zeljeznicar',
    roundPublicId: 'r1', roundName: 'Round 1', seasonName: '2026/27', competitionName: 'Premijer liga',
  };

  function paged(items: MatchListDto[], totalCount = items.length): PagedResult<MatchListDto> {
    return { items, totalCount, page: 1, pageSize: 10, totalPages: 1, hasNextPage: false, hasPreviousPage: false };
  }

  let getAll: ReturnType<typeof vi.fn>;

  async function setup(
    stadium: Observable<StadiumDetailsDto>,
    options: { matches?: PagedResult<MatchListDto>; played?: number; canManage?: boolean } = {},
  ): Promise<{ fixture: ComponentFixture<StadiumDetail>; page: HTMLElement }> {
    getAll = vi.fn((_page: number, _size: number, filter: MatchListFilter) =>
      of(filter.phase === 'results' ? paged([], options.played ?? 0) : options.matches ?? paged([])));
    await TestBed.configureTestingModule({
      imports: [StadiumDetail],
      providers: [
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { paramMap: of(convertToParamMap({ slug: 'kosevo' })) } },
        { provide: StadiumService, useValue: { getBySlug: () => stadium } },
        { provide: MatchService, useValue: { getAll } },
        { provide: UserStateService, useValue: { canManage: signal(options.canManage ?? false) } },
      ],
    })
      // Leaflet, the form and the shared tiles have their own specs; keep them out of this one.
      .overrideComponent(StadiumDetail, {
        set: { imports: [AppNumberPipe, AppDayLabelPipe, RouterLink], schemas: [CUSTOM_ELEMENTS_SCHEMA] },
      })
      .compileComponents();

    const fixture = TestBed.createComponent(StadiumDetail);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    return { fixture, page: fixture.nativeElement as HTMLElement };
  }

  it('shows the distance to the city centre in km with 1 decimal', async () => {
    const { fixture, page } = await setup(of(kosevo));

    expect(fixture.componentInstance.distanceToCityKm()).toBe(1.4);
    expect(page.querySelector('.distance')?.textContent).toContain('1,4 km');
  });

  it('hides the distance when the city has no coordinates', async () => {
    const { fixture, page } = await setup(of({ ...kosevo, cityLatitude: null, cityLongitude: null }));

    expect(fixture.componentInstance.distanceToCityKm()).toBeNull();
    expect(page.querySelector('.distance')).toBeNull();
  });

  it('fills the facts, directions and an Open in maps link', async () => {
    const { fixture, page } = await setup(of(kosevo), { played: 214 });
    const component = fixture.componentInstance;

    expect(component.builtYear()).toBe(1947);
    expect(component.playedCount()).toBe(214);
    expect(getAll).toHaveBeenCalledWith(1, 1, { stadiumPublicId: 's1', phase: 'results' });
    expect(component.coordinates()).toBe('43,8686° N, 18,4093° E');
    expect(page.querySelector('.geo')?.textContent).toContain('Patriotske lige 35');
    expect(page.querySelector('a[target="_blank"]')?.getAttribute('href')).toContain('mlat=43.8686&mlon=18.4093');
  });

  it('shows a labelled placeholder when there is no photo', async () => {
    const { page } = await setup(of(kosevo));

    expect(page.querySelector('.hero-img')).toBeNull();
    expect(page.querySelector('.hero-placeholder')?.textContent).toContain('No stadium photo yet');
  });

  it('lists the latest matches here, newest first', async () => {
    const { page } = await setup(of(kosevo), { matches: paged([derby], 12) });

    expect(getAll).toHaveBeenCalledWith(1, 5, { stadiumPublicId: 's1', sortBy: 'date', sortDir: 'desc' });
    const row = page.querySelector('a.match-row[href="/matches/derby"]');
    expect(row?.textContent).toContain('Sarajevo – Zeljeznicar');
    expect(row?.textContent).toContain('2 – 1');
    expect(page.querySelector('.matches-more a')?.getAttribute('href')).toBe('/matches');
  });

  it('shows an empty message and hides staff actions from visitors', async () => {
    const { page } = await setup(of(kosevo));

    expect(page.textContent).toContain('No matches scheduled here yet.');
    expect(page.querySelector('.hero-actions')).toBeNull();
  });

  it('opens the match form with this stadium picked for staff', async () => {
    const { fixture } = await setup(of(kosevo), { canManage: true });
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    expect(fixture.nativeElement.querySelector('.hero-actions')).not.toBeNull();
    fixture.componentInstance.scheduleMatch('s1');

    expect(navigate).toHaveBeenCalledWith(['/manage-matches'], { queryParams: { stadium: 's1' } });
  });

  it('follows the new slug after a rename', async () => {
    const { fixture } = await setup(of(kosevo), { canManage: true });
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    fixture.componentInstance.onSaved({ ...kosevo, slug: 'kosevo-arena' });

    expect(navigate).toHaveBeenCalledWith(['/stadium', 'kosevo-arena'], { replaceUrl: true });
  });

  it('tells a missing stadium apart from a failed load', async () => {
    const missing = await setup(throwError(() => new HttpErrorResponse({ status: 404 })));
    expect(missing.fixture.componentInstance.state().notFound).toBe(true);
    expect(missing.page.querySelector('app-empty-state')).not.toBeNull();

    TestBed.resetTestingModule();
    const failed = await setup(throwError(() => new HttpErrorResponse({ status: 500 })));
    expect(failed.fixture.componentInstance.state().error).not.toBe('');
    expect(failed.page.querySelector('app-error-banner')).not.toBeNull();
  });
});
