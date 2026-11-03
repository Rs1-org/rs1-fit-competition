import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { StadiumForm } from './stadium-form';
import { CityService } from '../../core/services/city-services';
import { StadiumService } from '../../core/services/stadium.service';
import { UserStateService } from '../../core/services/user-state.service';
import { ToastService } from '../../core/services/toast.service';
import { StadiumDetailsDto } from '../../core/models/stadium';

describe('StadiumForm', () => {
  let component: StadiumForm;
  let fixture: ComponentFixture<StadiumForm>;
  let stadiumService: { create: ReturnType<typeof vi.fn>; update: ReturnType<typeof vi.fn> };
  let toast: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };

  const grbavica: StadiumDetailsDto = {
    publicId: 's1', name: 'Grbavica', capacity: 13146, dateOfConstruction: '1953-01-01', slug: 'grbavica', pictureUrl: '',
    cityPublicId: 'sa', cityName: 'Sarajevo', latitude: 43.8524, longitude: 18.3963, geoDescription: 'Zvornička 27',
    cityLatitude: 43.8563, cityLongitude: 18.4131,
  };

  async function setup(canManage = true): Promise<void> {
    stadiumService = { create: vi.fn(() => of(grbavica)), update: vi.fn(() => of({ ...grbavica, slug: 'grbavica-new' })) };
    toast = { success: vi.fn(), error: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [StadiumForm],
      providers: [
        { provide: CityService, useValue: { getAllForDropdown: () => of([{ publicId: 'sa', name: 'Sarajevo' }]) } },
        { provide: StadiumService, useValue: stadiumService },
        { provide: UserStateService, useValue: { canManage: signal(canManage) } },
        { provide: ToastService, useValue: toast },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(StadiumForm);
    component = fixture.componentInstance;
    component.showTrigger = false;
    fixture.detectChanges();
    await fixture.whenStable();
  }

  it('does not open for visitors', async () => {
    await setup(false);

    component.openForEdit(grbavica);

    expect(component.modalState.isOpen()).toBe(false);
  });

  it('loads the stadium, including its coordinates, for editing', async () => {
    await setup();

    component.openForEdit(grbavica);

    expect(component.modalState.isOpen()).toBe(true);
    expect(component.form.getRawValue()).toEqual(expect.objectContaining({
      name: 'Grbavica', dateOfConstruction: '1953-01-01', cityPublicId: 'sa', latitude: 43.8524, longitude: 18.3963,
    }));
    expect(component.pinLat()).toBe(43.8524);
  });

  it('keeps the map pin and the coordinate fields in step', async () => {
    await setup();
    component.openModal();

    component.onLocationSelected({ lat: 43.1234567, lng: 17.7654321 });
    expect(component.form.controls.latitude.value).toBe(43.123457);
    expect(component.form.controls.longitude.value).toBe(17.765432);

    component.form.controls.longitude.setValue(-200);
    expect(component.pinLat()).toBeNull();
  });

  it('saves, shows a toast and hands the saved stadium back', async () => {
    await setup();
    const saved = vi.fn();
    component.saved.subscribe(saved);
    component.openForEdit(grbavica);

    component.onSubmit();

    expect(stadiumService.update).toHaveBeenCalledWith('grbavica', expect.any(FormData), undefined);
    expect(toast.success).toHaveBeenCalledWith('Stadium updated');
    expect(saved).toHaveBeenCalledWith(expect.objectContaining({ slug: 'grbavica-new' }));
    expect(component.modalState.isOpen()).toBe(false);
  });

  it('shows a server error on its field and stays open', async () => {
    await setup();
    stadiumService.update.mockReturnValue(throwError(() => new HttpErrorResponse({
      status: 400, error: { errors: [{ propertyName: 'Capacity', errorMessage: 'Capacity is too large.' }] },
    })));
    component.openForEdit(grbavica);

    component.onSubmit();

    expect(component.form.controls.capacity.getError('server')).toBe('Capacity is too large.');
    expect(component.modalState.isOpen()).toBe(true);
    expect(component.saving()).toBe(false);
  });
});
