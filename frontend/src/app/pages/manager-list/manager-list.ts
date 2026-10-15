import { Component, inject } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { AppDatePipe } from '../../shared/pipes/regional-format.pipes';
import { Router } from '@angular/router';
import { emptyPage } from '../../core/models/paged-result';
import { Manager } from '../../core/models/person.models';
import { UserStateService } from '../../core/services/user-state.service';
import { ManagerService, StaffListFilter } from '../../core/services/manager.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { BehaviorSubject, combineLatest, debounceTime, filter, map, switchMap } from 'rxjs';

function ageRangeValidator(group: AbstractControl): ValidationErrors | null {
  const { ageMin, ageMax } = group.value;
  return ageMin != null && ageMax != null && ageMax < ageMin ? { ageRange: true } : null;
}

@Component({
  selector: 'app-manager-list',
  imports: [GenericListComponent, CommonModule, ReactiveFormsModule, AppDatePipe],
  templateUrl: './manager-list.html',
  styleUrls: ['../catalog.css', './manager-list.css'],
})
export class ManagerList {
  private managerService = inject(ManagerService);
  private confirmDialog = inject(ConfirmDialogService);
  private router = inject(Router);
  private userState = inject(UserStateService);
  private fb = inject(FormBuilder);

  private refresh$ = new BehaviorSubject<void>(undefined);
  private query$ = new BehaviorSubject<{ page: number; filter: StaffListFilter }>({ page: 1, filter: {} });
  readonly pageSize = 10;

  filterForm = this.fb.group({
    name: this.fb.nonNullable.control('', Validators.maxLength(100)),
    ageMin: this.fb.control<number | null>(null, Validators.min(0)),
    ageMax: this.fb.control<number | null>(null, Validators.min(0)),
    careerStartFrom: this.fb.nonNullable.control(''),
    careerStartTo: this.fb.nonNullable.control(''),
  }, { validators: ageRangeValidator });

  result = toSignal(
    combineLatest([this.query$, this.refresh$, this.managerService.listRefreshChanges$]).pipe(
      switchMap(([{ page, filter }]) => this.managerService.getAll(page, this.pageSize, filter)),
    ),
    { initialValue: emptyPage<Manager>() },
  );

  canManagePersons = toSignal(
    this.userState.roles$.pipe(map(() => this.userState.canManagePersons())),
    { initialValue: this.userState.canManagePersons() },
  );

  constructor() {
    this.filterForm.valueChanges.pipe(
      debounceTime(300),
      filter(() => this.filterForm.valid),
      takeUntilDestroyed(),
    ).subscribe(() => this.query$.next({ page: 1, filter: this.toFilter() }));
  }

  add(): void {
    this.router.navigate(['/manage-managers']);
  }

  view(manager: Manager): void {
    this.router.navigate(['/manager', manager.id]);
  }

  edit(manager: Manager): void {
    this.router.navigate(['/manage-managers', manager.id]);
  }

  async delete(manager: Manager): Promise<void> {
    if (!manager.id) return;

    const confirmed = await this.confirmDialog.confirm({
      message: `Delete manager ${manager.firstName} ${manager.lastName}?`,
    });
    if (!confirmed) return;

    this.managerService.delete(manager.id).subscribe({
      next: () => this.refresh$.next(),
      error: (err) => console.error('Could not delete the manager:', err),
    });
  }

  resetFilters(): void {
    this.filterForm.reset({ name: '', careerStartFrom: '', careerStartTo: '' });
  }

  onPageChange(page: number): void {
    this.query$.next({ ...this.query$.value, page });
  }

  private toFilter(): StaffListFilter {
    const { name, ageMin, ageMax, careerStartFrom, careerStartTo } = this.filterForm.getRawValue();
    return {
      name: name.trim() || undefined,
      ageMin: ageMin ?? undefined,
      ageMax: ageMax ?? undefined,
      careerStartFrom: careerStartFrom || undefined,
      careerStartTo: careerStartTo || undefined,
    };
  }
}
