import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, debounceTime, filter, map, of, startWith, switchMap } from 'rxjs';
import { CompetitionService } from '../../core/services/competition.service';
import { CountryService } from '../../core/services/country-service';
import { CompetitionDetailsDto, CompetitionListDto } from '../../core/models/competition';
import { emptyPage } from '../../core/models/paged-result';
import { UserStateService } from '../../core/services/user-state.service';
import { ConfirmDialogService, deleteConfirm } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { CompetitionForm } from '../../components/competition-form/competition-form';
import { apiErrorMessage } from '../../core/utils/api-error';
import { ToastService } from '../../core/services/toast.service';
import { FilterCard, FilterChip } from '../../shared/components/filter-card/filter-card';
import { Pagination } from '../../shared/components/pagination/pagination';
import { Skeleton } from '../../shared/components/skeleton/skeleton';
import { EmptyState } from '../../shared/components/empty-state/empty-state';
import { ErrorBanner } from '../../shared/components/error-banner/error-banner';
import { Button } from '../../shared/components/button/button';
import { Icon } from '../../shared/components/icon/icon';
import { Crest } from '../../shared/components/crest/crest';
import { competitionCode } from '../../shared/match/match-display';

interface Query { page: number; search?: string; countryPublicId?: string; }

@Component({
  selector: 'app-competition-list',
  standalone: true,
  imports: [
    ReactiveFormsModule, RouterLink, CompetitionForm, FilterCard, Pagination, Skeleton, EmptyState, ErrorBanner, Button, Icon, Crest,
  ],
  templateUrl: './competition-list.html',
})
export class CompetitionList {
  readonly code = competitionCode;
  private competitionService = inject(CompetitionService);
  private countryService = inject(CountryService);
  private confirmDialog = inject(ConfirmDialogService);
  private toast = inject(ToastService);
  private userState = inject(UserStateService);
  private fb = inject(FormBuilder);

  private refresh$ = new BehaviorSubject<void>(undefined);
  private query$ = new BehaviorSubject<Query>({ page: 1 });
  readonly pageSize = 12;

  filterForm = this.fb.nonNullable.group({
    search: ['', Validators.maxLength(100)],
    countryPublicId: [''],
  });

  countries = toSignal(this.countryService.getAllForDropdown().pipe(catchError(() => of([]))), { initialValue: [] });

  state = toSignal(
    combineLatest([this.query$, this.refresh$]).pipe(
      switchMap(([q]) => this.competitionService.getAll(q.page, this.pageSize, q.search, q.countryPublicId).pipe(
        map((data) => ({ data, loading: false, error: '' })),
        startWith({ data: emptyPage<CompetitionListDto>(), loading: true, error: '' }),
        catchError((err) => of({ data: emptyPage<CompetitionListDto>(), loading: false, error: apiErrorMessage(err, 'Competitions could not be loaded.') })),
      )),
    ),
    { initialValue: { data: emptyPage<CompetitionListDto>(), loading: true, error: '' } },
  );

  // Mirrors the backend: Admin/Moderator can add and edit, only Admin can delete.
  canManage = this.userState.canManage;
  canDelete = this.userState.isAdmin;

  isFormOpen = signal(false);
  selected = signal<CompetitionDetailsDto | null>(null);

  constructor() {
    this.filterForm.valueChanges.pipe(debounceTime(300), filter(() => this.filterForm.valid), takeUntilDestroyed())
      .subscribe(() => this.apply());
  }

  apply(): void {
    if (this.filterForm.invalid) return;
    const { search, countryPublicId } = this.filterForm.getRawValue();
    this.query$.next({ page: 1, search: search.trim() || undefined, countryPublicId: countryPublicId || undefined });
  }

  chips(): FilterChip[] {
    const { search, countryPublicId } = this.filterForm.getRawValue();
    const chips: FilterChip[] = [];
    if (search.trim()) chips.push({ key: 'search', label: `"${search.trim()}"` });
    const country = this.countries().find((item) => item.publicId === countryPublicId)?.name;
    if (country) chips.push({ key: 'countryPublicId', label: country });
    return chips;
  }

  clearChip(key: string): void {
    this.filterForm.get(key)?.setValue('');
  }

  resetFilters(): void {
    this.filterForm.reset();
  }

  retry(): void {
    this.refresh$.next();
  }

  onPageChange(page: number): void {
    this.query$.next({ ...this.query$.value, page });
  }

  onAdd(): void {
    if (!this.canManage()) return;
    this.selected.set(null);
    this.isFormOpen.set(true);
  }

  onEdit(competition: CompetitionListDto): void {
    if (!this.canManage()) return;
    this.competitionService.getBySlug(competition.slug).subscribe({
      next: (details) => {
        this.selected.set(details);
        this.isFormOpen.set(true);
      },
      error: (err) => this.toast.error(apiErrorMessage(err, 'The competition could not be loaded.')),
    });
  }

  async onDelete(competition: CompetitionListDto): Promise<void> {
    if (!this.canDelete()) return;
    const confirmed = await this.confirmDialog.confirm(deleteConfirm(`${competition.name}`));
    if (!confirmed) return;
    this.competitionService.delete(competition.publicId).subscribe({
      next: () => {
        this.toast.success('Competition deleted');
        this.refresh$.next();
      },
      error: (err) => this.toast.error(apiErrorMessage(err, 'The competition could not be deleted.')),
    });
  }

  onFormSaved(): void {
    this.isFormOpen.set(false);
    this.selected.set(null);
    this.refresh$.next();
  }

  onFormClosed(): void {
    this.isFormOpen.set(false);
    this.selected.set(null);
  }
}
