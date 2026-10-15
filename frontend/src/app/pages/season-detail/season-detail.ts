import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, filter, map, of, shareReplay, switchMap } from 'rxjs';
import { SeasonService } from '../../core/services/season.service';
import { RoundService } from '../../core/services/round.service';
import { SeasonDetailsDto } from '../../core/models/season';
import { RoundListDto } from '../../core/models/round';
import { UserStateService } from '../../core/services/user-state.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { RoundForm } from '../../components/round-form/round-form';
import { apiErrorMessage } from '../../core/utils/api-error';
import { AppDatePipe } from '../../shared/pipes/regional-format.pipes';

@Component({
  selector: 'app-season-detail',
  standalone: true,
  imports: [RouterLink, GenericListComponent, RoundForm, AppDatePipe],
  templateUrl: './season-detail.html',
  styleUrls: ['../catalog.css', '../competition-list/competition-pages.css'],
})
export class SeasonDetail {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private seasonService = inject(SeasonService);
  private roundService = inject(RoundService);
  private confirmDialog = inject(ConfirmDialogService);
  private userState = inject(UserStateService);

  private refresh$ = new BehaviorSubject<void>(undefined);
  private page$ = new BehaviorSubject<number>(1);
  readonly pageSize = 20;

  errorMessage = signal('');

  private season$ = this.route.paramMap.pipe(
    map((params) => params.get('slug') ?? ''),
    switchMap((slug) =>
      this.seasonService.getBySlug(slug).pipe(
        catchError((err) => {
          this.errorMessage.set(apiErrorMessage(err, 'Season not found.'));
          return of(null);
        })
      )
    ),
    shareReplay(1)
  );

  season = toSignal<SeasonDetailsDto | null>(this.season$);

  rounds = toSignal(
    combineLatest([this.season$.pipe(filter((s): s is SeasonDetailsDto => !!s)), this.page$, this.refresh$]).pipe(
      switchMap(([season, page]) => this.roundService.getAll(page, this.pageSize, season.id))
    )
  );

  /** Suggest the next round number for the add form (highest known number + 1, so gaps don't collide). */
  nextNumber = computed(() => {
    const result = this.rounds();
    const highestShown = Math.max(0, ...(result?.items ?? []).map((r) => r.number));
    return Math.max(highestShown, result?.totalCount ?? 0) + 1;
  });

  private roles = toSignal(this.userState.roles$, { initialValue: this.userState.getRoles() });
  canManage = computed(() => this.roles().some((r) => r === 'Admin' || r === 'Moderator'));
  canDelete = computed(() => this.roles().includes('Admin'));

  isFormOpen = false;
  selected: RoundListDto | null = null;

  onPageChange(page: number): void {
    this.page$.next(page);
  }

  open(round: RoundListDto): void {
    this.router.navigate(['/rounds', round.slug]);
  }

  onAdd(): void {
    this.selected = null;
    this.isFormOpen = true;
  }

  onEdit(round: RoundListDto): void {
    this.selected = round;
    this.isFormOpen = true;
  }

  async onDelete(round: RoundListDto): Promise<void> {
    const confirmed = await this.confirmDialog.confirm({
      message: `Are you sure you want to delete ${round.name}?`,
    });
    if (!confirmed) return;
    this.errorMessage.set('');
    this.roundService.delete(round.id).subscribe({
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
