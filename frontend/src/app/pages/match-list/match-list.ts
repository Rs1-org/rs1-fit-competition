import { Component, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, debounceTime, filter, of, switchMap } from 'rxjs';
import { AppDateTimePipe } from '../../shared/pipes/regional-format.pipes';
import { Pagination } from '../../shared/components/pagination/pagination';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { MatchService } from '../../core/services/match.service';
import { MatchStatusService } from '../../core/services/match-status.service';
import { StadiumService } from '../../core/services/stadium.service';
import { UserStateService } from '../../core/services/user-state.service';
import { MatchListDto, MatchListFilter } from '../../core/models/match';
import { fetchAllPages, MAX_PAGE_SIZE } from '../../core/helpers/paging.helper';
import { apiErrorMessage } from '../../core/utils/api-error';

/** "Date to" must not be before "date from". */
export function dateRangeValidator(group: AbstractControl): ValidationErrors | null {
  const { dateFrom, dateTo } = group.value;
  return dateFrom && dateTo && dateTo < dateFrom ? { dateRange: true } : null;
}

@Component({
  selector: 'app-match-list',
  imports: [ReactiveFormsModule, RouterLink, Pagination, AppDateTimePipe],
  templateUrl: './match-list.html',
  styleUrl: '../stadium-list/stadium-list.css',
})
export class MatchList {
  private matchService = inject(MatchService);
  private statusService = inject(MatchStatusService);
  private stadiumService = inject(StadiumService);
  private confirmDialog = inject(ConfirmDialogService);
  private userState = inject(UserStateService);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  readonly pageSize = 10;
  private refresh$ = new BehaviorSubject<void>(undefined);
  private query$ = new BehaviorSubject<{ page: number; filter: MatchListFilter }>({ page: 1, filter: {} });

  filterForm = this.fb.nonNullable.group(
    {
      statusPublicId: '',
      stadiumPublicId: '',
      postponed: '' as '' | 'true' | 'false',
      dateFrom: '',
      dateTo: '',
    },
    { validators: dateRangeValidator },
  );

  statuses = toSignal(fetchAllPages((page) => this.statusService.getAll(page, MAX_PAGE_SIZE)), { initialValue: [] });
  stadiums = toSignal(fetchAllPages((page) => this.stadiumService.getAll(page, MAX_PAGE_SIZE)), { initialValue: [] });

  // Mirrors the backend: Admin/Moderator can add and edit, only Admin can delete.
  canManage = this.userState.canManage;
  canDelete = this.userState.isAdmin;
  error = '';

  pagedResult = toSignal(
    combineLatest([this.query$, this.refresh$]).pipe(
      switchMap(([{ page, filter }]) =>
        this.matchService.getAll(page, this.pageSize, filter).pipe(
          catchError((err) => {
            this.error = apiErrorMessage(err, 'Matches could not be loaded.');
            return of(null);
          }),
        ),
      ),
    ),
  );

  constructor() {
    // Any filter change goes back to page 1; an invalid date range is not sent.
    this.filterForm.valueChanges
      .pipe(debounceTime(300), filter(() => this.filterForm.valid), takeUntilDestroyed())
      .subscribe(() => {
        this.error = '';
        this.query$.next({ page: 1, filter: this.toFilter() });
      });
  }

  private toFilter(): MatchListFilter {
    const { statusPublicId, stadiumPublicId, postponed, dateFrom, dateTo } = this.filterForm.getRawValue();
    return {
      statusPublicId: statusPublicId || undefined,
      stadiumPublicId: stadiumPublicId || undefined,
      isPostponed: postponed === '' ? undefined : postponed === 'true',
      dateFrom: dateFrom || undefined,
      // Inclusive "to" day: the backend compares instants, so send the end of that day.
      dateTo: dateTo ? `${dateTo}T23:59:59` : undefined,
    };
  }

  resetFilters(): void {
    this.filterForm.reset();
  }

  onPageChange(page: number): void {
    this.query$.next({ ...this.query$.value, page });
  }

  view(match: MatchListDto): void {
    this.router.navigate(['/matches', match.slug]);
  }

  edit(match: MatchListDto): void {
    this.router.navigate(['/manage-matches', match.slug]);
  }

  async delete(match: MatchListDto): Promise<void> {
    if (!this.canDelete()) return;
    const confirmed = await this.confirmDialog.confirm({
      message: `Are you sure you want to delete ${match.homeSelectionName ?? 'TBD'} vs ${match.awaySelectionName ?? 'TBD'}?`,
    });
    if (!confirmed) return;
    this.matchService.delete(match.publicId).subscribe({
      next: () => this.refresh$.next(),
      error: (err) => (this.error = apiErrorMessage(err, 'The match could not be deleted.')),
    });
  }
}
