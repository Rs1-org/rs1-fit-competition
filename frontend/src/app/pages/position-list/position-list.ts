import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, combineLatest, switchMap } from 'rxjs';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { UserStateService } from '../../core/services/user-state.service';
import { PositionService } from '../../core/services/position.service';
import { Position } from '../../core/models/formation.models';
import { PositionForm } from '../../components/position-form/position-form';

@Component({
  selector: 'app-position-list',
  imports: [CommonModule, GenericListComponent, GenericSearch, PositionForm],
  templateUrl: './position-list.html',
  styleUrl: '../catalog.css',
})
export class PositionList {
  private service = inject(PositionService);
  private confirmDialog = inject(ConfirmDialogService);
  private userState = inject(UserStateService);
  private refresh$ = new BehaviorSubject<void>(undefined);
  private page$ = new BehaviorSubject<number>(1);
  private search$ = new BehaviorSubject<string>('');
  readonly pageSize = 10;

  result = toSignal(
    combineLatest([this.page$, this.search$, this.refresh$]).pipe(
      switchMap(([page, term]) => this.service.getAll(page, this.pageSize, term || undefined)),
    ),
  );

  user = this.userState.getUser();
  isFormOpen = false;
  selected: Position | null = null;

  onPageChange(page: number): void { this.page$.next(page); }
  search(term: string): void { this.search$.next(term); this.page$.next(1); }
  onAdd(): void { this.selected = null; this.isFormOpen = true; }
  onEdit(item: Position): void { this.selected = item; this.isFormOpen = true; }

  async onDelete(item: Position): Promise<void> {
    const confirmed = await this.confirmDialog.confirm({ message: `Delete position ${item.name}?` });
    if (!confirmed) return;
    this.service.delete(item.id).subscribe({ next: () => this.refresh$.next() });
  }

  onFormSaved(): void { this.isFormOpen = false; this.selected = null; this.refresh$.next(); }
  onFormClosed(): void { this.isFormOpen = false; this.selected = null; }
}
