import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { FormControl } from '@angular/forms';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { ClubService } from '../../core/services/club.service';
import { CityService } from '../../core/services/city-services';
import { StadiumService } from '../../core/services/stadium.service';
import { UserStateService } from '../../core/services/user-state.service';
import { ClubList } from './club-list';
import { clubNameValidator, foundingDateValidator, ManageClub } from './manage-club';

describe('Club validation and API', () => {
  it('requires a name that can produce the backend slug', () => {
    expect(clubNameValidator(new FormControl('---'))).toEqual({ name: true });
    expect(clubNameValidator(new FormControl('Čelik'))).toBeNull();
    expect(clubNameValidator(new FormControl('   '))).toEqual({ name: true });
  });
  it('rejects impossible and future founding dates but accepts leap day', () => {
    expect(foundingDateValidator(new FormControl('2020-02-30'))).toEqual({ date: true });
    expect(foundingDateValidator(new FormControl('9999-01-01'))).toEqual({ future: true });
    expect(foundingDateValidator(new FormControl('2020-02-29'))).toBeNull();
  });
  it('sends all five filters, including the zero-valued U7 age group', () => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const http = TestBed.inject(HttpTestingController);
    TestBed.inject(ClubService).getAll(2, 10, { name: 'Club', cityId: 'city', stadiumId: 'stadium', foundingYear: 1950, clubAge: 0 }).subscribe();
    const request = http.expectOne(req => req.url.endsWith('/api/club'));
    for (const [key, value] of Object.entries({ page: '2', name: 'Club', cityId: 'city', stadiumId: 'stadium', foundingYear: '1950', clubAge: '0' }))
      expect(request.request.params.get(key)).toBe(value);
    request.flush({ items: [], totalCount: 0 }); http.verify();
  });
});

describe('Club screens', () => {
  const page = { items: [], totalCount: 0, page: 1, pageSize: 10, totalPages: 0, hasPreviousPage: false, hasNextPage: false };
  function providers(admin: boolean) {
    return [provideRouter([]),
      { provide: ActivatedRoute, useValue: { paramMap: of(convertToParamMap({})) } },
      { provide: UserStateService, useValue: { canManage: () => admin, isAdmin: () => admin } },
      { provide: CityService, useValue: { getAllForDropdown: () => of([]) } },
      { provide: StadiumService, useValue: { getAll: () => of(page) } },
      { provide: ClubService, useValue: { getAll: () => of(page) } },
    ];
  }
  it('shows empty state and keeps management hidden for visitors', async () => {
    await TestBed.configureTestingModule({ imports: [ClubList], providers: providers(false) }).compileComponents();
    const fixture = TestBed.createComponent(ClubList);
    fixture.detectChanges(); await fixture.whenStable(); fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('No clubs match');
    expect(fixture.nativeElement.textContent).not.toContain('Add club');
  });
  it('resets page to one when filters are applied', async () => {
    await TestBed.configureTestingModule({ imports: [ClubList], providers: providers(true) }).compileComponents();
    const fixture = TestBed.createComponent(ClubList); fixture.detectChanges();
    fixture.componentInstance.page = 4;
    fixture.componentInstance.filters.controls.clubAge.setValue('0');
    fixture.componentInstance.apply(); expect(fixture.componentInstance.page).toBe(1);
  });
  it('requires fields and rejects whitespace-only names and oversized names', async () => {
    await TestBed.configureTestingModule({ imports: [ManageClub], providers: providers(true) }).compileComponents();
    const fixture = TestBed.createComponent(ManageClub); fixture.detectChanges();
    const form = fixture.componentInstance.form;
    expect(form.invalid).toBe(true);
    form.controls.name.setValue('   '); expect(form.controls.name.invalid).toBe(true);
    form.controls.name.setValue('a'.repeat(101)); expect(form.controls.name.invalid).toBe(true);
  });
});
