import { Component, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, distinctUntilChanged, filter, map, of, startWith, switchMap } from 'rxjs';
import { CompetitionService } from '../../core/services/competition.service';
import { SeasonService } from '../../core/services/season.service';
import { CompetitionDetailsDto } from '../../core/models/competition';
import { SeasonDetailsDto, SeasonListDto } from '../../core/models/season';
import { emptyPage } from '../../core/models/paged-result';
import { UserStateService } from '../../core/services/user-state.service';
import { ConfirmDialogService, deleteConfirm } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { SeasonForm } from '../../components/season-form/season-form';
import { apiErrorMessage } from '../../core/utils/api-error';
import { AppDatePipe, AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { ToastService } from '../../core/services/toast.service';
import { Breadcrumb, Crumb } from '../../shared/components/breadcrumb/breadcrumb';
import { EntityHeader } from '../../shared/components/entity-header/entity-header';
import { StatTile } from '../../shared/components/stat-tile/stat-tile';
import { Pagination } from '../../shared/components/pagination/pagination';
import { Skeleton } from '../../shared/components/skeleton/skeleton';
import { EmptyState } from '../../shared/components/empty-state/empty-state';
import { ErrorBanner } from '../../shared/components/error-banner/error-banner';
import { Button } from '../../shared/components/button/button';
import { Icon } from '../../shared/components/icon/icon';
import { competitionCode } from '../../shared/match/match-display';

interface CompetitionState { data: CompetitionDetailsDto | null; loading: boolean; error: string; notFound: boolean; }

@Component({
  selector: 'app-competition-detail',
  standalone: true,
  imports: [
    RouterLink, SeasonForm, AppDatePipe, AppNumberPipe, Breadcrumb, EntityHeader, StatTile, Pagination, Skeleton, EmptyState,
    ErrorBanner, Button, Icon,
  ],
  templateUrl: './competition-detail.html',
  styleUrl: './competition-detail.css',
})
export class CompetitionDetail {
  readonly code = competitionCode;
  private route = inject(ActivatedRoute);
  private competitionService = inject(CompetitionService);
  private seasonService = inject(SeasonService);
  private confirmDialog = inject(ConfirmDialogService);
  private toast = inject(ToastService);
  private userState = inject(UserStateService);

  private reload$ = new BehaviorSubject<void>(undefined);
  private refresh$ = new BehaviorSubject<void>(undefined);
  private page$ = new BehaviorSubject<number>(1);
  readonly pageSize = 12;

  canManage = this.userState.canManage;
  canDelete = this.userState.isAdmin;

  state = toSignal(
    combineLatest([this.route.paramMap.pipe(map((params) => params.get('slug') ?? '')), this.reload$]).pipe(
      switchMap(([slug]) => this.competitionService.getBySlug(slug).pipe(
        map((data): CompetitionState => ({ data, loading: false, error: '', notFound: false })),
        startWith<CompetitionState>({ data: null, loading: true, error: '', notFound: false }),
        catchError((err) => of<CompetitionState>(err instanceof HttpErrorResponse && err.status === 404
          ? { data: null, loading: false, error: '', notFound: true }
          : { data: null, loading: false, error: apiErrorMessage(err, 'The competition could not be loaded.'), notFound: false })),
      )),
    ),
    { initialValue: { data: null, loading: true, error: '', notFound: false } as CompetitionState },
  );

  competition = computed(() => this.state().data);
  private competition$ = toObservable(this.competition).pipe(
    filter((c): c is CompetitionDetailsDto => !!c),
    distinctUntilChanged((a, b) => a.publicId === b.publicId),
  );

  seasons = toSignal(
    combineLatest([this.competition$, this.page$, this.refresh$]).pipe(
      switchMap(([c, page]) => this.seasonService.getAll(page, this.pageSize, c.publicId).pipe(
        map((data) => ({ data, loading: false, error: '' })),
        startWith({ data: emptyPage<SeasonListDto>(), loading: true, error: '' }),
        catchError((err) => of({ data: emptyPage<SeasonListDto>(), loading: false, error: apiErrorMessage(err, 'Seasons could not be loaded.') })),
      )),
    ),
    { initialValue: { data: emptyPage<SeasonListDto>(), loading: true, error: '' } },
  );

  /** The season marked current, wherever it is in the list. */
  currentSeason = toSignal(
    combineLatest([this.competition$, this.refresh$]).pipe(
      switchMap(([c]) => this.seasonService.getAll(1, 1, c.publicId, undefined, true).pipe(
        map((page) => page.items[0] ?? null),
        catchError(() => of(null)),
      )),
    ),
    { initialValue: null as SeasonListDto | null },
  );

  crumbs = computed<Crumb[]>(() => [
    { label: 'Competitions', route: '/competitions' },
    { label: this.competition()?.name ?? 'Competition' },
  ]);

  subtitle = computed(() => {
    const c = this.competition();
    return c ? [c.countryName || 'International', c.shortName].filter(Boolean).join(' · ') : '';
  });

  isFormOpen = signal(false);
  selected = signal<SeasonDetailsDto | null>(null);

  retry(): void {
    this.reload$.next();
  }

  retrySeasons(): void {
    this.refresh$.next();
  }

  onPageChange(page: number): void {
    this.page$.next(page);
  }

  onAdd(): void {
    if (!this.canManage()) return;
    this.selected.set(null);
    this.isFormOpen.set(true);
  }

  onEdit(season: SeasonListDto): void {
    if (!this.canManage()) return;
    this.seasonService.getBySlug(season.slug).subscribe({
      next: (details) => {
        this.selected.set(details);
        this.isFormOpen.set(true);
      },
      error: (err) => this.toast.error(apiErrorMessage(err, 'The season could not be loaded.')),
    });
  }

  async onDelete(season: SeasonListDto): Promise<void> {
    if (!this.canDelete()) return;
    const confirmed = await this.confirmDialog.confirm(deleteConfirm(`season ${season.name}`));
    if (!confirmed) return;
    this.seasonService.delete(season.publicId).subscribe({
      next: () => {
        this.toast.success('Season deleted');
        this.refresh$.next();
      },
      error: (err) => this.toast.error(apiErrorMessage(err, 'The season could not be deleted.')),
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
