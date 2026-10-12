import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, filter, map, of, shareReplay, switchMap } from 'rxjs';
import { CompetitionService } from '../../core/services/competition.service';
import { SeasonService } from '../../core/services/season.service';
import { CompetitionDetailsDto } from '../../core/models/competition';
import { SeasonDetailsDto, SeasonListDto } from '../../core/models/season';
import { UserStateService } from '../../core/services/user-state.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { SeasonForm } from '../../components/season-form/season-form';
import { apiErrorMessage } from '../../core/utils/api-error';

@Component({
  selector: 'app-competition-detail',
  standalone: true,
  imports: [RouterLink, GenericListComponent, SeasonForm],
  templateUrl: './competition-detail.html',
  styleUrls: ['../catalog.css', '../competition-list/competition-pages.css'],
})
export class CompetitionDetail {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private competitionService = inject(CompetitionService);
  private seasonService = inject(SeasonService);
  private confirmDialog = inject(ConfirmDialogService);
  private userState = inject(UserStateService);

  private refresh$ = new BehaviorSubject<void>(undefined);
  private page$ = new BehaviorSubject<number>(1);
  readonly pageSize = 10;

  errorMessage = signal('');

  private competition$ = this.route.paramMap.pipe(
    map((params) => params.get('slug') ?? ''),
    switchMap((slug) =>
      this.competitionService.getBySlug(slug).pipe(
        catchError((err) => {
          this.errorMessage.set(apiErrorMessage(err, 'Competition not found.'));
          return of(null);
        })
      )
    ),
    shareReplay(1)
  );

  competition = toSignal<CompetitionDetailsDto | null>(this.competition$);

  seasons = toSignal(
    combineLatest([this.competition$.pipe(filter((c): c is CompetitionDetailsDto => !!c)), this.page$, this.refresh$]).pipe(
      switchMap(([competition, page]) => this.seasonService.getAll(page, this.pageSize, competition.id))
    )
  );

  private roles = toSignal(this.userState.roles$, { initialValue: this.userState.getRoles() });
  canManage = computed(() => this.roles().some((r) => r === 'Admin' || r === 'Moderator'));
  canDelete = computed(() => this.roles().includes('Admin'));

  isFormOpen = false;
  selected: SeasonDetailsDto | null = null;

  onPageChange(page: number): void {
    this.page$.next(page);
  }

  open(season: SeasonListDto): void {
    this.router.navigate(['/seasons', season.slug]);
  }

  onAdd(): void {
    this.selected = null;
    this.isFormOpen = true;
  }

  onEdit(season: SeasonListDto): void {
    this.seasonService.getBySlug(season.slug).subscribe({
      next: (details) => {
        this.selected = details;
        this.isFormOpen = true;
      },
      error: (err) => this.errorMessage.set(apiErrorMessage(err)),
    });
  }

  async onDelete(season: SeasonListDto): Promise<void> {
    const confirmed = await this.confirmDialog.confirm({
      message: `Are you sure you want to delete season ${season.name}?`,
    });
    if (!confirmed) return;
    this.errorMessage.set('');
    this.seasonService.delete(season.id).subscribe({
      next: () => this.refresh$.next(),
      error: (err) => this.errorMessage.set(apiErrorMessage(err)),
    });
  }

  onFormSaved(): void {
    this.isFormOpen = false;
    this.selected = null;
    this.refresh$.next();
  }

  onFormClosed(): void {
    this.isFormOpen = false;
    this.selected = null;
  }
}
