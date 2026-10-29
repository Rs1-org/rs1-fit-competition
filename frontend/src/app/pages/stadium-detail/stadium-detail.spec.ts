import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA, signal } from '@angular/core';
import { registerLocaleData } from '@angular/common';
import localeBs from '@angular/common/locales/bs-Latn';
import { AppDatePipe, AppDateTimePipe, AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { ActivatedRoute, Router, RouterLink, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { StadiumDetail } from './stadium-detail';
import { StadiumService } from '../../core/services/stadium.service';
import { MatchService } from '../../core/services/match.service';
import { UserStateService } from '../../core/services/user-state.service';
import { StadiumDetailsDto } from '../../core/models/stadium';
import { MatchListDto } from '../../core/models/match';
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
    geoDescription: '',
    cityLatitude: 43.8563,
    cityLongitude: 18.4131,
  };

  const derby: MatchListDto = {
    publicId: 'm1', slug: 'derby', date: '2026-10-30T16:00:00Z', homeScore: null, awayScore: null, isPostponed: false,
    statusName: 'Scheduled', stadiumName: 'Asim Ferhatović Hase', homeSelectionName: 'Sarajevo', awaySelectionName: 'Zeljeznicar',
    roundPublicId: 'r1', roundName: 'Round 1', seasonName: '2026/27', competitionName: 'Premijer liga',
  };

  function paged(items: MatchListDto[], totalCount = items.length): PagedResult<MatchListDto> {
    return { items, totalCount, page: 1, pageSize: 10, totalPages: 1, hasNextPage: false, hasPreviousPage: false };
  }

  let getAll: ReturnType<typeof vi.fn>;

  async function setup(
    stadium: StadiumDetailsDto,
    matches: PagedResult<MatchListDto> = paged([]),
    canManage = false,
  ): Promise<ComponentFixture<StadiumDetail>> {
    getAll = vi.fn(() => of(matches));
    await TestBed.configureTestingModule({
      imports: [StadiumDetail],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { paramMap: of(convertToParamMap({ slug: stadium.slug })) },
        },
        { provide: StadiumService, useValue: { getBySlug: () => of(stadium) } },
        { provide: MatchService, useValue: { getAll } },
        { provide: UserStateService, useValue: { canManage: signal(canManage) } },
      ],
    })
      // The Leaflet map is covered by its own spec; keep it out of this one.
      .overrideComponent(StadiumDetail, {
        set: { imports: [AppNumberPipe, AppDatePipe, AppDateTimePipe, RouterLink], schemas: [CUSTOM_ELEMENTS_SCHEMA] },
      })
      .compileComponents();

    const fixture = TestBed.createComponent(StadiumDetail);
    fixture.detectChanges();
    await fixture.whenStable();
    return fixture;
  }

  it('shows the distance to the city centre in km with 1 decimal', async () => {
    const fixture = await setup(kosevo);

    expect(fixture.componentInstance.distanceToCityKm()).toBe(1.4);
    expect(fixture.nativeElement.querySelector('.distance').textContent).toContain('1,4 km');
  });

  it('hides the distance when the city has no coordinates', async () => {
    const fixture = await setup({ ...kosevo, cityLatitude: null, cityLongitude: null });

    expect(fixture.componentInstance.distanceToCityKm()).toBeNull();
    expect(fixture.nativeElement.querySelector('.distance')).toBeNull();
  });

  it('lists the matches at this stadium with their round', async () => {
    const fixture = await setup(kosevo, paged([derby]));
    const page = fixture.nativeElement as HTMLElement;

    expect(getAll).toHaveBeenCalledWith(1, 10, { stadiumPublicId: 's1' });
    const row = page.querySelector('a.match-row[href="/matches/derby"]');
    expect(row?.textContent).toContain('Sarajevo vs Zeljeznicar');
    expect(row?.textContent).toContain('Premijer liga · 2026/27 · Round 1');
  });

  it('shows an empty message and hides the schedule button from visitors', async () => {
    const fixture = await setup(kosevo);
    const page = fixture.nativeElement as HTMLElement;

    expect(page.textContent).toContain('No matches scheduled here yet.');
    expect(page.querySelector('.btn-schedule')).toBeNull();
  });

  it('opens the match form with this stadium picked for staff', async () => {
    const fixture = await setup(kosevo, paged([]), true);
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    (fixture.nativeElement.querySelector('.btn-schedule') as HTMLButtonElement).click();

    expect(navigate).toHaveBeenCalledWith(['/manage-matches'], { queryParams: { stadium: 's1' } });
  });
});
