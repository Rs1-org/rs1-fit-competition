import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CountryForm } from '../../components/country-form/country-form';
import { CountryService } from '../../core/services/country-service';
import { CountryListDto } from '../../core/models/country';
import { UserStateService } from '../../core/services/user-state.service';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, combineLatest, switchMap } from 'rxjs';
import { GenericListComponent } from '../../generic/generic-list/generic-list.component';
import { GenericSearch } from '../../generic/generic-search/generic-search';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';

@Component({
  selector: 'app-country-list',
  standalone: true,
  imports: [CountryForm, GenericListComponent, CommonModule, GenericSearch],
  templateUrl: './country-list.html',
  styleUrl: './country-list.css',
})
export class CountryList {
  private countryService = inject(CountryService);
  private confirmDialog = inject(ConfirmDialogService);
  private userStateService = inject(UserStateService);
  private refresh$ = new BehaviorSubject<void>(undefined);
  private page$ = new BehaviorSubject<number>(1);
  private search$ = new BehaviorSubject<string>('');
  readonly pageSize = 10;

  countriesResult = toSignal(
    combineLatest([this.page$, this.search$, this.refresh$]).pipe(
      switchMap(([page, term]) => this.countryService.getAll(page, this.pageSize, term || undefined))
    )
  );

  user = this.userStateService.getUser();
  isFormOpen = false;
  selectedCountry: CountryListDto | null = null;

  onPageChange(page: number) {
    this.page$.next(page);
  }

  onAdd(): void {
    this.selectedCountry = null;
    this.isFormOpen = true;
  }

  onEdit(country: CountryListDto): void {
    this.selectedCountry = country;
    this.isFormOpen = true;
  }

  async onDelete(country: CountryListDto): Promise<void> {
    const confirmed = await this.confirmDialog.confirm({
      message: `Are you sure you want to delete ${country.name}?`
    });
    if (!confirmed) return;
    this.countryService.delete(country.id).subscribe({
      next: () => this.refresh$.next(),
      error: (err) => console.error(err)
    });
  }

  search(term: string) {
    this.search$.next(term);
    this.page$.next(1);
  }

  onFormSaved(): void {
    this.isFormOpen = false;
    this.selectedCountry = null;
    this.refresh$.next();
  }

  onFormClosed(): void {
    this.isFormOpen = false;
    this.selectedCountry = null;
  }
}
