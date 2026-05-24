import { Component, inject, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { Router } from '@angular/router';
import { StadiumService } from '../../core/services/stadium.service';
import { StadiumListDto } from '../../core/models/stadium';
import { StadiumForm } from '../../components/stadium-form/stadium-form';
import { PagedResult } from '../../core/models/paged-result';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { UserStateService } from '../../core/services/user-state.service';
import { UserDto } from '../../core/models/auth.models';

@Component({
  selector: 'app-stadium-list',
  standalone: true,
  imports: [GenericListComponent, CommonModule, GenericSearch, StadiumForm],
  templateUrl: './stadium-list.html',
  styleUrl: './stadium-list.css',
})
export class StadiumList implements OnInit {
  @ViewChild(StadiumForm) stadiumFormRef!: StadiumForm;

  pagedResult: PagedResult<StadiumListDto> | null = null;
  currentPage = 1;
  readonly pageSize = 10;

  private stadiumService   = inject(StadiumService);
  private confirmDialog    = inject(ConfirmDialogService);
  private router           = inject(Router);
  private cdr              = inject(ChangeDetectorRef);
  private userStateService = inject(UserStateService);

  user: UserDto | null = null;

  ngOnInit() {
    this.userStateService.user$.subscribe((u) => (this.user = u));
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

  async delete(stadium: StadiumListDto) {
    const confirmed = await this.confirmDialog.confirm({
      message: `Jeste li sigurni da želite obrisati ${stadium.name}?`
    });
    if (!confirmed) return;

    this.stadiumService.delete(stadium.id).subscribe({
      next: () => this.loadStadiums(this.currentPage),
      error: (err) => console.error('Error deleting stadium: ', err)
    });
  }

  search(term: string) {
    if (!term) {
      this.loadStadiums(1);
    }
  }
}
