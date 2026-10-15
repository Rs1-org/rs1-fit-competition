import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { Router } from '@angular/router';
import { emptyPage } from '../../core/models/paged-result';
import { Referee } from '../../core/models/person.models';
import { UserStateService } from '../../core/services/user-state.service';
import { RefereeService } from '../../core/services/referee.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { BehaviorSubject, combineLatest, map, switchMap } from 'rxjs';

@Component({
  selector: 'app-referee-list',
  imports: [GenericListComponent, CommonModule, GenericSearch],
  templateUrl: './referee-list.html',
  styleUrls: ['../catalog.css', './referee-list.css'],
})
export class RefereeList {
  private refereeService = inject(RefereeService);
  private confirmDialog = inject(ConfirmDialogService);
  private router = inject(Router);
  private userState = inject(UserStateService);

  private searchTerm$ = new BehaviorSubject<string>('');
  private page$ = new BehaviorSubject<number>(1);
  private refresh$ = new BehaviorSubject<void>(undefined);
  readonly pageSize = 10;

  result = toSignal(
    combineLatest([this.page$, this.searchTerm$, this.refresh$, this.refereeService.listRefreshChanges$]).pipe(
      switchMap(([page, term]) =>
        this.refereeService.getAll(page, this.pageSize, { name: term.trim() || undefined }),
      ),
    ),
    { initialValue: emptyPage<Referee>() },
  );

  canManagePersons = toSignal(
    this.userState.roles$.pipe(map(() => this.userState.canManagePersons())),
    { initialValue: this.userState.canManagePersons() },
  );

  add(): void {
    this.router.navigate(['/manage-referees']);
  }

  view(referee: Referee): void {
    this.router.navigate(['/referee', referee.id]);
  }

  edit(referee: Referee): void {
    this.router.navigate(['/manage-referees', referee.id]);
  }

  async delete(referee: Referee): Promise<void> {
    if (!referee.id) return;

    const confirmed = await this.confirmDialog.confirm({
      message: `Delete referee ${referee.firstName} ${referee.lastName}?`,
    });
    if (!confirmed) return;

    this.refereeService.delete(referee.id).subscribe({
      next: () => this.refresh$.next(),
      error: (err) => console.error('Greška pri brisanju suca:', err),
    });
  }

  search(term: string): void {
    this.searchTerm$.next(term);
    this.page$.next(1);
  }

  onPageChange(page: number): void {
    this.page$.next(page);
  }
}
