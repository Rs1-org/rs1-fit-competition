import { registerLocaleData } from '@angular/common';
import localeBs from '@angular/common/locales/bs-Latn';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { CountryList } from './country-list';
import { CountryService } from '../../core/services/country-service';
import { UserStateService } from '../../core/services/user-state.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { CountryListDto } from '../../core/models/country';
import { emptyPage, PagedResult } from '../../core/models/paged-result';

registerLocaleData(localeBs);

function page<T>(items: T[]): PagedResult<T> {
  return { ...emptyPage<T>(), items, totalCount: items.length, totalPages: 1 };
}

const bosnia = {
  publicId: 'bih', name: 'Bosnia and Herzegovina', slug: 'bosnia-and-herzegovina', pictureUrl: '', createdAt: '2026-10-01T10:00:00Z',
} as CountryListDto;

function render(role: { manage: boolean; admin: boolean }, getAll = vi.fn(() => of(page([bosnia])))) {
  const confirm = vi.fn(async () => false);
  TestBed.configureTestingModule({
    imports: [CountryList],
    providers: [
      provideRouter([]),
      { provide: CountryService, useValue: { getAll } },
      { provide: UserStateService, useValue: { canManage: signal(role.manage), isAdmin: signal(role.admin) } },
      { provide: ConfirmDialogService, useValue: { confirm } },
    ],
  });
  const fixture = TestBed.createComponent(CountryList);
  fixture.detectChanges();
  return { fixture, component: fixture.componentInstance, element: fixture.nativeElement as HTMLElement, getAll, confirm };
}

describe('CountryList', () => {
  it('shows the country with the regional date and a flag placeholder', () => {
    const { element } = render({ manage: false, admin: false });

    expect(element.querySelector('tbody')?.textContent).toContain('01.10.2026');
    expect(element.querySelector('.picture-placeholder')?.textContent).toContain('No flag');
  });

  it('shows staff actions by role', async () => {
    const visitor = render({ manage: false, admin: false });
    expect(visitor.element.textContent).not.toContain('Add country');
    expect(visitor.element.querySelector('[aria-label^="Edit"]')).toBeNull();

    TestBed.resetTestingModule();
    const moderator = render({ manage: true, admin: false });
    expect(moderator.element.textContent).toContain('Add country');
    expect(moderator.element.querySelector('[aria-label^="Delete"]')).toBeNull();

    TestBed.resetTestingModule();
    const admin = render({ manage: true, admin: true });
    await admin.component.onDelete(bosnia);
    expect(admin.confirm).toHaveBeenCalledTimes(1);
  });

  it('sends all five filters with the sort, back on page 1', () => {
    const { component, getAll } = render({ manage: false, admin: false });

    component.sortBy('createdAt');
    component.filterForm.patchValue({ name: 'Bos', slug: 'bos', hasPicture: 'false', createdFrom: '2026-01-01', createdTo: '2026-12-31' });
    component.apply();

    expect(getAll).toHaveBeenLastCalledWith(1, 10, {
      name: 'Bos', slug: 'bos', hasPicture: false, createdFrom: '2026-01-01', createdTo: '2026-12-31', sortBy: 'createdAt', sortDir: 'asc',
    });
    expect(component.sortNote()).toBe('Sorted by date added, oldest first');
  });

  it('turns active filters into chips that can be removed', () => {
    const { component } = render({ manage: false, admin: false });

    component.filterForm.patchValue({ hasPicture: 'true', createdFrom: '2026-10-01' });
    expect(component.chips().map((chip) => chip.label)).toEqual(['With picture', 'Added from 01.10.2026']);

    component.clearChip('createdFrom');
    expect(component.chips().map((chip) => chip.key)).toEqual(['hasPicture']);
  });

  it('shows the empty and error states', () => {
    const empty = render({ manage: false, admin: false }, vi.fn(() => of(page<CountryListDto>([]))));
    expect(empty.element.querySelector('app-empty-state')).not.toBeNull();

    TestBed.resetTestingModule();
    const failed = render({ manage: false, admin: false }, vi.fn(() => throwError(() => new Error('down'))));
    expect(failed.element.querySelector('app-error-banner')).not.toBeNull();
  });
});
