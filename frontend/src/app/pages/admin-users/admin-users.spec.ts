import { TestBed } from '@angular/core/testing';
import { registerLocaleData } from '@angular/common';
import localeBs from '@angular/common/locales/bs-Latn';
import { HttpErrorResponse } from '@angular/common/http';
import { of, Subject, throwError } from 'rxjs';
import { AdminUsers } from './admin-users';
import { AdminUserService } from '../../core/services/admin-user.service';
import { AdminUser } from '../../core/models/admin-user';
import { emptyPage } from '../../core/models/paged-result';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';

registerLocaleData(localeBs);

describe('Admin user management', () => {
  const user: AdminUser = {
    publicId: 'a'.repeat(32), email: 'dev@example.invalid', firstName: 'Dev', lastName: 'Test',
    isActive: true, registeredAt: null, roles: ['User'], isSelf: false,
  };
  const getAll = vi.fn(), changeRole = vi.fn(), setActive = vi.fn(), confirm = vi.fn();
  let component: AdminUsers;
  beforeEach(() => {
    vi.useFakeTimers();
    getAll.mockReset().mockReturnValue(of({ ...emptyPage<AdminUser>(), items: [user], totalCount: 1 }));
    changeRole.mockReset().mockReturnValue(of(true));
    setActive.mockReset().mockReturnValue(of(true));
    confirm.mockReset().mockResolvedValue(true);
    TestBed.configureTestingModule({ providers: [
      { provide: AdminUserService, useValue: { getAll, changeRole, setActive } },
      { provide: ConfirmDialogService, useValue: { confirm } },
    ] });
    component = TestBed.runInInjectionContext(() => new AdminUsers());
  });
  afterEach(() => { TestBed.resetTestingModule(); vi.useRealTimers(); });

  it('sends all five filters to the server and resets paging, preserving inactive=false', () => {
    component.goToPage(3);
    component.filterForm.patchValue({ search: ' Dev ', role: 'Moderator', isActive: 'false', registeredFrom: '2025-01-01', registeredTo: '2025-01-31' });
    vi.advanceTimersByTime(300);
    expect(getAll).toHaveBeenLastCalledWith(1, 10, {
      search: 'Dev', role: 'Moderator', isActive: false,
      registeredFrom: new Date(2025, 0, 1).toISOString(),
      registeredTo: new Date(2025, 0, 31, 23, 59, 59, 999).toISOString(),
    });
  });

  it('blocks invalid calendar dates and backwards ranges before loading', () => {
    const calls = getAll.mock.calls.length;
    component.filterForm.patchValue({ registeredFrom: '2025-02-31' });
    vi.advanceTimersByTime(300);
    expect(component.filterForm.invalid).toBe(true);
    expect(getAll).toHaveBeenCalledTimes(calls);
    component.filterForm.patchValue({ registeredFrom: '2025-03-01', registeredTo: '2025-02-01' });
    vi.advanceTimersByTime(300);
    expect(getAll).toHaveBeenCalledTimes(calls);
  });

  it('waits for confirmation and prevents duplicate role submissions', async () => {
    const response = new Subject<boolean>();
    changeRole.mockReturnValue(response);
    component.roleControls.get(user.publicId)!.setValue('Moderator');
    await component.changeRole(user);
    await component.changeRole(user);
    expect(confirm).toHaveBeenCalledOnce();
    expect(changeRole).toHaveBeenCalledExactlyOnceWith(user.publicId, 'Moderator');
    expect(component.busy()).toBe(user.publicId);
    response.next(true); response.complete();
    expect(component.busy()).toBeNull();
    expect(component.actionMessage()).toContain(user.email);
  });

  it('does not change an account when confirmation is cancelled or it is the current user', async () => {
    confirm.mockResolvedValue(false);
    await component.setActive(user);
    expect(setActive).not.toHaveBeenCalled();
    expect(component.busy()).toBeNull();
    const self = { ...user, isSelf: true };
    await component.setActive(self);
    await component.changeRole(self);
    expect(changeRole).not.toHaveBeenCalled();
    expect(confirm).toHaveBeenCalledOnce();
  });

  it('shows mutation validation errors and allows another attempt', async () => {
    setActive.mockReturnValueOnce(throwError(() => new HttpErrorResponse({ status: 400, error: {
      errors: [{ propertyName: 'PublicId', errorMessage: 'You cannot change your own account.' }],
    } })));
    await component.setActive(user);
    expect(component.actionError()).toContain('cannot change');
    expect(component.busy()).toBeNull();
    await component.setActive(user);
    expect(setActive).toHaveBeenCalledTimes(2);
  });

  it('keeps load failures distinct from empty data and supports retry', () => {
    getAll.mockReturnValueOnce(throwError(() => new Error('offline')));
    component.retry();
    expect(component.state().error).toBeTruthy();
    expect(component.state().data).toBeNull();
    component.retry();
    expect(component.state().error).toBe('');
    expect(component.state().data?.items).toEqual([user]);
  });

  it('renders an honest unknown registration date and hides all self actions', () => {
    getAll.mockReturnValue(of({ ...emptyPage<AdminUser>(), items: [{ ...user, isSelf: true }], totalCount: 1 }));
    const fixture = TestBed.createComponent(AdminUsers);
    fixture.detectChanges();
    const row: HTMLElement = fixture.nativeElement.querySelector('tbody tr');
    expect(row.textContent).toContain('Not recorded');
    expect(row.textContent).toContain('Your account');
    expect(row.querySelector('button, select')).toBeNull();
  });
});
