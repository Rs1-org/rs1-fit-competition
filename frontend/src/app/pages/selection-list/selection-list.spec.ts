import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { signal } from '@angular/core';
import { of } from 'rxjs';
import { SelectionList } from './selection-list';
import { SelectionService } from '../../core/services/selection.service';
import { ClubService } from '../../core/services/club.service';
import { CountryService } from '../../core/services/country-service';
import { UserStateService } from '../../core/services/user-state.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { emptyPage } from '../../core/models/paged-result';

describe('SelectionList club filter', () => {
  it('resets server paging on club changes and reset', () => {
    vi.useFakeTimers();
    const getAll = vi.fn().mockReturnValue(of(emptyPage()));
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: SelectionService, useValue: { getAll } },
        { provide: ClubService, useValue: { getAll: () => of(emptyPage()) } },
        { provide: CountryService, useValue: { getAll: () => of(emptyPage()) } },
        {
          provide: UserStateService,
          useValue: { roles$: of([]), canManagePersons: () => false, isAdmin: signal(false) },
        },
        { provide: ConfirmDialogService, useValue: {} },
      ],
    });
    try {
      const component = TestBed.runInInjectionContext(() => new SelectionList());
      component.onPageChange(3);
      component.filterForm.controls.clubPublicId.setValue('a'.repeat(32));
      vi.advanceTimersByTime(300);
      expect(getAll).toHaveBeenLastCalledWith(
        1,
        10,
        expect.objectContaining({ clubPublicId: 'a'.repeat(32) }),
      );
      component.resetFilters();
      vi.advanceTimersByTime(300);
      expect(getAll).toHaveBeenLastCalledWith(
        1,
        10,
        expect.objectContaining({ clubPublicId: undefined }),
      );
    } finally {
      vi.useRealTimers();
    }
  });
});
