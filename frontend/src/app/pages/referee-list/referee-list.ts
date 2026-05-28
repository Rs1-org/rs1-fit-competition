import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { Router } from '@angular/router';
import { Referee } from '../../core/models/person.models';
import { UserStateService } from '../../core/services/user-state.service';
import { RefereeService } from '../../core/services/referee.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { BehaviorSubject, combineLatest, map, switchMap } from 'rxjs';

@Component({
  selector: 'app-referee-list',
  imports: [GenericListComponent, CommonModule, GenericSearch],
  templateUrl: './referee-list.html',
  styleUrl: './referee-list.css',
})
export class RefereeList {
  private refereeService = inject(RefereeService);
  private confirmDialog = inject(ConfirmDialogService);
  private router = inject(Router);
  private userState = inject(UserStateService);

  private searchTerm$ = new BehaviorSubject<string>('');
  private refresh$ = new BehaviorSubject<void>(undefined);

  referees = toSignal(
    combineLatest([this.searchTerm$, this.refresh$]).pipe(
      switchMap(([term]) =>
        term.trim() ? this.refereeService.search(term) : this.refereeService.getAll(),
      ),
    ),
    { initialValue: [] as Referee[] },
  );

  canManagePersons = toSignal(
    this.userState.roles$.pipe(map(() => this.userState.canManagePersons())),
    { initialValue: this.userState.canManagePersons() },
  );

  view(referee: Referee): void {
    this.router.navigate(['/referee', referee.id]);
  }

  edit(referee: Referee): void {
    this.router.navigate(['/manage-referees', referee.id]);
  }

  async delete(referee: Referee): Promise<void> {
    if (!referee.id) return;

    const confirmed = await this.confirmDialog.confirm({
      message: `Jeste li sigurni da želite obrisati suca ${referee.firstName} ${referee.lastName}?`,
    });
    if (!confirmed) return;

    this.refereeService.delete(referee.id).subscribe({
      next: () => this.refresh$.next(),
      error: (err) => console.error('Greška pri brisanju suca:', err),
    });
  }

  search(term: string): void {
    this.searchTerm$.next(term);
  }
}
