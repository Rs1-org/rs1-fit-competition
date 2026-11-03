import { Component, computed, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { CountryForm } from '../../components/country-form/country-form';
import { CountryService } from '../../core/services/country-service';
import { CountryListDto, CountryListFilter } from '../../core/models/country';
import { ExportFormat, SortParams } from '../../core/models/import-export';
import { emptyPage } from '../../core/models/paged-result';
import { UserStateService } from '../../core/services/user-state.service';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, debounceTime, filter, map, of, startWith, switchMap } from 'rxjs';
import { ConfirmDialogService, deleteConfirm } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { Pagination } from '../../shared/components/pagination/pagination';
import { ImportExportBar } from '../../shared/components/import-export-bar/import-export-bar';
import { AppDatePipe } from '../../shared/pipes/regional-format.pipes';
import { ariaSort, nextSort, sortIndicator } from '../../core/helpers/sort-state.helper';
import { ToastService } from '../../core/services/toast.service';
import { apiErrorMessage } from '../../core/utils/api-error';
import { FilterCard, FilterChip } from '../../shared/components/filter-card/filter-card';
import { Skeleton } from '../../shared/components/skeleton/skeleton';
import { EmptyState } from '../../shared/components/empty-state/empty-state';
import { ErrorBanner } from '../../shared/components/error-banner/error-banner';
import { Button } from '../../shared/components/button/button';
import { Icon } from '../../shared/components/icon/icon';

/** Mirrors the backend rule: "Created to" must be on or after "Created from". */
function createdRangeValidator(group: AbstractControl): ValidationErrors | null {
  const { createdFrom, createdTo } = group.value;
  return createdFrom && createdTo && createdTo < createdFrom ? { createdRange: true } : null;
}

@Component({
  selector: 'app-country-list',
  standalone: true,
  imports: [
    CountryForm, ReactiveFormsModule, Pagination, ImportExportBar, AppDatePipe, FilterCard, Skeleton, EmptyState, ErrorBanner,
    Button, Icon,
  ],
  templateUrl: './country-list.html',
  styleUrl: './country-list.css',
})
export class CountryList {
  private countryService = inject(CountryService);
  private confirmDialog = inject(ConfirmDialogService);
  private toast = inject(ToastService);
  private userStateService = inject(UserStateService);
  private fb = inject(FormBuilder);
  private datePipe = new AppDatePipe();
  private refresh$ = new BehaviorSubject<void>(undefined);
  private query$ = new BehaviorSubject<{ page: number; filter: CountryListFilter }>({ page: 1, filter: {} });
  readonly pageSize = 10;

  filterForm = this.fb.nonNullable.group({
    name: ['', Validators.maxLength(100)],
    slug: ['', Validators.maxLength(100)],
    hasPicture: [''],
    createdFrom: [''],
    createdTo: [''],
  }, { validators: createdRangeValidator });

  state = toSignal(
    combineLatest([this.query$, this.refresh$]).pipe(
      switchMap(([{ page, filter }]) =>
        this.countryService.getAll(page, this.pageSize, filter).pipe(
          map((data) => ({ data, loading: false, error: '' })),
          startWith({ data: emptyPage<CountryListDto>(), loading: true, error: '' }),
          catchError((err) =>
            of({ data: emptyPage<CountryListDto>(), loading: false, error: apiErrorMessage(err, 'Countries could not be loaded.') }),
          ),
        ),
      ),
    ),
    { initialValue: { data: emptyPage<CountryListDto>(), loading: true, error: '' } },
  );

  // Mirrors the backend: Admin/Moderator can add and edit, only Admin can delete and import.
  canManage = this.userStateService.canManage;
  canDelete = this.userStateService.isAdmin;
  isFormOpen = signal(false);
  selectedCountry = signal<CountryListDto | null>(null);

  // Backend column sorting (RS1 item 13).
  sort = signal<SortParams>({});
  readonly ariaSort = ariaSort;
  readonly sortIndicator = sortIndicator;
  sortNote = computed(() => {
    const { sortBy, sortDir } = this.sort();
    const desc = sortDir === 'desc';
    if (sortBy === 'createdAt') return `Sorted by date added, ${desc ? 'newest' : 'oldest'} first`;
    return `Sorted by name, ${desc ? 'Z to A' : 'A to Z'}`;
  });

  // Export uses the filters + sort currently applied to the table (RS1 item 86).
  exportFn = (format: ExportFormat) => this.countryService.export(this.query$.value.filter, format);
  importFn = (file: File) => this.countryService.import(file);

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

  private toFilter(): CountryListFilter {
    const { name, slug, hasPicture, createdFrom, createdTo } = this.filterForm.getRawValue();
    return {
      name: name.trim() || undefined,
      slug: slug.trim() || undefined,
      hasPicture: hasPicture === '' ? undefined : hasPicture === 'true',
      createdFrom: createdFrom || undefined,
      createdTo: createdTo || undefined,
      ...this.sort(),
    };
  }

  chips(): FilterChip[] {
    const { name, slug, hasPicture, createdFrom, createdTo } = this.filterForm.getRawValue();
    const chips: FilterChip[] = [];
    if (name.trim()) chips.push({ key: 'name', label: `"${name.trim()}"` });
    if (slug.trim()) chips.push({ key: 'slug', label: `Slug "${slug.trim()}"` });
    if (hasPicture) chips.push({ key: 'hasPicture', label: hasPicture === 'true' ? 'With picture' : 'Without picture' });
    if (createdFrom) chips.push({ key: 'createdFrom', label: `Added from ${this.datePipe.transform(createdFrom)}` });
    if (createdTo) chips.push({ key: 'createdTo', label: `Added to ${this.datePipe.transform(createdTo)}` });
    return chips;
  }

  clearChip(key: string): void {
    this.filterForm.get(key)?.setValue('');
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
    this.selectedCountry.set(null);
    this.isFormOpen.set(true);
  }

  onEdit(country: CountryListDto): void {
    if (!this.canManage()) return;
    this.selectedCountry.set(country);
    this.isFormOpen.set(true);
  }

  async onDelete(country: CountryListDto): Promise<void> {
    if (!this.canDelete()) return;
    const confirmed = await this.confirmDialog.confirm(deleteConfirm(`${country.name}`));
    if (!confirmed) return;
    this.countryService.delete(country.publicId).subscribe({
      next: () => {
        this.toast.success('Country deleted');
        this.refresh$.next();
      },
      error: (err) => this.toast.error(apiErrorMessage(err, 'The country could not be deleted.')),
    });
  }

  onFormSaved(): void {
    this.isFormOpen.set(false);
    this.selectedCountry.set(null);
    this.refresh$.next();
  }

  onFormClosed(): void {
    this.isFormOpen.set(false);
    this.selectedCountry.set(null);
  }
}
