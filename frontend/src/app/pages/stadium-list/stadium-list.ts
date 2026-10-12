import { Component, inject, ViewChild } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { Router } from '@angular/router';
import { StadiumService } from '../../core/services/stadium.service';
import { CountryService } from '../../core/services/country-service';
import { CityService } from '../../core/services/city-services';
import { StadiumListDto, StadiumListFilter } from '../../core/models/stadium';
import { StadiumForm } from '../../components/stadium-form/stadium-form';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { UserStateService } from '../../core/services/user-state.service';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, combineLatest, debounceTime, distinctUntilChanged, filter, startWith, switchMap } from 'rxjs';
import { fetchAllPages, MAX_PAGE_SIZE } from '../../core/helpers/paging.helper';

/** Mirrors the backend rule: max capacity must be >= min capacity. */
function capacityRangeValidator(group: AbstractControl): ValidationErrors | null {
  const { minCapacity, maxCapacity } = group.value;
  return minCapacity != null && maxCapacity != null && maxCapacity < minCapacity
    ? { capacityRange: true }
    : null;
}

@Component({
  selector: 'app-stadium-list',
  standalone: true,
  imports: [GenericListComponent, CommonModule, StadiumForm, DecimalPipe, ReactiveFormsModule],
  templateUrl: './stadium-list.html',
  styleUrl: './stadium-list.css',
})
export class StadiumList {
  @ViewChild(StadiumForm) stadiumFormRef!: StadiumForm;

  private stadiumService   = inject(StadiumService);
  private countryService   = inject(CountryService);
  private cityService      = inject(CityService);
  private confirmDialog    = inject(ConfirmDialogService);
  private router           = inject(Router);
  private userStateService = inject(UserStateService);
  private fb               = inject(FormBuilder);

  private refresh$ = new BehaviorSubject<void>(undefined);
  private query$ = new BehaviorSubject<{ page: number; filter: StadiumListFilter }>({ page: 1, filter: {} });
  readonly pageSize = 10;

  filterForm = this.fb.group({
    name: this.fb.nonNullable.control('', Validators.maxLength(150)),
    countryId: this.fb.nonNullable.control(''),
    cityId: this.fb.nonNullable.control(''),
    minCapacity: this.fb.control<number | null>(null, Validators.min(0)),
    maxCapacity: this.fb.control<number | null>(null, Validators.min(0)),
  }, { validators: capacityRangeValidator });

  countries = toSignal(this.countryService.getAllForDropdown(), { initialValue: [] });
  private countryId$ = this.filterForm.controls.countryId.valueChanges.pipe(
    startWith(this.filterForm.controls.countryId.value),
    distinctUntilChanged(),
  );

  // City options follow the selected country (all cities when no country is picked).
  cities = toSignal(
    this.countryId$.pipe(
      switchMap((countryId) => fetchAllPages((page) =>
        this.cityService.getAll(page, MAX_PAGE_SIZE, { countryId: countryId || undefined })))
    ),
    { initialValue: [] }
  );

  user = this.userStateService.getUser();

  pagedResult = toSignal(
    combineLatest([this.query$, this.refresh$]).pipe(
      switchMap(([{ page, filter }]) => this.stadiumService.getAll(page, this.pageSize, filter))
    )
  );

  constructor() {
    // Any filter change goes back to page 1; invalid combinations are not sent.
    this.filterForm.valueChanges.pipe(
      debounceTime(300),
      filter(() => this.filterForm.valid),
      takeUntilDestroyed(),
    ).subscribe(() => this.query$.next({ page: 1, filter: this.toFilter() }));

    // A city picked for the previous country would no longer match, so clear it.
    this.filterForm.controls.countryId.valueChanges.pipe(takeUntilDestroyed())
      .subscribe(() => this.filterForm.controls.cityId.setValue(''));
  }

  private toFilter(): StadiumListFilter {
    const { name, countryId, cityId, minCapacity, maxCapacity } = this.filterForm.getRawValue();
    return {
      name: name.trim() || undefined,
      countryId: countryId || undefined,
      cityId: cityId || undefined,
      minCapacity: minCapacity ?? undefined,
      maxCapacity: maxCapacity ?? undefined,
    };
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
    this.stadiumService.getBySlug(stadium.slug).subscribe({
      next: (data) => this.stadiumFormRef.openForEdit(data),
      error: (err) => console.error('Error loading stadium details: ', err)
    });
  }

  async delete(stadium: StadiumListDto) {
    const confirmed = await this.confirmDialog.confirm({
      message: `Are you sure you want to delete ${stadium.name}?`
    });
    if (!confirmed) return;

    this.stadiumService.delete(stadium.id).subscribe({
      next: () => this.refresh$.next(),
      error: (err) => console.error('Error deleting stadium: ', err)
    });
  }
}
