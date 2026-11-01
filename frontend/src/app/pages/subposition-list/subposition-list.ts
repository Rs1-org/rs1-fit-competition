import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { toSignal } from '@angular/core/rxjs-interop';
import { signal } from '@angular/core';
import { BehaviorSubject, catchError, combineLatest, of, switchMap, tap } from 'rxjs';
import { emptyPage } from '../../core/models/paged-result';
import { Skeleton } from '../../shared/components/skeleton/skeleton';
import { ErrorBanner } from '../../shared/components/error-banner/error-banner';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { ConfirmDialogService, deleteConfirm } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { UserStateService } from '../../core/services/user-state.service';
import { SubPositionService } from '../../core/services/subposition.service';
import { SubPosition } from '../../core/models/formation.models';
import { SubPositionForm } from '../../components/subposition-form/subposition-form';
import { ToastService } from '../../core/services/toast.service';
import { apiErrorMessage } from '../../core/utils/api-error';

@Component({
  selector: 'app-subposition-list',
  imports: [CommonModule, GenericListComponent, GenericSearch, SubPositionForm, Skeleton, ErrorBanner],
  templateUrl: './subposition-list.html',
  styleUrl: '../catalog.css',
})
export class SubPositionList {
  private service = inject(SubPositionService);
  private confirmDialog = inject(ConfirmDialogService);
  private toast = inject(ToastService);
  private userState = inject(UserStateService);
  private refresh$ = new BehaviorSubject<void>(undefined);
  private page$ = new BehaviorSubject<number>(1);
  private search$ = new BehaviorSubject<string>('');
  readonly pageSize = 10;

  loadError = signal('');

  result = toSignal(
    combineLatest([this.page$, this.search$, this.refresh$]).pipe(
      switchMap(([page, term]) => this.service.getAll(page, this.pageSize, term || undefined).pipe(
        tap(() => this.loadError.set('')),
        catchError((err) => {
          this.loadError.set(apiErrorMessage(err, 'Subpositions could not be loaded.'));
          return of(emptyPage<SubPosition>());
        }),
      )),
    ),
  );

  retry(): void { this.refresh$.next(); }

  user = this.userState.getUser();
  isFormOpen = false;
  selected: SubPosition | null = null;

  onPageChange(page: number): void { this.page$.next(page); }
  search(term: string): void { this.search$.next(term); this.page$.next(1); }
  onAdd(): void { this.selected = null; this.isFormOpen = true; }
  onEdit(item: SubPosition): void { this.selected = item; this.isFormOpen = true; }

  async onDelete(item: SubPosition): Promise<void> {
    const confirmed = await this.confirmDialog.confirm(deleteConfirm(`subposition ${item.name}`));
    if (!confirmed) return;
    this.service.delete(item.publicId).subscribe({
      next: () => {
        this.toast.success('Subposition deleted');
        this.refresh$.next();
      },
      error: (err) => this.toast.error(apiErrorMessage(err, 'The subposition could not be deleted.')),
    });
  }

  onFormSaved(): void { this.isFormOpen = false; this.selected = null; this.refresh$.next(); }
  onFormClosed(): void { this.isFormOpen = false; this.selected = null; }
}
