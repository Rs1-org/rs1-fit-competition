import { registerLocaleData } from '@angular/common';
import localeBs from '@angular/common/locales/bs-Latn';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { StadiumList } from './stadium-list';
import { StadiumService } from '../../core/services/stadium.service';
import { CountryService } from '../../core/services/country-service';
import { CityService } from '../../core/services/city-services';
import { UserStateService } from '../../core/services/user-state.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { StadiumListDto } from '../../core/models/stadium';
import { emptyPage, PagedResult } from '../../core/models/paged-result';

registerLocaleData(localeBs);

function page<T>(items: T[]): PagedResult<T> {
  return { ...emptyPage<T>(), items, totalCount: items.length, totalPages: 1 };
}

const grbavica: StadiumListDto = {
  publicId: 's1', name: 'Grbavica', capacity: 13146, dateOfConstruction: '1953-01-01', slug: 'grbavica',
  pictureUrl: '', cityName: 'Sarajevo', countryName: 'Bosnia and Herzegovina',
};

function render(role: { manage: boolean; admin: boolean }, getAll = vi.fn(() => of(page([grbavica])))) {
  const confirm = vi.fn(async () => false);
  TestBed.configureTestingModule({
    imports: [StadiumList],
    providers: [
      provideRouter([]),
      provideHttpClient(),
      provideHttpClientTesting(),
      { provide: StadiumService, useValue: { getAll } },
      { provide: CountryService, useValue: { getAllForDropdown: () => of([{ publicId: 'bih', name: 'Bosnia and Herzegovina' }]) } },
      { provide: CityService, useValue: { getAll: () => of(page([{ publicId: 'sa', name: 'Sarajevo' }])), getAllForDropdown: () => of([]) } },
      { provide: UserStateService, useValue: { canManage: signal(role.manage), isAdmin: signal(role.admin) } },
      { provide: ConfirmDialogService, useValue: { confirm } },
    ],
  });
  const fixture = TestBed.createComponent(StadiumList);
  fixture.detectChanges();
  return { fixture, component: fixture.componentInstance, element: fixture.nativeElement as HTMLElement, getAll, confirm };
}

describe('StadiumList', () => {
  it('shows the stadium with its capacity in the regional format and a photo placeholder', () => {
    const { element } = render({ manage: false, admin: false });

    expect(element.querySelector('tbody')?.textContent).toContain('Grbavica');
    expect(element.querySelector('td.num')?.textContent).toContain('13.146');
    expect(element.querySelector('.picture-placeholder')?.textContent).toContain('No photo');
  });

  it('shows visitors export but no staff actions', () => {
    const { element } = render({ manage: false, admin: false });

    expect(element.textContent).not.toContain('Add stadium');
    expect(element.querySelector('app-import-export-bar')).not.toBeNull();
    expect(element.querySelector('[aria-label^="Edit"]')).toBeNull();
    expect(element.querySelector('[aria-label^="Delete"]')).toBeNull();
  });

  it('gives moderators add and edit but no delete', () => {
    const { element } = render({ manage: true, admin: false });

    expect(element.textContent).toContain('Add stadium');
    expect(element.querySelector('[aria-label="Edit Grbavica"]')).not.toBeNull();
    expect(element.querySelector('[aria-label^="Delete"]')).toBeNull();
  });

  it('gives admins delete behind the confirm dialog', async () => {
    const { component, confirm, element } = render({ manage: true, admin: true });

    expect(element.querySelector('[aria-label="Delete Grbavica"]')).not.toBeNull();
    await component.delete(grbavica);

    expect(confirm).toHaveBeenCalledTimes(1);
  });

  it('sends all five filters with the sort, back on page 1', () => {
    const { component, getAll } = render({ manage: false, admin: false });

    component.sortBy('capacity');
    component.filterForm.patchValue({ name: ' Grb ', countryPublicId: 'bih' });
    component.filterForm.patchValue({ cityPublicId: 'sa', minCapacity: 1000, maxCapacity: 20000 });
    component.apply();

    expect(getAll).toHaveBeenLastCalledWith(1, 10, {
      name: 'Grb', countryPublicId: 'bih', cityPublicId: 'sa', minCapacity: 1000, maxCapacity: 20000, sortBy: 'capacity', sortDir: 'asc',
    });
    expect(component.sortNote()).toBe('Sorted by capacity, smallest first');
  });

  it('does not search with a reversed capacity range', () => {
    const { component, getAll } = render({ manage: false, admin: false });
    const calls = getAll.mock.calls.length;

    component.filterForm.patchValue({ minCapacity: 5000, maxCapacity: 100 });
    component.apply();

    expect(component.filterForm.hasError('capacityRange')).toBe(true);
    expect(getAll.mock.calls.length).toBe(calls);
  });

  it('turns active filters into chips that can be removed', () => {
    const { component } = render({ manage: false, admin: false });

    component.filterForm.patchValue({ name: 'Grb', countryPublicId: 'bih' });
    component.filterForm.patchValue({ minCapacity: 10000 });
    expect(component.chips().map((chip) => chip.label)).toEqual(['"Grb"', 'Bosnia and Herzegovina', 'At least 10.000 seats']);

    component.clearChip('minCapacity');
    expect(component.filterForm.controls.minCapacity.value).toBeNull();
    expect(component.chips().map((chip) => chip.key)).toEqual(['name', 'countryPublicId']);
  });

  it('shows the empty and error states', () => {
    const empty = render({ manage: false, admin: false }, vi.fn(() => of(page<StadiumListDto>([]))));
    expect(empty.element.querySelector('app-empty-state')).not.toBeNull();

    TestBed.resetTestingModule();
    const failed = render({ manage: false, admin: false }, vi.fn(() => throwError(() => new Error('down'))));
    expect(failed.element.querySelector('app-error-banner')).not.toBeNull();
  });
});
