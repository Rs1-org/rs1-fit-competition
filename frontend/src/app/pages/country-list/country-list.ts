import { Component, inject, OnInit } from '@angular/core';
import { CountryForm } from '../../components/country-form/country-form';
import { CountryService } from '../../core/services/country-service';
import { CountryListDto } from '../../core/models/country';

@Component({
  selector: 'app-country-list',
  imports: [CountryForm],
  templateUrl: './country-list.html',
  styleUrl: './country-list.css',
})
export class CountryList implements OnInit {
  private countryService = inject(CountryService);

  countries: CountryListDto[] = [];
  isFormOpen = false;
  selectedCountry: CountryListDto | null = null;

  ngOnInit(): void {
    this.loadCountries();
  }

  loadCountries(): void {
    this.countryService.getAll().subscribe({
      next: (data) => { this.countries = data; },
      error: (err) => console.error(err)
    });
  }

  onAdd(): void {
    this.selectedCountry = null;
    this.isFormOpen = true;
  }

  onEdit(country: CountryListDto): void {
    this.selectedCountry = country;
    this.isFormOpen = true;
  }

  onDelete(id: string): void {
    if (!confirm('Are you sure you want to delete this country?')) return;
    this.countryService.delete(id).subscribe({
      next: () => this.loadCountries(),
      error: (err) => console.error(err)
    });
  }

  onFormSaved(): void {
    this.isFormOpen = false;
    this.selectedCountry = null;
    this.loadCountries();
  }

  onFormClosed(): void {
    this.isFormOpen = false;
    this.selectedCountry = null;
  }
}
