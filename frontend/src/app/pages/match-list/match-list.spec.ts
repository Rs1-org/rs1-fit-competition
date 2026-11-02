import { registerLocaleData } from '@angular/common';
import localeBs from '@angular/common/locales/bs-Latn';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { MatchList } from './match-list';
import { MatchService } from '../../core/services/match.service';
import { MatchStatusService } from '../../core/services/match-status.service';
import { StadiumService } from '../../core/services/stadium.service';
import { CompetitionService } from '../../core/services/competition.service';
import { UserStateService } from '../../core/services/user-state.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { MatchListDto } from '../../core/models/match';
import { emptyPage, PagedResult } from '../../core/models/paged-result';

registerLocaleData(localeBs);

function page<T>(items: T[]): PagedResult<T> {
  return { ...emptyPage<T>(), items, totalCount: items.length, totalPages: 1 };
}

const match: MatchListDto = {
  publicId: 'm1',
  slug: 'derby',
  date: '2026-10-25T17:00:00Z',
  homeScore: 2,
  awayScore: 1,
  isPostponed: false,
  statusName: 'Finished',
  stadiumName: 'Koševo',
  homeSelectionName: 'FK Sarajevo',
  awaySelectionName: 'FK Borac',
  roundPublicId: null,
  roundName: 'Round 12',
  seasonName: '2026/27',
  competitionName: 'Premijer liga',
};

function render(role: { manage: boolean; admin: boolean }, getAll = vi.fn(() => of(page([match])))) {
  const empty = { getAll: () => of(emptyPage()) };
  const confirm = vi.fn(async () => false);
  TestBed.configureTestingModule({
    imports: [MatchList],
    providers: [
      provideRouter([]),
      { provide: MatchService, useValue: { getAll } },
      { provide: MatchStatusService, useValue: { getAll: () => of(page([{ publicId: 's1', name: 'Live' }])) } },
      { provide: StadiumService, useValue: empty },
      { provide: CompetitionService, useValue: empty },
      { provide: UserStateService, useValue: { canManage: signal(role.manage), isAdmin: signal(role.admin) } },
      { provide: ConfirmDialogService, useValue: { confirm } },
    ],
  });
  const fixture = TestBed.createComponent(MatchList);
  fixture.detectChanges();
  return { fixture, component: fixture.componentInstance, element: fixture.nativeElement as HTMLElement, getAll, confirm };
}

describe('MatchList', () => {
  it('asks for the newest matches first', () => {
    const { getAll } = render({ manage: false, admin: false });

    expect(getAll).toHaveBeenCalledWith(1, 10, { sortBy: 'date', sortDir: 'desc' });
  });

  it('shows visitors no staff actions', () => {
    const { element } = render({ manage: false, admin: false });

    expect(element.textContent).not.toContain('Schedule match');
    expect(element.querySelector('app-import-export-bar')).toBeNull();
    expect(element.querySelector('[aria-label^="Edit"]')).toBeNull();
    expect(element.querySelector('[aria-label^="Delete"]')).toBeNull();
  });

  it('gives moderators edit and export but no delete or import', () => {
    const { element } = render({ manage: true, admin: false });

    expect(element.textContent).toContain('Schedule match');
    expect(element.textContent).toContain('Export Excel');
    expect(element.textContent).not.toContain('Import CSV');
    expect(element.querySelector('[aria-label^="Edit"]')).not.toBeNull();
    expect(element.querySelector('[aria-label^="Delete"]')).toBeNull();
  });

  it('gives admins delete behind the confirm dialog', async () => {
    const { component, confirm, element } = render({ manage: true, admin: true });

    expect(element.textContent).toContain('Import CSV');
    await component.delete(match);

    expect(confirm).toHaveBeenCalledTimes(1);
  });

  it('keeps the tab and sort when a filter is applied, back on page 1', () => {
    const { component, getAll } = render({ manage: false, admin: false });

    component.selectPhase('live');
    component.sortBy('home');
    component.filterForm.controls.statusPublicId.setValue('s1');
    component.apply();

    expect(getAll).toHaveBeenLastCalledWith(1, 10, expect.objectContaining({
      phase: 'live', sortBy: 'home', sortDir: 'asc', statusPublicId: 's1',
    }));
  });

  it('turns active filters into chips that can be removed', () => {
    const { component } = render({ manage: false, admin: false });

    component.filterForm.patchValue({ statusPublicId: 's1', dateFrom: '2026-10-01', postponed: 'true' });
    expect(component.chips().map((chip) => chip.label)).toEqual(['Live', 'From 01.10.2026', 'Postponed only']);

    component.clearChip('statusPublicId');
    expect(component.chips().map((chip) => chip.key)).toEqual(['dateFrom', 'postponed']);
  });

  it('shows the empty and error states', () => {
    const empty = render({ manage: false, admin: false }, vi.fn(() => of(page<MatchListDto>([]))));
    expect(empty.element.querySelector('app-empty-state')).not.toBeNull();

    TestBed.resetTestingModule();
    const failed = render({ manage: false, admin: false }, vi.fn(() => throwError(() => new Error('down'))));
    expect(failed.element.querySelector('app-error-banner')).not.toBeNull();
  });
});
