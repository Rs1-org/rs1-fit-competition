import { Component, inject, signal } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, debounceTime, map, of, startWith, switchMap, tap } from 'rxjs';
import { AdminUserService } from '../../core/services/admin-user.service';
import { AdminUser, ASSIGNABLE_USER_ROLES, USER_FILTER_ROLES, UserFilter } from '../../core/models/admin-user';
import { PagedResult } from '../../core/models/paged-result';
import { apiErrorMessage } from '../../core/utils/api-error';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { Pagination } from '../../shared/components/pagination/pagination';
import { AppDatePipe, AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { userDateBoundary, userDateRangeValidator } from './user-date-filter';

interface UserPageState { loading: boolean; data: PagedResult<AdminUser> | null; error: string; }

@Component({
  selector: 'app-admin-users',
  imports: [ReactiveFormsModule, Pagination, AppDatePipe, AppNumberPipe],
  templateUrl: './admin-users.html',
  styleUrl: './admin-users.css',
})
export class AdminUsers {
  private readonly service = inject(AdminUserService);
  private readonly confirm = inject(ConfirmDialogService);
  private readonly fb = inject(FormBuilder);
  readonly roles = ASSIGNABLE_USER_ROLES;
  readonly filterRoles = USER_FILTER_ROLES;
  readonly pageSize = 10;
  readonly busy = signal<string | null>(null);
  readonly actionError = signal('');
  readonly actionMessage = signal('');
  readonly roleControls = new Map<string, FormControl<string>>();
  readonly filterForm = this.fb.nonNullable.group({
    search: ['', Validators.maxLength(256)], role: '', isActive: '', registeredFrom: '', registeredTo: '',
  }, { validators: userDateRangeValidator });
  private readonly query$ = new BehaviorSubject<{ page: number; filter: UserFilter }>({ page: 1, filter: {} });
  readonly state = toSignal(this.query$.pipe(
    switchMap(({ page, filter }) => this.service.getAll(page, this.pageSize, filter).pipe(
      tap(result => {
        this.roleControls.clear();
        for (const user of result.items) {
          const role = user.roles.includes('Admin') ? 'Admin' : user.roles[0] ?? 'User';
          this.roleControls.set(user.publicId, new FormControl(role, { nonNullable: true, validators: [
            Validators.required, control => this.roles.some(x => x === control.value) ? null : { role: true },
          ] }));
          if (this.busy()) this.roleControls.get(user.publicId)!.disable({ emitEvent: false });
        }
      }),
      map((data): UserPageState => ({ loading: false, data, error: '' })),
      startWith({ loading: true, data: null, error: '' } satisfies UserPageState),
      catchError(err => of({ loading: false, data: null, error: apiErrorMessage(err, 'Users could not be loaded.') } satisfies UserPageState)),
    )),
  ), { initialValue: { loading: true, data: null, error: '' } as UserPageState });

  constructor() {
    this.filterForm.valueChanges.pipe(debounceTime(250), takeUntilDestroyed()).subscribe(() => {
      if (this.filterForm.invalid) return;
      const value = this.filterForm.getRawValue();
      this.query$.next({ page: 1, filter: {
        search: value.search.trim(), role: value.role || undefined,
        isActive: value.isActive === '' ? undefined : value.isActive === 'true',
        registeredFrom: userDateBoundary(value.registeredFrom), registeredTo: userDateBoundary(value.registeredTo, true),
      } });
    });
  }

  resetFilters(): void { this.filterForm.reset(); }
  retry(): void { this.query$.next({ ...this.query$.value }); }
  goToPage(page: number): void {
    if (this.filterForm.valid) this.query$.next({ ...this.query$.value, page });
  }

  canSaveRole(user: AdminUser): boolean {
    const control = this.roleControls.get(user.publicId);
    return !user.isSelf && !this.busy() && !!control?.valid
      && !(user.roles.length === 1 && user.roles[0] === control.value);
  }

  isAssignable(role: string): boolean { return this.roles.some(x => x === role); }

  private setBusy(id: string | null): void {
    this.busy.set(id);
    for (const control of this.roleControls.values()) {
      if (id) control.disable({ emitEvent: false });
      else control.enable({ emitEvent: false });
    }
  }

  async changeRole(user: AdminUser): Promise<void> {
    if (!this.canSaveRole(user)) return;
    const role = this.roleControls.get(user.publicId)!.value;
    this.setBusy(user.publicId);
    this.actionError.set('');
    this.actionMessage.set('');
    if (!await this.confirm.confirm({ title: 'Change user role', message: `Set ${user.email}'s role to ${role}?`, confirmLabel: 'Change role' })) {
      this.setBusy(null); return;
    }
    this.service.changeRole(user.publicId, role).subscribe({
      next: () => this.saved(`Role updated for ${user.email}.`),
      error: err => this.failed(err),
    });
  }

  async setActive(user: AdminUser): Promise<void> {
    if (user.isSelf || this.busy()) return;
    const action = user.isActive ? 'Deactivate' : 'Reactivate';
    this.setBusy(user.publicId);
    this.actionError.set('');
    this.actionMessage.set('');
    if (!await this.confirm.confirm({ title: `${action} account`, message: `${action} ${user.email}'s account?`, confirmLabel: action })) {
      this.setBusy(null); return;
    }
    this.service.setActive(user.publicId, !user.isActive).subscribe({
      next: () => this.saved(`${user.email} ${user.isActive ? 'deactivated' : 'reactivated'}.`),
      error: err => this.failed(err),
    });
  }

  private saved(message: string): void {
    this.setBusy(null);
    this.actionMessage.set(message);
    this.query$.next({ ...this.query$.value, page: 1 });
  }
  private failed(error: unknown): void {
    this.setBusy(null);
    this.actionError.set(apiErrorMessage(error, 'The account could not be updated.'));
  }
}
