import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import {
  BehaviorSubject,
  catchError,
  combineLatest,
  debounceTime,
  filter,
  map,
  of,
  startWith,
  switchMap,
} from 'rxjs';
import { ClubService } from '../../core/services/club.service';
import { CountryService } from '../../core/services/country-service';
import { CityService } from '../../core/services/city-services';
import { CLUB_AGES, ClubListDto, ClubListFilter } from '../../core/models/club';
import { CountryListDto } from '../../core/models/country';
import { UserStateService } from '../../core/services/user-state.service';
import { PagedResult } from '../../core/models/paged-result';
import { fetchAllPages } from '../../core/helpers/paging.helper';
import { apiErrorMessage } from '../../core/utils/api-error';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { Pagination } from '../../shared/components/pagination/pagination';
import { AppDatePipe, AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { clubDateRangeValidator } from '../../components/club-form/club-validation';

@Component({
  selector: 'app-club-list',
  imports: [ReactiveFormsModule, RouterLink, Pagination, AppDatePipe, AppNumberPipe],
  templateUrl: './club-list.html',
  styleUrl: './club-pages.css',
})
export class ClubList {
  private readonly service = inject(ClubService);
  private readonly countryService = inject(CountryService);
  private readonly cityService = inject(CityService);
  private readonly confirm = inject(ConfirmDialogService);
  private readonly fb = inject(FormBuilder);
  readonly user = inject(UserStateService);
  readonly ages = CLUB_AGES;
  readonly pageSize = 10;
  readonly countries = signal<CountryListDto[]>([]);
  readonly optionsError = signal('');
  readonly actionError = signal('');
  readonly deleting = signal<string | null>(null);
  private readonly refresh$ = new BehaviorSubject<void>(undefined);
  private readonly optionsRefresh$ = new BehaviorSubject<void>(undefined);
  private readonly query$ = new BehaviorSubject<{ page: number; filter: ClubListFilter }>({
    page: 1,
    filter: {},
  });

  readonly filterForm = this.fb.nonNullable.group(
    {
      name: '',
      countryPublicId: '',
      cityPublicId: '',
      foundedFrom: '',
      foundedTo: '',
      hasStadium: '',
      clubAge: '',
      sort: 'name',
    },
    { validators: clubDateRangeValidator },
  );

  readonly cities = toSignal(
    combineLatest([
      this.filterForm.controls.countryPublicId.valueChanges.pipe(startWith('')),
      this.optionsRefresh$,
    ]).pipe(
      switchMap(([countryPublicId]) =>
        fetchAllPages((page) => this.cityService.getAll(page, 100, { countryPublicId })).pipe(
          catchError((err) => {
            this.optionsError.set(apiErrorMessage(err, 'City options could not be loaded.'));
            return of([]);
          }),
        ),
      ),
    ),
    { initialValue: [] },
  );

  readonly state = toSignal(
    combineLatest([this.query$, this.refresh$]).pipe(
      switchMap(([{ page, filter: filters }]) =>
        this.service.getAll(page, this.pageSize, filters).pipe(
          map((data) => ({ data, loading: false, error: '' })),
          startWith({ data: null as PagedResult<ClubListDto> | null, loading: true, error: '' }),
          catchError((err) =>
            of({
              data: null,
              loading: false,
              error: apiErrorMessage(err, 'Clubs could not be loaded.'),
            }),
          ),
        ),
      ),
    ),
    { initialValue: { data: null, loading: true, error: '' } },
  );

  constructor() {
    this.loadOptions();
    this.filterForm.controls.countryPublicId.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.filterForm.controls.cityPublicId.setValue('', { emitEvent: false }));
    this.filterForm.valueChanges
      .pipe(
        debounceTime(300),
        filter(() => this.filterForm.valid),
        takeUntilDestroyed(),
      )
      .subscribe(() => this.query$.next({ page: 1, filter: this.toFilter() }));
  }

  loadOptions(): void {
    this.optionsError.set('');
    this.optionsRefresh$.next();
    this.countryService.getAllForDropdown().subscribe({
      next: (data) => this.countries.set(data),
      error: (err) =>
        this.optionsError.set(apiErrorMessage(err, 'Country options could not be loaded.')),
    });
  }

  retry(): void {
    this.refresh$.next();
  }
  onPageChange(page: number): void {
    this.query$.next({ ...this.query$.value, page });
  }
  resetFilters(): void {
    this.filterForm.reset();
  }

  async onDelete(club: ClubListDto): Promise<void> {
    if (!this.user.isAdmin() || this.deleting()) return;
    if (
      !(await this.confirm.confirm({
        title: 'Delete club',
        message: `Delete ${club.name}?`,
        confirmLabel: 'Delete club',
      }))
    )
      return;
    this.deleting.set(club.publicId);
    this.actionError.set('');
    this.service.delete(club.publicId).subscribe({
      next: () => {
        this.deleting.set(null);
        this.query$.next({ ...this.query$.value, page: 1 });
      },
      error: (err) => {
        this.deleting.set(null);
        this.actionError.set(apiErrorMessage(err));
      },
    });
  }

  private toFilter(): ClubListFilter {
    const v = this.filterForm.getRawValue();
    return {
      name: v.name.trim() || undefined,
      countryPublicId: v.countryPublicId || undefined,
      cityPublicId: this.filterForm.controls.cityPublicId.value || undefined,
      foundedFrom: v.foundedFrom || undefined,
      foundedTo: v.foundedTo || undefined,
      hasStadium: v.hasStadium === '' ? undefined : v.hasStadium === 'true',
      clubAge: v.clubAge === '' ? undefined : Number(v.clubAge),
      sortBy: v.sort === 'founded' ? 'foundingDate' : v.sort === 'age' ? 'clubAge' : 'name',
      descending: v.sort === 'founded',
    };
  }
}
