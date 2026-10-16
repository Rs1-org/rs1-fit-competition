import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SimpleChange } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { CityForm } from './city-form';
import { CityDetailsDto } from '../../core/models/city';

describe('CityForm', () => {
  let component: CityForm;
  let fixture: ComponentFixture<CityForm>;

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

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CityForm],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(CityForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('loads the saved coordinates when editing a city', () => {
    component.editCity = savedCity;
    component.ngOnChanges({ editCity: new SimpleChange(null, savedCity, true) });

    expect(component.isEditMode).toBe(true);
    expect(component.form.controls.latitude.value).toBe(43.3438);
    expect(component.form.controls.longitude.value).toBe(17.8078);
  });

  it('writes a moved marker back into the form', () => {
    component.editCity = savedCity;
    component.ngOnChanges({ editCity: new SimpleChange(null, savedCity, true) });

    component.onLocationSelected({ lat: 43.35, lng: 17.81 });

    expect(component.form.controls.latitude.value).toBe(43.35);
    expect(component.form.controls.longitude.value).toBe(17.81);
    expect(component.form.controls.latitude.dirty).toBe(true);
  });
});
