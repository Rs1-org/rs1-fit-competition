import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SimpleChange } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { CountryForm } from './country-form';
import { CountryService } from '../../core/services/country-service';
import { ToastService } from '../../core/services/toast.service';
import { CountryListDto } from '../../core/models/country';

describe('CountryForm', () => {
  let component: CountryForm;
  let fixture: ComponentFixture<CountryForm>;
  let countryService: { create: ReturnType<typeof vi.fn>; update: ReturnType<typeof vi.fn> };
  let toast: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    countryService = { create: vi.fn(() => of({})), update: vi.fn(() => of({})) };
    toast = { success: vi.fn(), error: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [CountryForm],
      providers: [
        { provide: CountryService, useValue: countryService },
        { provide: ToastService, useValue: toast },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CountryForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('does not send an empty name', () => {
    component.onSubmit();

    expect(component.form.controls.name.touched).toBe(true);
    expect(countryService.create).not.toHaveBeenCalled();
  });

  it('adds a country and shows a toast', () => {
    const saved = vi.fn();
    component.saved.subscribe(saved);
    component.form.controls.name.setValue('Croatia');

    component.onSubmit();

    expect(countryService.create).toHaveBeenCalledTimes(1);
    expect(toast.success).toHaveBeenCalledWith('Country added');
    expect(saved).toHaveBeenCalled();
  });

  it('puts a duplicate name error next to the field when editing', () => {
    const croatia = { publicId: 'hr', name: 'Croatia', slug: 'croatia' } as CountryListDto;
    component.editCountry = croatia;
    component.ngOnChanges({ editCountry: new SimpleChange(null, croatia, true) });
    countryService.update.mockReturnValue(throwError(() => new HttpErrorResponse({
      status: 400, error: { errors: [{ propertyName: 'Name', errorMessage: 'This country already exists.' }] },
    })));

    component.onSubmit();

    expect(countryService.update).toHaveBeenCalledWith('croatia', expect.any(FormData), undefined);
    expect(component.form.controls.name.getError('server')).toBe('This country already exists.');
    expect(component.saving()).toBe(false);
  });
});
