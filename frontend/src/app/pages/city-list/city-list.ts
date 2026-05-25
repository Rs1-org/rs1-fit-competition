import { Component, inject } from '@angular/core';
import { DecimalPipe, CommonModule } from '@angular/common';
import { CityForm } from '../../components/city-form/city-form';
import { CityService } from '../../core/services/city-services';
import { CityDetailsDto, CityListDto } from '../../core/models/city';
import { UserStateService } from '../../core/services/user-state.service';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, combineLatest, switchMap } from 'rxjs';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';

@Component({
  selector: 'app-city-list',
  standalone: true,
  imports: [CityForm, DecimalPipe, GenericListComponent, CommonModule, GenericSearch],
  templateUrl: './city-list.html',
  styleUrl: './city-list.css',
})
export class CityList {
  private cityService = inject(CityService);
  private confirmDialog = inject(ConfirmDialogService);
  private userStateService = inject(UserStateService);
  private refresh$ = new BehaviorSubject<void>(undefined);
  private page$ = new BehaviorSubject<number>(1);
  readonly pageSize = 10;

  citiesResult = toSignal(
    combineLatest([this.page$, this.refresh$]).pipe(
      switchMap(([page]) => this.cityService.getAll(page, this.pageSize))
    )
  );

  user = this.userStateService.getUser();
  isFormOpen = false;
  selectedCity: CityDetailsDto | null = null;

  onPageChange(page: number) {
    this.page$.next(page);
  }

  onAdd(): void {
    this.selectedCity = null;
    this.isFormOpen = true;
  }

  onEdit(city: CityListDto): void {
    this.cityService.getBySlug(city.slug).subscribe({
      next: (details) => {
        this.selectedCity = details;
        this.isFormOpen = true;
      },
      error: (err) => console.error(err)
    });
  }

  async onDelete(city: CityListDto): Promise<void> {
    const confirmed = await this.confirmDialog.confirm({
      message: `Are you sure you want to delete ${city.name}?`
    });
    if (!confirmed) return;
    this.cityService.delete(city.id).subscribe({
      next: () => this.refresh$.next(),
      error: (err) => console.error(err)
    });
  }

  search(term: string) {
    if (!term) {
      this.page$.next(1);
    }
  }

  onFormSaved(): void {
    this.isFormOpen = false;
    this.selectedCity = null;
    this.refresh$.next();
  }

  onFormClosed(): void {
    this.isFormOpen = false;
    this.selectedCity = null;
  }
}