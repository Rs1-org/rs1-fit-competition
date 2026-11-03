import { Component, EventEmitter, inject, Input, OnChanges, OnInit, Output, signal, SimpleChanges } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputBox } from '../../shared/components/input-box/input-box';
import { Modal } from '../../shared/components/modal/modal';
import { CountryListDto } from '../../core/models/country';
import { CityDetailsDto } from '../../core/models/city';
import { CountryService } from '../../core/services/country-service';
import { CityService } from '../../core/services/city-services';
import { ToastService } from '../../core/services/toast.service';
import { Button } from '../../shared/components/button/button';
import { Dropdown } from '../../shared/components/dropdown/dropdown';
import { Map } from '../../shared/components/map/map';
import { showServerErrors } from '../../core/utils/api-error';

/** Where the map opens; a new city starts here until a point is picked (Sarajevo). */
export const DEFAULT_CITY_POINT = { latitude: 43.8563, longitude: 18.4131 };

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
  private toast = inject(ToastService);

  countries = signal<CountryListDto[]>([]);
  saving = signal(false);
  formError = signal('');

  form = this.fb.group({
    name: this.fb.control('', { validators: [Validators.required, Validators.maxLength(100)] }),
    population: this.fb.control(0, { validators: [Validators.required, Validators.min(1)] }),
    isCapital: this.fb.control(false),
    countryPublicId: this.fb.control('', { validators: Validators.required }),
    latitude: this.fb.control(DEFAULT_CITY_POINT.latitude, { validators: [Validators.required, Validators.min(-90), Validators.max(90)] }),
    longitude: this.fb.control(DEFAULT_CITY_POINT.longitude, { validators: [Validators.required, Validators.min(-180), Validators.max(180)] }),
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
      this.formError.set('');
      if (this.editCity) {
        this.form.patchValue({
          name: this.editCity.name,
          population: this.editCity.population,
          isCapital: this.editCity.isCapital,
          countryPublicId: this.editCity.countryPublicId,
          latitude: Number(this.editCity.latitude),
          longitude: Number(this.editCity.longitude),
          description: this.editCity.description
        });
      } else {
        this.form.reset();
      }
    }
  }

  loadCountries(): void {
    this.countryService.getAllForDropdown().subscribe({
      next: (countries) => this.countries.set(countries),
      error: () => this.formError.set('Countries could not be loaded. Close the form and try again.'),
    });
  }

  onLocationSelected(event: { lat: number, lng: number }) {
    this.form.patchValue({ latitude: round(event.lat), longitude: round(event.lng) });
    this.form.controls.latitude.markAsDirty();
    this.form.controls.longitude.markAsDirty();
  }

  /** The pin follows the latitude and longitude fields while both are valid. */
  pinLat(): number | null {
    return this.form.controls.latitude.valid && this.form.controls.longitude.valid ? this.form.controls.latitude.value : null;
  }

  pinLng(): number | null {
    return this.pinLat() === null ? null : this.form.controls.longitude.value;
  }

  onClose(): void {
    this.form.reset();
    this.formError.set('');
    this.closed.emit();
  }

  onSubmit(): void {
    if (this.saving()) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.form.getRawValue();
    const editing = this.isEditMode;
    const request$ = editing ? this.cityService.update(this.editCity!.slug, payload) : this.cityService.create(payload);
    this.saving.set(true);
    this.formError.set('');
    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success(editing ? 'City updated' : 'City added');
        this.form.reset();
        this.saved.emit();
      },
      error: (err) => {
        this.saving.set(false);
        const message = showServerErrors(this.form, err, 'The city could not be saved.');
        this.formError.set(message);
        if (message) this.toast.error(message);
      },
    });
  }
}

/** Six decimals are about 10 cm. */
function round(value: number): number {
  return Math.round(value * 1e6) / 1e6;
}
