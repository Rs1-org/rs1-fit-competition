import { Component, computed, inject, signal } from '@angular/core';
import { AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { CityForm } from '../../components/city-form/city-form';
import { CityService } from '../../core/services/city-services';
import { CountryService } from '../../core/services/country-service';
import { CityDetailsDto, CityListDto, CityListFilter } from '../../core/models/city';
import { emptyPage } from '../../core/models/paged-result';
import { UserStateService } from '../../core/services/user-state.service';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, debounceTime, filter, map, of, startWith, switchMap } from 'rxjs';
import { Pagination } from '../../shared/components/pagination/pagination';
import { ImportExportBar } from '../../shared/components/import-export-bar/import-export-bar';
import { ExportFormat, SortParams } from '../../core/models/import-export';
import { ariaSort, nextSort, sortIndicator } from '../../core/helpers/sort-state.helper';
import { ConfirmDialogService, deleteConfirm } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { ToastService } from '../../core/services/toast.service';
import { apiErrorMessage } from '../../core/utils/api-error';
import { FilterCard, FilterChip } from '../../shared/components/filter-card/filter-card';
import { Skeleton } from '../../shared/components/skeleton/skeleton';
import { EmptyState } from '../../shared/components/empty-state/empty-state';
import { ErrorBanner } from '../../shared/components/error-banner/error-banner';
import { Button } from '../../shared/components/button/button';
import { Icon } from '../../shared/components/icon/icon';

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
  imports: [
    CityForm, AppNumberPipe, ReactiveFormsModule, Pagination, ImportExportBar, FilterCard, Skeleton, EmptyState, ErrorBanner,
    Button, Icon,
  ],
  templateUrl: './city-list.html',
  styleUrl: './city-list.css',
})
export class CityList {
  private cityService = inject(CityService);
  private countryService = inject(CountryService);
  private confirmDialog = inject(ConfirmDialogService);
  private toast = inject(ToastService);
  private userStateService = inject(UserStateService);
  private fb = inject(FormBuilder);
  private numberPipe = new AppNumberPipe();
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

  state = toSignal(
    combineLatest([this.query$, this.refresh$]).pipe(
      switchMap(([{ page, filter }]) =>
        this.cityService.getAll(page, this.pageSize, filter).pipe(
          map((data) => ({ data, loading: false, error: '' })),
          startWith({ data: emptyPage<CityListDto>(), loading: true, error: '' }),
          catchError((err) =>
            of({ data: emptyPage<CityListDto>(), loading: false, error: apiErrorMessage(err, 'Cities could not be loaded.') }),
          ),
        ),
      ),
    ),
    { initialValue: { data: emptyPage<CityListDto>(), loading: true, error: '' } },
  );

  // Mirrors the backend: Admin/Moderator can add and edit, only Admin can delete and import.
  canManage = this.userStateService.canManage;
  canDelete = this.userStateService.isAdmin;
  isFormOpen = signal(false);
  selectedCity = signal<CityDetailsDto | null>(null);

  // Backend column sorting (RS1 item 13).
  sort = signal<SortParams>({});
  readonly ariaSort = ariaSort;
  readonly sortIndicator = sortIndicator;
  sortNote = computed(() => {
    const { sortBy, sortDir } = this.sort();
    const desc = sortDir === 'desc';
    if (sortBy === 'population') return `Sorted by population, ${desc ? 'largest' : 'smallest'} first`;
    if (sortBy === 'country') return `Sorted by country, ${desc ? 'Z to A' : 'A to Z'}`;
    return `Sorted by name, ${desc ? 'Z to A' : 'A to Z'}`;
  });

  // Export uses the filters + sort currently applied to the table (RS1 item 86).
  exportFn = (format: ExportFormat) => this.cityService.export(this.query$.value.filter, format);
  importFn = (file: File) => this.cityService.import(file);

  constructor() {
    // Any filter change goes back to page 1; invalid combinations are not sent.
    this.filterForm.valueChanges.pipe(
      debounceTime(300),
      filter(() => this.filterForm.valid),
      takeUntilDestroyed(),
    ).subscribe(() => this.search());
  }

  private search(): void {
    this.query$.next({ page: 1, filter: this.toFilter() });
  }

  private toFilter(): CityListFilter {
    const { name, countryPublicId, isCapital, minPopulation, maxPopulation } = this.filterForm.getRawValue();
    return {
      name: name.trim() || undefined,
      countryPublicId: countryPublicId || undefined,
      isCapital: isCapital === '' ? undefined : isCapital === 'true',
      minPopulation: minPopulation ?? undefined,
      maxPopulation: maxPopulation ?? undefined,
      ...this.sort(),
    };
  }

  chips(): FilterChip[] {
    const { name, countryPublicId, isCapital, minPopulation, maxPopulation } = this.filterForm.getRawValue();
    const chips: FilterChip[] = [];
    if (name.trim()) chips.push({ key: 'name', label: `"${name.trim()}"` });
    const country = this.countries().find((item) => item.publicId === countryPublicId)?.name;
    if (country) chips.push({ key: 'countryPublicId', label: country });
    if (isCapital) chips.push({ key: 'isCapital', label: isCapital === 'true' ? 'Capital only' : 'Not capital' });
    if (minPopulation != null) chips.push({ key: 'minPopulation', label: `Population from ${this.numberPipe.transform(minPopulation, '1.0-0')}` });
    if (maxPopulation != null) chips.push({ key: 'maxPopulation', label: `Population to ${this.numberPipe.transform(maxPopulation, '1.0-0')}` });
    return chips;
  }

  clearChip(key: string): void {
    this.filterForm.get(key)?.setValue(key === 'minPopulation' || key === 'maxPopulation' ? null : '');
  }

  apply(): void {
    if (this.filterForm.valid) this.search();
  }

  sortBy(key: string): void {
    this.sort.update((current) => nextSort(current, key));
    this.query$.next({ page: 1, filter: { ...this.query$.value.filter, ...this.sort() } });
  }

  retry(): void {
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
    this.selectedCity.set(null);
    this.isFormOpen.set(true);
  }

  onEdit(city: CityListDto): void {
    if (!this.canManage()) return;
    this.cityService.getBySlug(city.slug).subscribe({
      next: (details) => {
        this.selectedCity.set(details);
        this.isFormOpen.set(true);
      },
      error: (err) => this.toast.error(apiErrorMessage(err, 'The city could not be loaded.')),
    });
  }

  async onDelete(city: CityListDto): Promise<void> {
    if (!this.canDelete()) return;
    const confirmed = await this.confirmDialog.confirm(deleteConfirm(`${city.name}`));
    if (!confirmed) return;
    this.cityService.delete(city.publicId).subscribe({
      next: () => {
        this.toast.success('City deleted');
        this.refresh$.next();
      },
      error: (err) => this.toast.error(apiErrorMessage(err, 'The city could not be deleted.')),
    });
  }

  onFormSaved(): void {
    this.isFormOpen.set(false);
    this.selectedCity.set(null);
    this.refresh$.next();
  }

  onFormClosed(): void {
    this.isFormOpen.set(false);
    this.selectedCity.set(null);
  }
}
