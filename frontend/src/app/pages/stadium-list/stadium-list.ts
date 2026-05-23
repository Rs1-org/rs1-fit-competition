import { Component, OnInit, ChangeDetectorRef, inject, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { Router } from '@angular/router';
import { StadiumService } from '../../core/services/stadium.service';
import { StadiumListDto } from '../../core/models/stadium';
import { StadiumForm } from '../../components/stadium-form/stadium-form';
import { PagedResult } from '../../core/models/paged-result';
import { ConfirmDialog } from '../../shared/components/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-stadium-list',
  standalone: true,
  imports: [GenericListComponent, CommonModule, GenericSearch, StadiumForm, ConfirmDialog],
  templateUrl: './stadium-list.html',
  styleUrl: './stadium-list.css',
})
export class StadiumList implements OnInit {
  @ViewChild(StadiumForm)    stadiumFormRef!: StadiumForm;
  @ViewChild(ConfirmDialog)  confirmDialogRef!: ConfirmDialog;

  pagedResult: PagedResult<StadiumListDto> | null = null;
  currentPage = 1;
  readonly pageSize = 10;

  pendingDelete: StadiumListDto | null = null;

  private stadiumService = inject(StadiumService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  ngOnInit() {
    this.loadStadiums();
  }

  loadStadiums(page = this.currentPage) {
    this.stadiumService.getAll(page, this.pageSize).subscribe({
      next: (data) => {
        this.pagedResult = data;
        this.currentPage = data.page;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error loading stadiums: ', err)
    });
  }

  onPageChange(page: number) {
    this.loadStadiums(page);
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

  delete(stadium: StadiumListDto) {
    this.pendingDelete = stadium;
    this.confirmDialogRef.open();
  }

  confirmDelete() {
    if (!this.pendingDelete) return;
    this.stadiumService.delete(this.pendingDelete.id).subscribe({
      next: () => {
        this.pendingDelete = null;
        this.loadStadiums(this.currentPage);
      },
      error: (err) => {
        this.pendingDelete = null;
        console.error('Error deleting stadium: ', err);
      }
    });
  }

  search(term: string) {
    if (!term) {
      this.loadStadiums(1);
    }
  }
}
