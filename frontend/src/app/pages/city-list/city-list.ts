import { Component, inject } from '@angular/core';
import { DecimalPipe, CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { CityForm } from '../../components/city-form/city-form';
import { CityService } from '../../core/services/city-services';
import { CountryService } from '../../core/services/country-service';
import { CityDetailsDto, CityListDto, CityListFilter } from '../../core/models/city';
import { UserStateService } from '../../core/services/user-state.service';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, combineLatest, debounceTime, filter, switchMap } from 'rxjs';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';

/** Mirrors the backend rule: max population must be >= min population. */
function populationRangeValidator(group: AbstractControl): ValidationErrors | null {
  const { minPopulation, maxPopulation } = group.value;
  return minPopulation != null && maxPopulation != null && maxPopulation < minPopulation
    ? { populationRange: true }
    : null;
}

@Component({
  selector: 'app-city-list',
  standalone: true,
  imports: [CityForm, DecimalPipe, GenericListComponent, CommonModule, ReactiveFormsModule],
  templateUrl: './city-list.html',
  styleUrl: './city-list.css',
})
export class CityList {
  private cityService = inject(CityService);
  private countryService = inject(CountryService);
  private confirmDialog = inject(ConfirmDialogService);
  private userStateService = inject(UserStateService);
  private fb = inject(FormBuilder);
  private refresh$ = new BehaviorSubject<void>(undefined);
  private query$ = new BehaviorSubject<{ page: number; filter: CityListFilter }>({ page: 1, filter: {} });
  readonly pageSize = 10;

  filterForm = this.fb.group({
    name: this.fb.nonNullable.control('', Validators.maxLength(100)),
    countryId: this.fb.nonNullable.control(''),
    isCapital: this.fb.nonNullable.control(''),
    minPopulation: this.fb.control<number | null>(null, Validators.min(0)),
    maxPopulation: this.fb.control<number | null>(null, Validators.min(0)),
  }, { validators: populationRangeValidator });

  countries = toSignal(this.countryService.getAllForDropdown(), { initialValue: [] });

  citiesResult = toSignal(
    combineLatest([this.query$, this.refresh$]).pipe(
      switchMap(([{ page, filter }]) => this.cityService.getAll(page, this.pageSize, filter))
    )
  );

  // Mirrors the backend: Admin/Moderator can add and edit, only Admin can delete.
  canManage = this.userStateService.canManage;
  canDelete = this.userStateService.isAdmin;
  isFormOpen = false;
  selectedCity: CityDetailsDto | null = null;

  constructor() {
    // Any filter change goes back to page 1; invalid combinations are not sent.
    this.filterForm.valueChanges.pipe(
      debounceTime(300),
      filter(() => this.filterForm.valid),
      takeUntilDestroyed(),
    ).subscribe(() => this.query$.next({ page: 1, filter: this.toFilter() }));
  }

  private toFilter(): CityListFilter {
    const { name, countryId, isCapital, minPopulation, maxPopulation } = this.filterForm.getRawValue();
    return {
      name: name.trim() || undefined,
      countryId: countryId || undefined,
      isCapital: isCapital === '' ? undefined : isCapital === 'true',
      minPopulation: minPopulation ?? undefined,
      maxPopulation: maxPopulation ?? undefined,
    };
  }

  resetFilters(): void {
    this.filterForm.reset();
  }

  onPageChange(page: number) {
    this.query$.next({ ...this.query$.value, page });
  }

  onAdd(): void {
    if (!this.canManage()) return;
    this.selectedCity = null;
    this.isFormOpen = true;
  }

  onEdit(city: CityListDto): void {
    if (!this.canManage()) return;
    this.cityService.getBySlug(city.slug).subscribe({
      next: (details) => {
        this.selectedCity = details;
        this.isFormOpen = true;
      },
      error: (err) => console.error(err)
    });
  }

  async onDelete(city: CityListDto): Promise<void> {
    if (!this.canDelete()) return;
    const confirmed = await this.confirmDialog.confirm({
      message: `Are you sure you want to delete ${city.name}?`
    });
    if (!confirmed) return;
    this.cityService.delete(city.id).subscribe({
      next: () => this.refresh$.next(),
      error: (err) => console.error(err)
    });
  }

  onFormSaved(): void {
    this.isFormOpen = false;
    this.selectedCity = null;
    this.refresh$.next();
  }

  onFormClosed(): void {
    this.isFormOpen = false;
    this.selectedCity = null;
  }
}
