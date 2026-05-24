import { Component, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { CityForm } from '../../components/city-form/city-form';
import { CityService } from '../../core/services/city-services';
import { CityDetailsDto, CityListDto } from '../../core/models/city';
import { UserStateService } from '../../core/services/user-state.service';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, switchMap } from 'rxjs';

@Component({
  selector: 'app-city-list',
  imports: [CityForm, DecimalPipe],
  templateUrl: './city-list.html',
  styleUrl: './city-list.css',
})
export class CityList {
  private cityService = inject(CityService);
  private userStateService = inject(UserStateService);
  private refresh$ = new BehaviorSubject<void>(undefined);

  cities = toSignal(
    this.refresh$.pipe(switchMap(() => this.cityService.getAll())),
    { initialValue: [] }
  );

  user = this.userStateService.getUser();
  isFormOpen = false;
  selectedCity: CityDetailsDto | null = null;

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

  onDelete(id: string): void {
    if (!confirm('Are you sure you want to delete this city?')) return;
    this.cityService.delete(id).subscribe({
      next: () => this.refresh$.next(),
      error: (err) => console.error(err)
    });
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