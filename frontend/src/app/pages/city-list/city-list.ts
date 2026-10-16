import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { CityForm } from '../../components/city-form/city-form';
import { CityService } from '../../core/services/city-services';
import { CountryService } from '../../core/services/country-service';
import { CityDetailsDto, CityListDto, CityListFilter } from '../../core/models/city';
import { UserStateService } from '../../core/services/user-state.service';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, combineLatest, debounceTime, filter, switchMap } from 'rxjs';
import { Pagination } from '../../shared/components/pagination/pagination';
import { ImportExportBar } from '../../shared/components/import-export-bar/import-export-bar';
import { ExportFormat, SortParams } from '../../core/models/import-export';
import { ariaSort, nextSort, sortIndicator } from '../../core/helpers/sort-state.helper';
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
  imports: [CityForm, AppNumberPipe, CommonModule, ReactiveFormsModule, Pagination, ImportExportBar],
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
    countryPublicId: this.fb.nonNullable.control(''),
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

  // Mirrors the backend: Admin/Moderator can add and edit, only Admin can delete and import.
  canManage = this.userStateService.canManage;
  canDelete = this.userStateService.isAdmin;
  isFormOpen = false;
  selectedCity: CityDetailsDto | null = null;

  // Backend column sorting (RS1 item 13).
  sort: SortParams = {};
  readonly ariaSort = ariaSort;
  readonly sortIndicator = sortIndicator;

  // Export uses the filters + sort currently applied to the table (RS1 item 86).
  exportFn = (format: ExportFormat) => this.cityService.export(this.query$.value.filter, format);
  importFn = (file: File) => this.cityService.import(file);

  constructor() {
    // Any filter change goes back to page 1; invalid combinations are not sent.
    this.filterForm.valueChanges.pipe(
      debounceTime(300),
      filter(() => this.filterForm.valid),
      takeUntilDestroyed(),
    ).subscribe(() => this.query$.next({ page: 1, filter: this.toFilter() }));
  }

  private toFilter(): CityListFilter {
    const { name, countryPublicId, isCapital, minPopulation, maxPopulation } = this.filterForm.getRawValue();
    return {
      name: name.trim() || undefined,
      countryPublicId: countryPublicId || undefined,
      isCapital: isCapital === '' ? undefined : isCapital === 'true',
      minPopulation: minPopulation ?? undefined,
      maxPopulation: maxPopulation ?? undefined,
      ...this.sort,
    };
  }

  sortBy(key: string): void {
    this.sort = nextSort(this.sort, key);
    this.query$.next({ page: 1, filter: { ...this.query$.value.filter, ...this.sort } });
  }

  onImported(): void {
    this.refresh$.next();
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
    this.cityService.delete(city.publicId).subscribe({
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
