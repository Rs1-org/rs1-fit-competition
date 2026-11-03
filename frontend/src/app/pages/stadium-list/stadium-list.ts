import { Component, computed, inject, signal, ViewChild } from '@angular/core';
import { AppDatePipe, AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Pagination } from '../../shared/components/pagination/pagination';
import { ImportExportBar } from '../../shared/components/import-export-bar/import-export-bar';
import { ExportFormat, SortParams } from '../../core/models/import-export';
import { ariaSort, nextSort, sortIndicator } from '../../core/helpers/sort-state.helper';
import { Router } from '@angular/router';
import { StadiumService } from '../../core/services/stadium.service';
import { CountryService } from '../../core/services/country-service';
import { CityService } from '../../core/services/city-services';
import { StadiumListDto, StadiumListFilter } from '../../core/models/stadium';
import { emptyPage } from '../../core/models/paged-result';
import { StadiumForm } from '../../components/stadium-form/stadium-form';
import { ConfirmDialogService, deleteConfirm } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { UserStateService } from '../../core/services/user-state.service';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, debounceTime, distinctUntilChanged, filter, map, of, startWith, switchMap } from 'rxjs';
import { fetchAllPages, MAX_PAGE_SIZE } from '../../core/helpers/paging.helper';
import { ToastService } from '../../core/services/toast.service';
import { apiErrorMessage } from '../../core/utils/api-error';
import { FilterCard, FilterChip } from '../../shared/components/filter-card/filter-card';
import { Skeleton } from '../../shared/components/skeleton/skeleton';
import { EmptyState } from '../../shared/components/empty-state/empty-state';
import { ErrorBanner } from '../../shared/components/error-banner/error-banner';
import { Button } from '../../shared/components/button/button';
import { Icon } from '../../shared/components/icon/icon';

/** Mirrors the backend rule: max capacity must be >= min capacity. */
function capacityRangeValidator(group: AbstractControl): ValidationErrors | null {
  const { minCapacity, maxCapacity } = group.value;
  return minCapacity != null && maxCapacity != null && maxCapacity < minCapacity
    ? { capacityRange: true }
    : null;
}

const SORT_LABELS: Record<string, string> = { name: 'name', city: 'city', capacity: 'capacity', dateOfConstruction: 'year built' };

@Component({
  selector: 'app-stadium-list',
  standalone: true,
  imports: [
    StadiumForm, AppNumberPipe, AppDatePipe, ReactiveFormsModule, Pagination, ImportExportBar, FilterCard, Skeleton, EmptyState,
    ErrorBanner, Button, Icon,
  ],
  templateUrl: './stadium-list.html',
  styleUrl: './stadium-list.css',
})
export class StadiumList {
  @ViewChild(StadiumForm) stadiumFormRef!: StadiumForm;

  private stadiumService   = inject(StadiumService);
  private countryService   = inject(CountryService);
  private cityService      = inject(CityService);
  private confirmDialog    = inject(ConfirmDialogService);
  private toast = inject(ToastService);
  private router           = inject(Router);
  private userStateService = inject(UserStateService);
  private fb               = inject(FormBuilder);
  private numberPipe       = new AppNumberPipe();

  private refresh$ = new BehaviorSubject<void>(undefined);
  private query$ = new BehaviorSubject<{ page: number; filter: StadiumListFilter }>({ page: 1, filter: {} });
  readonly pageSize = 10;

  filterForm = this.fb.group({
    name: this.fb.nonNullable.control('', Validators.maxLength(150)),
    countryPublicId: this.fb.nonNullable.control(''),
    cityPublicId: this.fb.nonNullable.control(''),
    minCapacity: this.fb.control<number | null>(null, Validators.min(0)),
    maxCapacity: this.fb.control<number | null>(null, Validators.min(0)),
  }, { validators: capacityRangeValidator });

  countries = toSignal(this.countryService.getAllForDropdown(), { initialValue: [] });
  private countryPublicId$ = this.filterForm.controls.countryPublicId.valueChanges.pipe(
    startWith(this.filterForm.controls.countryPublicId.value),
    distinctUntilChanged(),
  );

  // City options follow the selected country (all cities when no country is picked).
  cities = toSignal(
    this.countryPublicId$.pipe(
      switchMap((countryPublicId) => fetchAllPages((page) =>
        this.cityService.getAll(page, MAX_PAGE_SIZE, { countryPublicId: countryPublicId || undefined })))
    ),
    { initialValue: [] }
  );

  // Mirrors the backend: Admin/Moderator can add and edit, only Admin can delete and import.
  canManage = this.userStateService.canManage;
  canDelete = this.userStateService.isAdmin;

  // Backend column sorting (RS1 item 13).
  sort = signal<SortParams>({});
  readonly ariaSort = ariaSort;
  readonly sortIndicator = sortIndicator;
  sortNote = computed(() => {
    const { sortBy, sortDir } = this.sort();
    if (!sortBy) return 'Sorted by name, A to Z';
    const desc = sortDir === 'desc';
    const direction = sortBy === 'capacity' ? (desc ? 'largest first' : 'smallest first')
      : sortBy === 'dateOfConstruction' ? (desc ? 'newest first' : 'oldest first')
      : (desc ? 'Z to A' : 'A to Z');
    return `Sorted by ${SORT_LABELS[sortBy] ?? sortBy}, ${direction}`;
  });

  // Export uses the filters + sort currently applied to the table (RS1 item 86).
  exportFn = (format: ExportFormat) => this.stadiumService.export(this.query$.value.filter, format);
  importFn = (file: File) => this.stadiumService.import(file);

  state = toSignal(
    combineLatest([this.query$, this.refresh$]).pipe(
      switchMap(([{ page, filter }]) =>
        this.stadiumService.getAll(page, this.pageSize, filter).pipe(
          map((data) => ({ data, loading: false, error: '' })),
          startWith({ data: emptyPage<StadiumListDto>(), loading: true, error: '' }),
          catchError((err) =>
            of({ data: emptyPage<StadiumListDto>(), loading: false, error: apiErrorMessage(err, 'Stadiums could not be loaded.') }),
          ),
        ),
      ),
    ),
    { initialValue: { data: emptyPage<StadiumListDto>(), loading: true, error: '' } },
  );

  constructor() {
    // Any filter change goes back to page 1; invalid combinations are not sent.
    this.filterForm.valueChanges.pipe(
      debounceTime(300),
      filter(() => this.filterForm.valid),
      takeUntilDestroyed(),
    ).subscribe(() => this.search());

    // A city picked for the previous country would no longer match, so clear it.
    this.filterForm.controls.countryPublicId.valueChanges.pipe(takeUntilDestroyed())
      .subscribe(() => this.filterForm.controls.cityPublicId.setValue(''));
  }

  private search(): void {
    this.query$.next({ page: 1, filter: this.toFilter() });
  }

  private toFilter(): StadiumListFilter {
    const { name, countryPublicId, cityPublicId, minCapacity, maxCapacity } = this.filterForm.getRawValue();
    return {
      name: name.trim() || undefined,
      countryPublicId: countryPublicId || undefined,
      cityPublicId: cityPublicId || undefined,
      minCapacity: minCapacity ?? undefined,
      maxCapacity: maxCapacity ?? undefined,
      ...this.sort(),
    };
  }

  chips(): FilterChip[] {
    const { name, countryPublicId, cityPublicId, minCapacity, maxCapacity } = this.filterForm.getRawValue();
    const chips: FilterChip[] = [];
    if (name.trim()) chips.push({ key: 'name', label: `"${name.trim()}"` });
    const country = this.countries().find((item) => item.publicId === countryPublicId)?.name;
    if (country) chips.push({ key: 'countryPublicId', label: country });
    const city = this.cities().find((item) => item.publicId === cityPublicId)?.name;
    if (city) chips.push({ key: 'cityPublicId', label: city });
    if (minCapacity != null) chips.push({ key: 'minCapacity', label: `At least ${this.numberPipe.transform(minCapacity, '1.0-0')} seats` });
    if (maxCapacity != null) chips.push({ key: 'maxCapacity', label: `At most ${this.numberPipe.transform(maxCapacity, '1.0-0')} seats` });
    return chips;
  }

  clearChip(key: string): void {
    const control = this.filterForm.get(key);
    control?.setValue(key === 'minCapacity' || key === 'maxCapacity' ? null : '');
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

  onFormSaved(): void {
    this.refresh$.next();
  }

  view(stadium: StadiumListDto) {
    this.router.navigate(['/stadium', stadium.slug]);
  }

  edit(stadium: StadiumListDto) {
    if (!this.canManage()) return;
    this.stadiumService.getBySlug(stadium.slug).subscribe({
      next: (data) => this.stadiumFormRef.openForEdit(data),
      error: (err) => this.toast.error(apiErrorMessage(err, 'The stadium could not be loaded.')),
    });
  }

  async delete(stadium: StadiumListDto) {
    if (!this.canDelete()) return;
    const confirmed = await this.confirmDialog.confirm(deleteConfirm(`${stadium.name}`));
    if (!confirmed) return;

    this.stadiumService.delete(stadium.publicId).subscribe({
      next: () => {
        this.toast.success('Stadium deleted');
        this.refresh$.next();
      },
      error: (err) => this.toast.error(apiErrorMessage(err, 'The stadium could not be deleted.')),
    });
  }
}
