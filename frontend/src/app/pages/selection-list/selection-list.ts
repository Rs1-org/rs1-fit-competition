import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, combineLatest, map, switchMap } from 'rxjs';
import { Router } from '@angular/router';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { UserStateService } from '../../core/services/user-state.service';
import { SelectionOption, SelectionService } from '../../core/services/selection.service';

@Component({
  selector: 'app-selection-list',
  imports: [CommonModule, GenericListComponent, GenericSearch],
  templateUrl: './selection-list.html',
  styleUrl: '../catalog.css',
})
export class SelectionList {
  private service = inject(SelectionService);
  private confirmDialog = inject(ConfirmDialogService);
  private userState = inject(UserStateService);
  private router = inject(Router);
  private refresh$ = new BehaviorSubject<void>(undefined);
  private page$ = new BehaviorSubject<number>(1);
  private search$ = new BehaviorSubject<string>('');
  readonly pageSize = 10;

  result = toSignal(
    combineLatest([this.page$, this.search$, this.refresh$]).pipe(
      switchMap(([page, term]) => this.service.getAll(page, this.pageSize, term || undefined)),
    ),
  );

  canManage = toSignal(
    this.userState.roles$.pipe(map(() => this.userState.canManagePersons())),
    { initialValue: this.userState.canManagePersons() },
  );

  onPageChange(page: number): void { this.page$.next(page); }
  search(term: string): void { this.search$.next(term); this.page$.next(1); }
  onAdd(): void { this.router.navigate(['/manage-selections']); }
  onEdit(item: SelectionOption): void { this.router.navigate(['/manage-selections', item.slug]); }

  async onDelete(item: SelectionOption): Promise<void> {
    const confirmed = await this.confirmDialog.confirm({ message: `Delete selection ${item.name}?` });
    if (!confirmed) return;
    this.service.delete(item.id).subscribe({ next: () => this.refresh$.next() });
  }
}
