import { Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, combineLatest, switchMap } from 'rxjs';
import { CompetitionService } from '../../core/services/competition.service';
import { CompetitionDetailsDto, CompetitionListDto } from '../../core/models/competition';
import { UserStateService } from '../../core/services/user-state.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { CompetitionForm } from '../../components/competition-form/competition-form';
import { apiErrorMessage } from '../../core/utils/api-error';

@Component({
  selector: 'app-competition-list',
  standalone: true,
  imports: [GenericListComponent, GenericSearch, CompetitionForm],
  templateUrl: './competition-list.html',
  styleUrls: ['../catalog.css', './competition-pages.css'],
})
export class CompetitionList {
  private competitionService = inject(CompetitionService);
  private confirmDialog = inject(ConfirmDialogService);
  private userState = inject(UserStateService);
  private router = inject(Router);

  private refresh$ = new BehaviorSubject<void>(undefined);
  private page$ = new BehaviorSubject<number>(1);
  private search$ = new BehaviorSubject<string>('');
  readonly pageSize = 10;

  competitions = toSignal(
    combineLatest([this.page$, this.search$, this.refresh$]).pipe(
      switchMap(([page, term]) => this.competitionService.getAll(page, this.pageSize, term || undefined))
    )
  );

  private roles = toSignal(this.userState.roles$, { initialValue: this.userState.getRoles() });
  canManage = computed(() => this.roles().some((r) => r === 'Admin' || r === 'Moderator'));
  canDelete = computed(() => this.roles().includes('Admin'));

  errorMessage = signal('');
  isFormOpen = false;
  selected: CompetitionDetailsDto | null = null;

  search(term: string): void {
    this.search$.next(term);
    this.page$.next(1);
  }

  onPageChange(page: number): void {
    this.page$.next(page);
  }

  open(competition: CompetitionListDto): void {
    this.router.navigate(['/competitions', competition.slug]);
  }

  onAdd(): void {
    this.selected = null;
    this.isFormOpen = true;
  }

  onEdit(competition: CompetitionListDto): void {
    this.competitionService.getBySlug(competition.slug).subscribe({
      next: (details) => {
        this.selected = details;
        this.isFormOpen = true;
      },
      error: (err) => this.errorMessage.set(apiErrorMessage(err)),
    });
  }

  async onDelete(competition: CompetitionListDto): Promise<void> {
    const confirmed = await this.confirmDialog.confirm({
      message: `Are you sure you want to delete ${competition.name}?`,
    });
    if (!confirmed) return;
    this.errorMessage.set('');
    this.competitionService.delete(competition.publicId).subscribe({
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
