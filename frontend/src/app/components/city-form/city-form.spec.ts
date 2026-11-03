import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SimpleChange } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { CityForm, DEFAULT_CITY_POINT } from './city-form';
import { CityDetailsDto } from '../../core/models/city';
import { CityService } from '../../core/services/city-services';
import { CountryService } from '../../core/services/country-service';
import { ToastService } from '../../core/services/toast.service';

describe('CityForm', () => {
  let component: CityForm;
  let fixture: ComponentFixture<CityForm>;
  let cityService: { create: ReturnType<typeof vi.fn>; update: ReturnType<typeof vi.fn> };
  let toast: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };

  const savedCity: CityDetailsDto = {
    name: 'Mostar',
    slug: 'mostar',
    population: 105000,
    isCapital: false,
    latitude: 43.3438,
    longitude: 17.8078,
    description: 'Old Bridge',
    countryName: 'Bosnia and Herzegovina',
    countryPublicId: 'c1'
  };

  function edit(city: CityDetailsDto): void {
    component.editCity = city;
    component.ngOnChanges({ editCity: new SimpleChange(null, city, true) });
  }

  beforeEach(async () => {
    cityService = { create: vi.fn(() => of({})), update: vi.fn(() => of({})) };
    toast = { success: vi.fn(), error: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [CityForm],
      providers: [
        { provide: CityService, useValue: cityService },
        { provide: CountryService, useValue: { getAllForDropdown: () => of([]) } },
        { provide: ToastService, useValue: toast },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CityForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('starts a new city where the map opens', () => {
    component.ngOnChanges({ editCity: new SimpleChange(savedCity, null, false) });

    expect(component.form.controls.latitude.value).toBe(DEFAULT_CITY_POINT.latitude);
    expect(component.form.controls.longitude.value).toBe(DEFAULT_CITY_POINT.longitude);
    expect(component.pinLat()).toBe(DEFAULT_CITY_POINT.latitude);
  });

  it('loads the saved coordinates when editing a city', () => {
    edit(savedCity);

    expect(component.isEditMode).toBe(true);
    expect(component.form.controls.latitude.value).toBe(43.3438);
    expect(component.form.controls.longitude.value).toBe(17.8078);
  });

  it('writes a moved marker back into the fields, rounded to 6 decimals', () => {
    edit(savedCity);

    component.onLocationSelected({ lat: 43.350000123, lng: 17.81 });

    expect(component.form.controls.latitude.value).toBe(43.35);
    expect(component.form.controls.longitude.value).toBe(17.81);
    expect(component.form.controls.latitude.dirty).toBe(true);
  });

  it('hides the pin while a typed coordinate is out of range', () => {
    component.form.controls.latitude.setValue(95);

    expect(component.form.controls.latitude.hasError('max')).toBe(true);
    expect(component.pinLat()).toBeNull();
    expect(component.pinLng()).toBeNull();
  });

  it('saves, shows a toast and tells the list', () => {
    const saved = vi.fn();
    component.saved.subscribe(saved);
    edit(savedCity);

    component.onSubmit();

    expect(cityService.update).toHaveBeenCalledWith('mostar', expect.objectContaining({ name: 'Mostar', latitude: 43.3438 }));
    expect(toast.success).toHaveBeenCalledWith('City updated');
    expect(saved).toHaveBeenCalled();
    expect(component.saving()).toBe(false);
  });

  it('puts a server error next to its field and keeps the form open', () => {
    const saved = vi.fn();
    component.saved.subscribe(saved);
    cityService.update.mockReturnValue(throwError(() => new HttpErrorResponse({
      status: 400, error: { errors: [{ propertyName: 'Name', errorMessage: 'A city with this name already exists.' }] },
    })));
    edit(savedCity);

    component.onSubmit();

    expect(component.form.controls.name.getError('server')).toBe('A city with this name already exists.');
    expect(component.formError()).toBe('');
    expect(toast.error).not.toHaveBeenCalled();
    expect(saved).not.toHaveBeenCalled();
  });

  it('shows a toast when the error has no field', () => {
    cityService.update.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 500, error: { message: 'Database is down.' } })));
    edit(savedCity);

    component.onSubmit();

    expect(component.formError()).toBe('Database is down.');
    expect(toast.error).toHaveBeenCalledWith('Database is down.');
  });
});
