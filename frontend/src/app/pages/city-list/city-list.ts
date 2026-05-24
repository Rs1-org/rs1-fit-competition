import { Component, inject, OnInit } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { CityForm } from '../../components/city-form/city-form';
import { CityService } from '../../core/services/city-services';
import { CityDetailsDto, CityListDto } from '../../core/models/city';
import { UserStateService } from '../../core/services/user-state.service';
import { UserDto } from '../../core/models/auth.models';

@Component({
  selector: 'app-city-list',
  imports: [CityForm, DecimalPipe],
  templateUrl: './city-list.html',
  styleUrl: './city-list.css',
})
export class CityList implements OnInit {
  private cityService = inject(CityService);
  private userStateService = inject(UserStateService);

  cities: CityListDto[] = [];
  isFormOpen = false;
  selectedCity: CityDetailsDto | null = null;
  user: UserDto | null = null;

  ngOnInit(): void {
    this.userStateService.user$.subscribe((u) => (this.user = u));
    this.loadCities();
  }

  loadCities(): void {
    this.cityService.getAll().subscribe({
      next: (data) => { this.cities = data; },
      error: (err) => console.error(err)
    });
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

  onDelete(id: string): void {
    if (!confirm('Are you sure you want to delete this city?')) return;
    this.cityService.delete(id).subscribe({
      next: () => this.loadCities(),
      error: (err) => console.error(err)
    });
  }

  onFormSaved(): void {
    this.isFormOpen = false;
    this.selectedCity = null;
    this.loadCities();
  }

  onFormClosed(): void {
    this.isFormOpen = false;
    this.selectedCity = null;
  }
}
