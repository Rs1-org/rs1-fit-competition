import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { toSignal } from '@angular/core/rxjs-interop';
import { signal } from '@angular/core';
import { BehaviorSubject, catchError, combineLatest, map, of, switchMap, tap } from 'rxjs';
import { emptyPage } from '../../core/models/paged-result';
import { Skeleton } from '../../shared/components/skeleton/skeleton';
import { ErrorBanner } from '../../shared/components/error-banner/error-banner';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { ConfirmDialogService, deleteConfirm } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { UserStateService } from '../../core/services/user-state.service';
import { FormationService } from '../../core/services/formation.service';
import { Formation } from '../../core/models/formation.models';
import { Router } from '@angular/router';
import { ToastService } from '../../core/services/toast.service';
import { apiErrorMessage } from '../../core/utils/api-error';

@Component({
  selector: 'app-formation-list',
  imports: [CommonModule, GenericListComponent, GenericSearch, Skeleton, ErrorBanner],
  templateUrl: './formation-list.html',
  styleUrls: ['../catalog.css', './formation-list.css'],
})
export class FormationList {
  private service = inject(FormationService);
  private confirmDialog = inject(ConfirmDialogService);
  private toast = inject(ToastService);
  private userState = inject(UserStateService);
  private router = inject(Router);
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
          this.loadError.set(apiErrorMessage(err, 'Formations could not be loaded.'));
          return of(emptyPage<Formation>());
        }),
      )),
    ),
  );

  retry(): void { this.refresh$.next(); }

  canManage = toSignal(
    this.userState.roles$.pipe(map(() => this.userState.canManagePersons())),
    { initialValue: this.userState.canManagePersons() },
  );

  isAdmin = toSignal(
    this.userState.roles$.pipe(map(() => this.userState.isAdmin())),
    { initialValue: this.userState.isAdmin() },
  );

  onPageChange(page: number): void { this.page$.next(page); }
  search(term: string): void { this.search$.next(term); this.page$.next(1); }
  onAdd(): void { this.router.navigate(['/manage-formations']); }
  onEdit(item: Formation): void { this.router.navigate(['/manage-formations', item.slug]); }

  async onDelete(item: Formation): Promise<void> {
    const confirmed = await this.confirmDialog.confirm(deleteConfirm(`formation ${item.code}`));
    if (!confirmed) return;
    this.service.delete(item.id).subscribe({
      next: () => {
        this.toast.success('Formation deleted');
        this.refresh$.next();
      },
      error: (err) => this.toast.error(apiErrorMessage(err, 'The formation could not be deleted.')),
    });
  }

}
