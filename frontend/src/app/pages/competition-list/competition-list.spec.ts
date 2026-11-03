import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { CompetitionList } from './competition-list';
import { CompetitionService } from '../../core/services/competition.service';
import { CountryService } from '../../core/services/country-service';
import { UserStateService } from '../../core/services/user-state.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { CompetitionListDto } from '../../core/models/competition';
import { emptyPage, PagedResult } from '../../core/models/paged-result';

function page<T>(items: T[]): PagedResult<T> {
  return { ...emptyPage<T>(), items, totalCount: items.length, totalPages: 1 };
}

const league: CompetitionListDto = {
  publicId: 'c1', name: 'Premijer liga BiH', slug: 'premijer-liga', shortName: 'PL', countryName: 'Bosnia and Herzegovina', seasonCount: 2,
};

function render(role: { manage: boolean; admin: boolean }, getAll = vi.fn(() => of(page([league])))) {
  const confirm = vi.fn(async () => false);
  TestBed.configureTestingModule({
    imports: [CompetitionList],
    providers: [
      provideRouter([]),
      { provide: CompetitionService, useValue: { getAll } },
      { provide: CountryService, useValue: { getAllForDropdown: () => of([{ publicId: 'bih', name: 'Bosnia and Herzegovina' }]) } },
      { provide: UserStateService, useValue: { canManage: signal(role.manage), isAdmin: signal(role.admin) } },
      { provide: ConfirmDialogService, useValue: { confirm } },
    ],
  });
  const fixture = TestBed.createComponent(CompetitionList);
  fixture.detectChanges();
  return { component: fixture.componentInstance, element: fixture.nativeElement as HTMLElement, getAll, confirm };
}

describe('CompetitionList', () => {
  it('shows each competition as a card linking to its page', () => {
    const { element } = render({ manage: false, admin: false });

    const card = element.querySelector('a.entity-card-link[href="/competitions/premijer-liga"]');
    expect(card?.textContent).toContain('Premijer liga BiH');
    expect(card?.textContent).toContain('Bosnia and Herzegovina · 2');
    expect(element.textContent).not.toContain('Add competition');
    expect(element.querySelector('[aria-label^="Edit"]')).toBeNull();
  });

  it('gives staff add and edit, and only admins delete', async () => {
    const moderator = render({ manage: true, admin: false });
    expect(moderator.element.textContent).toContain('Add competition');
    expect(moderator.element.querySelector('[aria-label="Edit Premijer liga BiH"]')).not.toBeNull();
    expect(moderator.element.querySelector('[aria-label^="Delete"]')).toBeNull();

    TestBed.resetTestingModule();
    const admin = render({ manage: true, admin: true });
    await admin.component.onDelete(league);
    expect(admin.confirm).toHaveBeenCalledTimes(1);
  });

  it('searches by name and country, back on page 1, with chips', () => {
    const { component, getAll } = render({ manage: false, admin: false });

    component.filterForm.setValue({ search: ' liga ', countryPublicId: 'bih' });
    component.apply();

    expect(getAll).toHaveBeenLastCalledWith(1, 12, 'liga', 'bih');
    expect(component.chips().map((chip) => chip.label)).toEqual(['"liga"', 'Bosnia and Herzegovina']);
  });

  it('shows the empty and error states', () => {
    const empty = render({ manage: false, admin: false }, vi.fn(() => of(page<CompetitionListDto>([]))));
    expect(empty.element.querySelector('app-empty-state')).not.toBeNull();

    TestBed.resetTestingModule();
    const failed = render({ manage: false, admin: false }, vi.fn(() => throwError(() => new Error('down'))));
    expect(failed.element.querySelector('app-error-banner')).not.toBeNull();
  });
});
