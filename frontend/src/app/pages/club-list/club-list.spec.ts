import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { of, throwError } from 'rxjs';
import { ClubList } from './club-list';
import { ClubService } from '../../core/services/club.service';
import { CityService } from '../../core/services/city-services';
import { CountryService } from '../../core/services/country-service';
import { UserStateService } from '../../core/services/user-state.service';
import { emptyPage } from '../../core/models/paged-result';
import { ClubListDto } from '../../core/models/club';

describe('ClubList', () => {
  const getAll = vi.fn();
  let component: ClubList;
  beforeEach(() => {
    vi.useFakeTimers();
    getAll.mockReset().mockReturnValue(of(emptyPage<ClubListDto>()));
    TestBed.configureTestingModule({
      providers: [
        { provide: ClubService, useValue: { getAll } },
        { provide: CityService, useValue: { getAll: () => of(emptyPage()) } },
        { provide: CountryService, useValue: { getAllForDropdown: () => of([]) } },
        {
          provide: UserStateService,
          useValue: { canManage: signal(false), isAdmin: signal(false) },
        },
      ],
    });
    component = TestBed.runInInjectionContext(() => new ClubList());
  });
  afterEach(() => {
    TestBed.resetTestingModule();
    vi.useRealTimers();
  });

  it('resets server paging when a filter changes and preserves false/zero filters', () => {
    component.onPageChange(3);
    component.filterForm.patchValue({ name: ' Club ', hasStadium: 'false', clubAge: '0' });
    vi.advanceTimersByTime(300);
    expect(getAll).toHaveBeenLastCalledWith(
      1,
      10,
      expect.objectContaining({ name: 'Club', hasStadium: false, clubAge: 0 }),
    );
  });

  it('clears the selected city when the country changes', () => {
    component.filterForm.patchValue({ cityPublicId: 'a'.repeat(32) });
    component.filterForm.controls.countryPublicId.setValue('b'.repeat(32));
    vi.advanceTimersByTime(300);
    expect(component.filterForm.controls.cityPublicId.value).toBe('');
    expect(getAll).toHaveBeenLastCalledWith(
      1,
      10,
      expect.objectContaining({ cityPublicId: undefined, countryPublicId: 'b'.repeat(32) }),
    );
  });

  it('shows load failures and supports retry without terminating the search stream', () => {
    getAll.mockReturnValueOnce(throwError(() => new Error('offline')));
    component.retry();
    expect(component.state().error).toBeTruthy();
    expect(component.state().data).toBeNull();
    component.retry();
    expect(component.state().error).toBe('');
    expect(component.state().data).not.toBeNull();
  });
});
