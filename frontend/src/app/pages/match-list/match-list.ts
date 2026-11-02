import { Component, computed, inject, signal } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, debounceTime, filter, map, of, startWith, switchMap } from 'rxjs';
import { AppDatePipe, AppDayLabelPipe, AppTimePipe } from '../../shared/pipes/regional-format.pipes';
import { Pagination } from '../../shared/components/pagination/pagination';
import { MatchRow } from '../../shared/components/match-row/match-row';
import { ConfirmDialogService, deleteConfirm } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { MatchService } from '../../core/services/match.service';
import { MatchStatusService } from '../../core/services/match-status.service';
import { StadiumService } from '../../core/services/stadium.service';
import { CompetitionService } from '../../core/services/competition.service';
import { UserStateService } from '../../core/services/user-state.service';
import { MatchListDto, MatchListFilter, MatchPhase } from '../../core/models/match';
import { ExportFormat, SortParams } from '../../core/models/import-export';
import { emptyPage } from '../../core/models/paged-result';
import { roundLabel } from '../../core/helpers/match.helper';
import { saveBlob } from '../../core/helpers/download.helper';
import { fetchAllPages, MAX_PAGE_SIZE } from '../../core/helpers/paging.helper';
import { ariaSort, nextSort, sortIndicator } from '../../core/helpers/sort-state.helper';
import { apiErrorMessage } from '../../core/utils/api-error';
import { ToastService } from '../../core/services/toast.service';
import { ErrorBanner } from '../../shared/components/error-banner/error-banner';
import { FilterCard, FilterChip } from '../../shared/components/filter-card/filter-card';
import { EmptyState } from '../../shared/components/empty-state/empty-state';
import { Skeleton } from '../../shared/components/skeleton/skeleton';
import { ImportExportBar } from '../../shared/components/import-export-bar/import-export-bar';
import { Button } from '../../shared/components/button/button';
import { Icon } from '../../shared/components/icon/icon';
import { Crest } from '../../shared/components/crest/crest';
import { ScoreDisplay } from '../../shared/components/score-display/score-display';
import { StatusBadge } from '../../shared/components/status-badge/status-badge';

const NEWEST_FIRST: SortParams = { sortBy: 'date', sortDir: 'desc' };

/** "Date to" must not be before "date from". */
export function dateRangeValidator(group: AbstractControl): ValidationErrors | null {
  const { dateFrom, dateTo } = group.value;
  return dateFrom && dateTo && dateTo < dateFrom ? { dateRange: true } : null;
}

@Component({
  selector: 'app-match-list',
  imports: [
    ReactiveFormsModule, RouterLink, NgTemplateOutlet, Pagination, AppDayLabelPipe, AppTimePipe, MatchRow, ErrorBanner, FilterCard,
    EmptyState, Skeleton, ImportExportBar, Button, Icon, Crest, ScoreDisplay, StatusBadge,
  ],
  templateUrl: './match-list.html',
  styleUrl: './match-list.css',
})
export class MatchList {
  private matchService = inject(MatchService);
  private statusService = inject(MatchStatusService);
  private stadiumService = inject(StadiumService);
  private competitionService = inject(CompetitionService);
  private confirmDialog = inject(ConfirmDialogService);
  private toast = inject(ToastService);
  private userState = inject(UserStateService);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private datePipe = new AppDatePipe();

  readonly pageSize = 10;
  readonly exportFormats: ExportFormat[] = ['xlsx'];
  readonly tabs: { label: string; phase: MatchPhase | undefined }[] = [
    { label: 'All', phase: undefined },
    { label: 'Live', phase: 'live' },
    { label: 'Results', phase: 'results' },
    { label: 'Upcoming', phase: 'upcoming' },
  ];
  private refresh$ = new BehaviorSubject<void>(undefined);
  private query$ = new BehaviorSubject<{ page: number; filter: MatchListFilter }>({ page: 1, filter: { ...NEWEST_FIRST } });

  filterForm = this.fb.nonNullable.group(
    {
      statusPublicId: '',
      stadiumPublicId: '',
      competitionPublicId: '',
      postponed: '' as '' | 'true' | 'false',
      dateFrom: '',
      dateTo: '',
    },
    { validators: dateRangeValidator },
  );

  statuses = toSignal(fetchAllPages((page) => this.statusService.getAll(page, MAX_PAGE_SIZE)), { initialValue: [] });
  stadiums = toSignal(fetchAllPages((page) => this.stadiumService.getAll(page, MAX_PAGE_SIZE)), { initialValue: [] });
  competitions = toSignal(fetchAllPages((page) => this.competitionService.getAll(page, MAX_PAGE_SIZE)), { initialValue: [] });

  // Mirrors the backend: Admin/Moderator can add, edit and export, only Admin can delete and import.
  canManage = this.userState.canManage;
  canDelete = this.userState.isAdmin;
  roundLabel = roundLabel;
  readonly ariaSort = ariaSort;
  readonly sortIndicator = sortIndicator;

  phase = signal<MatchPhase | undefined>(undefined);
  sort = signal<SortParams>(NEWEST_FIRST);
  sortNote = computed(() => {
    const { sortBy, sortDir } = this.sort();
    if (sortBy === 'date') return `Sorted by date, ${sortDir === 'desc' ? 'newest' : 'oldest'} first`;
    return `Sorted by ${sortBy} team, ${sortDir === 'desc' ? 'Z to A' : 'A to Z'}`;
  });

  state = toSignal(
    combineLatest([this.query$, this.refresh$]).pipe(
      switchMap(([{ page, filter }]) =>
        this.matchService.getAll(page, this.pageSize, filter).pipe(
          map((data) => ({ data, loading: false, error: '' })),
          startWith({ data: emptyPage<MatchListDto>(), loading: true, error: '' }),
          catchError((err) =>
            of({ data: emptyPage<MatchListDto>(), loading: false, error: apiErrorMessage(err, 'Matches could not be loaded.') }),
          ),
        ),
      ),
    ),
    { initialValue: { data: emptyPage<MatchListDto>(), loading: true, error: '' } },
  );

  // Export uses the filters, tab and sort currently applied to the table.
  exportFn = (format: ExportFormat) => this.matchService.export(this.query$.value.filter, format);
  importFn = (file: File) => this.matchService.import(file);

  constructor() {
    // Any filter change goes back to page 1; an invalid date range is not sent.
    this.filterForm.valueChanges
      .pipe(debounceTime(300), filter(() => this.filterForm.valid), takeUntilDestroyed())
      .subscribe(() => this.search());
  }

  private search(): void {
    this.query$.next({ page: 1, filter: { ...this.toFilter(), phase: this.phase(), ...this.sort() } });
  }

  private toFilter(): MatchListFilter {
    const { statusPublicId, stadiumPublicId, competitionPublicId, postponed, dateFrom, dateTo } = this.filterForm.getRawValue();
    return {
      statusPublicId: statusPublicId || undefined,
      stadiumPublicId: stadiumPublicId || undefined,
      competitionPublicId: competitionPublicId || undefined,
      isPostponed: postponed === '' ? undefined : postponed === 'true',
      dateFrom: dateFrom || undefined,
      // Inclusive "to" day: the backend compares instants, so send the end of that day.
      dateTo: dateTo ? `${dateTo}T23:59:59` : undefined,
    };
  }

  chips(): FilterChip[] {
    const { statusPublicId, stadiumPublicId, competitionPublicId, postponed, dateFrom, dateTo } = this.filterForm.getRawValue();
    const chips: FilterChip[] = [];
    const add = (key: string, label: string | null | undefined) => {
      if (label) chips.push({ key, label });
    };
    add('statusPublicId', this.statuses().find((item) => item.publicId === statusPublicId)?.name);
    add('competitionPublicId', this.competitions().find((item) => item.publicId === competitionPublicId)?.name);
    add('stadiumPublicId', this.stadiums().find((item) => item.publicId === stadiumPublicId)?.name);
    if (dateFrom) add('dateFrom', `From ${this.datePipe.transform(dateFrom)}`);
    if (dateTo) add('dateTo', `To ${this.datePipe.transform(dateTo)}`);
    if (postponed) add('postponed', postponed === 'true' ? 'Postponed only' : 'Not postponed');
    return chips;
  }

  clearChip(key: string): void {
    this.filterForm.get(key)?.setValue('');
  }

  apply(): void {
    if (this.filterForm.valid) this.search();
  }

  selectPhase(phase: MatchPhase | undefined): void {
    this.phase.set(phase);
    this.search();
  }

  sortBy(key: string): void {
    this.sort.update((current) => nextSort(current, key));
    this.search();
  }

  winner(match: MatchListDto): 'home' | 'away' | null {
    const { homeScore: home, awayScore: away } = match;
    if (home === null || away === null || home === away) return null;
    return home > away ? 'home' : 'away';
  }

  matchName(match: MatchListDto): string {
    return `${match.homeSelectionName ?? 'TBD'} vs ${match.awaySelectionName ?? 'TBD'}`;
  }

  /** The report needs a full, valid date range. */
  canDownloadReport(): boolean {
    const { dateFrom, dateTo } = this.filterForm.getRawValue();
    return this.filterForm.valid && !!dateFrom && !!dateTo;
  }

  downloadReport(): void {
    if (!this.canDownloadReport()) return;
    const { dateFrom, dateTo } = this.filterForm.getRawValue();
    this.matchService.downloadMatchesReport(this.toFilter()).subscribe({
      next: (blob) => saveBlob(blob, `matches-report-${dateFrom}-${dateTo}.pdf`),
      error: (err) => this.toast.error(apiErrorMessage(err, 'The report could not be downloaded.')),
    });
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

  schedule(): void {
    this.router.navigate(['/manage-matches']);
  }

  view(match: MatchListDto): void {
    this.router.navigate(['/matches', match.slug]);
  }

  edit(match: MatchListDto): void {
    this.router.navigate(['/manage-matches', match.slug]);
  }

  async delete(match: MatchListDto): Promise<void> {
    if (!this.canDelete()) return;
    const confirmed = await this.confirmDialog.confirm(deleteConfirm(this.matchName(match)));
    if (!confirmed) return;
    this.matchService.delete(match.publicId).subscribe({
      next: () => {
        this.toast.success('Match deleted');
        this.refresh$.next();
      },
      error: (err) => this.toast.error(apiErrorMessage(err, 'The match could not be deleted.')),
    });
  }
}
