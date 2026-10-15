import { Component, inject } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { AppDatePipe } from '../../shared/pipes/regional-format.pipes';
import { Router } from '@angular/router';
import { emptyPage } from '../../core/models/paged-result';
import { Referee } from '../../core/models/person.models';
import { UserStateService } from '../../core/services/user-state.service';
import { RefereeListFilter, RefereeService } from '../../core/services/referee.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { PersonAutocomplete } from '../../shared/components/person-autocomplete/person-autocomplete';
import { HasRoleDirective } from '../../shared/directives/has-role.directive';
import { BehaviorSubject, combineLatest, debounceTime, filter, map, switchMap } from 'rxjs';

function ageRangeValidator(group: AbstractControl): ValidationErrors | null {
  const { ageMin, ageMax } = group.value;
  return ageMin != null && ageMax != null && ageMax < ageMin ? { ageRange: true } : null;
}

@Component({
  selector: 'app-referee-list',
  imports: [GenericListComponent, CommonModule, ReactiveFormsModule, AppDatePipe, PersonAutocomplete, HasRoleDirective],
  templateUrl: './referee-list.html',
  styleUrls: ['../catalog.css', './referee-list.css'],
})
export class RefereeList {
  private refereeService = inject(RefereeService);

  searchReferees = (term: string) =>
    this.refereeService.getAll(1, 8, { name: term }).pipe(
      map((page) => page.items.map((referee) => ({
        id: referee.id ?? '',
        label: `${referee.firstName} ${referee.lastName}`.trim(),
      }))),
    );
  private confirmDialog = inject(ConfirmDialogService);
  private router = inject(Router);
  private userState = inject(UserStateService);
  private fb = inject(FormBuilder);

  private refresh$ = new BehaviorSubject<void>(undefined);
  private query$ = new BehaviorSubject<{ page: number; filter: RefereeListFilter }>({ page: 1, filter: {} });
  readonly pageSize = 10;

  filterForm = this.fb.group({
    name: this.fb.nonNullable.control('', Validators.maxLength(100)),
    ageMin: this.fb.control<number | null>(null, Validators.min(0)),
    ageMax: this.fb.control<number | null>(null, Validators.min(0)),
    careerStartFrom: this.fb.nonNullable.control(''),
    careerStartTo: this.fb.nonNullable.control(''),
  }, { validators: ageRangeValidator });

  result = toSignal(
    combineLatest([this.query$, this.refresh$, this.refereeService.listRefreshChanges$]).pipe(
      switchMap(([{ page, filter }]) => this.refereeService.getAll(page, this.pageSize, filter)),
    ),
    { initialValue: emptyPage<Referee>() },
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
    this.router.navigate(['/manage-referees']);
  }

  view(referee: Referee): void {
    this.router.navigate(['/referee', referee.id]);
  }

  edit(referee: Referee): void {
    this.router.navigate(['/manage-referees', referee.id]);
  }

  async delete(referee: Referee): Promise<void> {
    if (!referee.id) return;

    const confirmed = await this.confirmDialog.confirm({
      message: `Delete referee ${referee.firstName} ${referee.lastName}?`,
    });
    if (!confirmed) return;

    this.refereeService.delete(referee.id).subscribe({
      next: () => this.refresh$.next(),
      error: (err) => console.error('Could not delete the referee:', err),
    });
  }

  resetFilters(): void {
    this.filterForm.reset({ name: '', careerStartFrom: '', careerStartTo: '' });
  }

  onPageChange(page: number): void {
    this.query$.next({ ...this.query$.value, page });
  }

  private toFilter(): RefereeListFilter {
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
