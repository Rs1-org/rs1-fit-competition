import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { CountryForm } from '../../components/country-form/country-form';
import { CountryService } from '../../core/services/country-service';
import { CountryListDto, CountryListFilter } from '../../core/models/country';
import { UserStateService } from '../../core/services/user-state.service';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, combineLatest, debounceTime, filter, switchMap } from 'rxjs';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';

/** Mirrors the backend rule: "Created to" must be on or after "Created from". */
function createdRangeValidator(group: AbstractControl): ValidationErrors | null {
  const { createdFrom, createdTo } = group.value;
  return createdFrom && createdTo && createdTo < createdFrom ? { createdRange: true } : null;
}

@Component({
  selector: 'app-country-list',
  standalone: true,
  imports: [CountryForm, GenericListComponent, CommonModule, ReactiveFormsModule],
  templateUrl: './country-list.html',
  styleUrl: './country-list.css',
})
export class CountryList {
  private countryService = inject(CountryService);
  private confirmDialog = inject(ConfirmDialogService);
  private userStateService = inject(UserStateService);
  private fb = inject(FormBuilder);
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

  countriesResult = toSignal(
    combineLatest([this.query$, this.refresh$]).pipe(
      switchMap(([{ page, filter }]) => this.countryService.getAll(page, this.pageSize, filter))
    )
  );

  user = this.userStateService.getUser();
  isFormOpen = false;
  selectedCountry: CountryListDto | null = null;

  constructor() {
    // Any filter change goes back to page 1; invalid combinations are not sent.
    this.filterForm.valueChanges.pipe(
      debounceTime(300),
      filter(() => this.filterForm.valid),
      takeUntilDestroyed(),
    ).subscribe(() => this.query$.next({ page: 1, filter: this.toFilter() }));
  }

  private toFilter(): CountryListFilter {
    const { name, slug, hasPicture, createdFrom, createdTo } = this.filterForm.getRawValue();
    return {
      name: name.trim() || undefined,
      slug: slug.trim() || undefined,
      hasPicture: hasPicture === '' ? undefined : hasPicture === 'true',
      createdFrom: createdFrom || undefined,
      createdTo: createdTo || undefined,
    };
  }

  resetFilters(): void {
    this.filterForm.reset();
  }

  onPageChange(page: number) {
    this.query$.next({ ...this.query$.value, page });
  }

  onAdd(): void {
    this.selectedCountry = null;
    this.isFormOpen = true;
  }

  onEdit(country: CountryListDto): void {
    this.selectedCountry = country;
    this.isFormOpen = true;
  }

  async onDelete(country: CountryListDto): Promise<void> {
    const confirmed = await this.confirmDialog.confirm({
      message: `Are you sure you want to delete ${country.name}?`
    });
    if (!confirmed) return;
    this.countryService.delete(country.id).subscribe({
      next: () => this.refresh$.next(),
      error: (err) => console.error(err)
    });
  }

  onFormSaved(): void {
    this.isFormOpen = false;
    this.selectedCountry = null;
    this.refresh$.next();
  }

  onFormClosed(): void {
    this.isFormOpen = false;
    this.selectedCountry = null;
  }
}
