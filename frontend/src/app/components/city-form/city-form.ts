import { Component, EventEmitter, inject, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputBox } from '../../shared/components/input-box/input-box';
import { Modal } from '../../shared/components/modal/modal';
import { CountryListDto } from '../../core/models/country';
import { CityDetailsDto } from '../../core/models/city';
import { CountryService } from '../../core/services/country-service';
import { CityService } from '../../core/services/city-services';
import { Button } from '../../shared/components/button/button';
import { Dropdown } from '../../shared/components/dropdown/dropdown';
import { Map } from '../../shared/components/map/map';

@Component({
  selector: 'app-city-form',
  imports: [InputBox, Modal, ReactiveFormsModule, Button, Dropdown, Map],
  templateUrl: './city-form.html',
  styleUrl: './city-form.css',
})
export class CityForm implements OnInit, OnChanges {
  @Input() isOpen = false;
  @Input() editCity: CityDetailsDto | null = null;
  @Output() saved = new EventEmitter<void>();
  @Output() closed = new EventEmitter<void>();

  private fb = inject(NonNullableFormBuilder);
  private countryService = inject(CountryService);
  private cityService = inject(CityService);

  countries: CountryListDto[] = [];

  form = this.fb.group({
    name: this.fb.control('', { validators: [Validators.required, Validators.maxLength(100)] }),
    population: this.fb.control(0, { validators: [Validators.required, Validators.min(1)] }),
    isCapital: this.fb.control(false),
    countryId: this.fb.control('', { validators: Validators.required }),
    latitude: this.fb.control(40, { validators: [Validators.required, Validators.min(-90), Validators.max(90)] }),
    longitude: this.fb.control(41, { validators: [Validators.required, Validators.min(-180), Validators.max(180)] }),
    description: this.fb.control('', { validators: Validators.required })
  });

  get isEditMode(): boolean {
    return this.editCity !== null;
  }

  ngOnInit(): void {
    this.loadCountries();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['editCity']) {
      if (this.editCity) {
        this.form.patchValue({
          name: this.editCity.name,
          population: this.editCity.population,
          isCapital: this.editCity.isCapital,
          countryId: this.editCity.countryId,
          latitude: Number(this.editCity.latitude),
          longitude: Number(this.editCity.longitude),
          description: this.editCity.description
        });
      } else {
        this.form.reset({ latitude: 40, longitude: 41 });
      }
    }
  }

  loadCountries(): void {
    this.countryService.getAll().subscribe({
      next: (data) => { this.countries = data; },
      error: (err) => console.error('Error loading countries:', err)
    });
  }

  onLocationSelected(event: { lat: number, lng: number }) {
    this.form.patchValue({ latitude: event.lat, longitude: event.lng });
  }

  onClose(): void {
    this.form.reset({ latitude: 40, longitude: 41 });
    this.closed.emit();
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.form.value;

    if (this.isEditMode) {
      this.cityService.update(this.editCity!.slug, payload).subscribe({
        next: () => {
          this.form.reset({ latitude: 40, longitude: 41 });
          this.saved.emit();
        },
        error: (err) => console.error(err)
      });
    } else {
      this.cityService.create(payload).subscribe({
        next: () => {
          this.form.reset({ latitude: 40, longitude: 41 });
          this.saved.emit();
        },
        error: (err) => console.error(err)
      });
    }
  }
}
