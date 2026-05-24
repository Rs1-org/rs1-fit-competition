import { Component, inject, ViewChild } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { Router } from '@angular/router';
import { StadiumService } from '../../core/services/stadium.service';
import { StadiumListDto } from '../../core/models/stadium';
import { StadiumForm } from '../../components/stadium-form/stadium-form';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { UserStateService } from '../../core/services/user-state.service';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, combineLatest, switchMap } from 'rxjs';

@Component({
  selector: 'app-stadium-list',
  standalone: true,
  imports: [GenericListComponent, CommonModule, GenericSearch, StadiumForm, DecimalPipe],
  templateUrl: './stadium-list.html',
  styleUrl: './stadium-list.css',
})
export class StadiumList {
  @ViewChild(StadiumForm) stadiumFormRef!: StadiumForm;

  private stadiumService   = inject(StadiumService);
  private confirmDialog    = inject(ConfirmDialogService);
  private router           = inject(Router);
  private userStateService = inject(UserStateService);

  private refresh$ = new BehaviorSubject<void>(undefined);
  private page$ = new BehaviorSubject<number>(1);
  readonly pageSize = 10;

  user = this.userStateService.getUser();

  pagedResult = toSignal(
    combineLatest([this.page$, this.refresh$]).pipe(
      switchMap(([page]) => this.stadiumService.getAll(page, this.pageSize))
    )
  );

  onPageChange(page: number) {
    this.page$.next(page);
  }

  onFormSaved(): void {
    this.refresh$.next();
  }

  view(stadium: StadiumListDto) {
    this.router.navigate(['/stadium', stadium.slug]);
  }

  edit(stadium: StadiumListDto) {
    this.stadiumService.getBySlug(stadium.slug).subscribe({
      next: (data) => this.stadiumFormRef.openForEdit(data),
      error: (err) => console.error('Error loading stadium details: ', err)
    });
  }

  async delete(stadium: StadiumListDto) {
    const confirmed = await this.confirmDialog.confirm({
      message: `Jeste li sigurni da želite obrisati ${stadium.name}?`
    });
    if (!confirmed) return;

    this.stadiumService.delete(stadium.id).subscribe({
      next: () => this.refresh$.next(),
      error: (err) => console.error('Error deleting stadium: ', err)
    });
  }

  search(term: string) {
    if (!term) {
      this.page$.next(1);
    }
  }
}
