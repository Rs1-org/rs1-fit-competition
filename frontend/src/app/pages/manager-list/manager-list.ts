import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { Router } from '@angular/router';
import { Manager } from '../../core/models/person.models';
import { UserStateService } from '../../core/services/user-state.service';
import { ManagerService } from '../../core/services/manager.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { BehaviorSubject, combineLatest, map, switchMap } from 'rxjs';

@Component({
  selector: 'app-manager-list',
  imports: [GenericListComponent, CommonModule, GenericSearch],
  templateUrl: './manager-list.html',
  styleUrl: './manager-list.css',
})
export class ManagerList {
  private managerService = inject(ManagerService);
  private confirmDialog = inject(ConfirmDialogService);
  private router = inject(Router);
  private userState = inject(UserStateService);

  private searchTerm$ = new BehaviorSubject<string>('');
  private refresh$ = new BehaviorSubject<void>(undefined);

  managers = toSignal(
    combineLatest([this.searchTerm$, this.refresh$, this.managerService.listRefreshChanges$]).pipe(
      switchMap(([term]) =>
        term.trim() ? this.managerService.search(term) : this.managerService.getAll(),
      ),
    ),
    { initialValue: [] as Manager[] },
  );

  canManagePersons = toSignal(
    this.userState.roles$.pipe(map(() => this.userState.canManagePersons())),
    { initialValue: this.userState.canManagePersons() },
  );

  view(manager: Manager): void {
    this.router.navigate(['/manager', manager.id]);
  }

  edit(manager: Manager): void {
    this.router.navigate(['/manage-managers', manager.id]);
  }

  async delete(manager: Manager): Promise<void> {
    if (!manager.id) return;

    const confirmed = await this.confirmDialog.confirm({
      message: `Jeste li sigurni da želite obrisati menadžera ${manager.firstName} ${manager.lastName}?`,
    });
    if (!confirmed) return;

    this.managerService.delete(manager.id).subscribe({
      next: () => this.refresh$.next(),
      error: (err) => console.error('Greška pri brisanju menadžera:', err),
    });
  }

  search(term: string): void {
    this.searchTerm$.next(term);
  }
}
