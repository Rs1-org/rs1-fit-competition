import { registerLocaleData } from '@angular/common';
import localeBs from '@angular/common/locales/bs-Latn';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { CityList } from './city-list';
import { CityService } from '../../core/services/city-services';
import { CountryService } from '../../core/services/country-service';
import { UserStateService } from '../../core/services/user-state.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { CityListDto } from '../../core/models/city';
import { emptyPage, PagedResult } from '../../core/models/paged-result';

registerLocaleData(localeBs);

function page<T>(items: T[]): PagedResult<T> {
  return { ...emptyPage<T>(), items, totalCount: items.length, totalPages: 1 };
}

const sarajevo = {
  publicId: 'sa', name: 'Sarajevo', slug: 'sarajevo', population: 275524, isCapital: true, countryName: 'Bosnia and Herzegovina',
} as CityListDto;

function render(role: { manage: boolean; admin: boolean }, getAll = vi.fn(() => of(page([sarajevo])))) {
  const confirm = vi.fn(async () => false);
  TestBed.configureTestingModule({
    imports: [CityList],
    providers: [
      provideRouter([]),
      { provide: CityService, useValue: { getAll } },
      { provide: CountryService, useValue: { getAllForDropdown: () => of([{ publicId: 'bih', name: 'Bosnia and Herzegovina' }]) } },
      { provide: UserStateService, useValue: { canManage: signal(role.manage), isAdmin: signal(role.admin) } },
      { provide: ConfirmDialogService, useValue: { confirm } },
    ],
  });
  const fixture = TestBed.createComponent(CityList);
  fixture.detectChanges();
  return { fixture, component: fixture.componentInstance, element: fixture.nativeElement as HTMLElement, getAll, confirm };
}

describe('CityList', () => {
  it('shows the city with its population in the regional format and the capital badge', () => {
    const { element } = render({ manage: false, admin: false });

    expect(element.querySelector('td.num')?.textContent).toContain('275.524');
    expect(element.querySelector('.badge-capital')?.textContent).toContain('Capital');
  });

  it('shows staff actions by role', async () => {
    const visitor = render({ manage: false, admin: false });
    expect(visitor.element.textContent).not.toContain('Add city');
    expect(visitor.element.querySelector('[aria-label^="Edit"]')).toBeNull();

    TestBed.resetTestingModule();
    const moderator = render({ manage: true, admin: false });
    expect(moderator.element.textContent).toContain('Add city');
    expect(moderator.element.querySelector('[aria-label="Edit Sarajevo"]')).not.toBeNull();
    expect(moderator.element.querySelector('[aria-label^="Delete"]')).toBeNull();

    TestBed.resetTestingModule();
    const admin = render({ manage: true, admin: true });
    await admin.component.onDelete(sarajevo);
    expect(admin.confirm).toHaveBeenCalledTimes(1);
  });

  it('sends all five filters with the sort, back on page 1', () => {
    const { component, getAll } = render({ manage: false, admin: false });

    component.sortBy('population');
    component.sortBy('population');
    component.filterForm.patchValue({ name: 'Sar', countryPublicId: 'bih', isCapital: 'true', minPopulation: 1000, maxPopulation: 500000 });
    component.apply();

    expect(getAll).toHaveBeenLastCalledWith(1, 10, {
      name: 'Sar', countryPublicId: 'bih', isCapital: true, minPopulation: 1000, maxPopulation: 500000, sortBy: 'population', sortDir: 'desc',
    });
    expect(component.sortNote()).toBe('Sorted by population, largest first');
  });

  it('turns active filters into chips that can be removed', () => {
    const { component } = render({ manage: false, admin: false });

    component.filterForm.patchValue({ countryPublicId: 'bih', isCapital: 'false', maxPopulation: 100000 });
    expect(component.chips().map((chip) => chip.label)).toEqual(['Bosnia and Herzegovina', 'Not capital', 'Population to 100.000']);

    component.clearChip('isCapital');
    expect(component.chips().map((chip) => chip.key)).toEqual(['countryPublicId', 'maxPopulation']);
  });

  it('shows the empty and error states', () => {
    const empty = render({ manage: false, admin: false }, vi.fn(() => of(page<CityListDto>([]))));
    expect(empty.element.querySelector('app-empty-state')).not.toBeNull();

    TestBed.resetTestingModule();
    const failed = render({ manage: false, admin: false }, vi.fn(() => throwError(() => new Error('down'))));
    expect(failed.element.querySelector('app-error-banner')).not.toBeNull();
  });
});
