import { Component, inject } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BehaviorSubject, combineLatest, debounceTime, filter, map, switchMap } from 'rxjs';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { UserStateService } from '../../core/services/user-state.service';
import { SelectionListFilter, SelectionOption, SelectionService } from '../../core/services/selection.service';
import { CountryService } from '../../core/services/country-service';
import { CountryListDto } from '../../core/models/country';
import { emptyPage } from '../../core/models/paged-result';
import { AppNumberPipe } from '../../shared/pipes/regional-format.pipes';

function followersRangeValidator(group: AbstractControl): ValidationErrors | null {
  const { followersMin, followersMax } = group.value;
  return followersMin != null && followersMax != null && followersMax < followersMin
    ? { followersRange: true }
    : null;
}

@Component({
  selector: 'app-selection-list',
  imports: [CommonModule, GenericListComponent, ReactiveFormsModule, AppNumberPipe],
  templateUrl: './selection-list.html',
  styleUrl: '../catalog.css',
})
export class SelectionList {
  private service = inject(SelectionService);
  private countries = inject(CountryService);
  private confirmDialog = inject(ConfirmDialogService);
  private userState = inject(UserStateService);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  private refresh$ = new BehaviorSubject<void>(undefined);
  private query$ = new BehaviorSubject<{ page: number; filter: SelectionListFilter }>({ page: 1, filter: {} });
  readonly pageSize = 10;

  filterForm = this.fb.group({
    name: this.fb.nonNullable.control('', Validators.maxLength(120)),
    countryPublicId: this.fb.nonNullable.control(''),
    abbreviation: this.fb.nonNullable.control('', Validators.maxLength(10)),
    followersMin: this.fb.control<number | null>(null, Validators.min(0)),
    followersMax: this.fb.control<number | null>(null, Validators.min(0)),
  }, { validators: followersRangeValidator });

  countryOptions = toSignal(this.countries.getAll(1, 100), { initialValue: emptyPage<CountryListDto>() });

  result = toSignal(
    combineLatest([this.query$, this.refresh$]).pipe(
      switchMap(([{ page, filter }]) => this.service.getAll(page, this.pageSize, filter)),
    ),
    { initialValue: emptyPage<SelectionOption>() },
  );

  canManage = toSignal(
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

  onPageChange(page: number): void {
    this.query$.next({ ...this.query$.value, page });
  }

  resetFilters(): void {
    this.filterForm.reset({ name: '', countryPublicId: '', abbreviation: '' });
  }

  onAdd(): void { this.router.navigate(['/manage-selections']); }
  onEdit(item: SelectionOption): void { this.router.navigate(['/manage-selections', item.slug]); }

  async onDelete(item: SelectionOption): Promise<void> {
    const confirmed = await this.confirmDialog.confirm({ message: `Delete selection ${item.name}?` });
    if (!confirmed) return;
    this.service.delete(item.id).subscribe({ next: () => this.refresh$.next() });
  }

  private toFilter(): SelectionListFilter {
    const { name, countryPublicId, abbreviation, followersMin, followersMax } = this.filterForm.getRawValue();
    return {
      name: name.trim() || undefined,
      countryPublicId: countryPublicId || undefined,
      abbreviation: abbreviation.trim() || undefined,
      followersMin: followersMin ?? undefined,
      followersMax: followersMax ?? undefined,
    };
  }
}
