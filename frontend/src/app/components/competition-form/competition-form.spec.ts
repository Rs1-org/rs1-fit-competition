import { TestBed } from '@angular/core/testing';
import { SimpleChange } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { CompetitionForm } from './competition-form';
import { CompetitionService } from '../../core/services/competition.service';
import { CountryService } from '../../core/services/country-service';
import { ToastService } from '../../core/services/toast.service';
import { CompetitionDetailsDto } from '../../core/models/competition';

describe('CompetitionForm', () => {
  let component: CompetitionForm;
  let service: { create: ReturnType<typeof vi.fn>; update: ReturnType<typeof vi.fn> };
  let toast: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    service = { create: vi.fn(() => of({})), update: vi.fn(() => of({})) };
    toast = { success: vi.fn(), error: vi.fn() };
    TestBed.configureTestingModule({
      imports: [CompetitionForm],
      providers: [
        { provide: CompetitionService, useValue: service },
        { provide: CountryService, useValue: { getAllForDropdown: () => of([{ publicId: 'bih', name: 'Bosnia and Herzegovina' }]) } },
        { provide: ToastService, useValue: toast },
      ],
    });
    component = TestBed.createComponent(CompetitionForm).componentInstance;
    component.ngOnInit();
  });

  it('offers an international option before the countries', () => {
    expect(component.countries().map((c) => c.name)).toEqual(['No country (international)', 'Bosnia and Herzegovina']);
  });

  it('adds a competition with trimmed values and shows a toast', () => {
    const saved = vi.fn();
    component.saved.subscribe(saved);
    component.form.setValue({ name: ' Premijer liga ', shortName: '', countryPublicId: '', description: '' });

    component.onSubmit();

    expect(service.create).toHaveBeenCalledWith({ name: 'Premijer liga', shortName: null, description: null, countryPublicId: null });
    expect(toast.success).toHaveBeenCalledWith('Competition added');
    expect(saved).toHaveBeenCalled();
  });

  it('puts a duplicate name error on the field when editing', () => {
    const league = { publicId: 'c1', name: 'Premijer liga', slug: 'pl', shortName: null, description: null, countryPublicId: null, countryName: null } as CompetitionDetailsDto;
    component.editCompetition = league;
    component.ngOnChanges({ editCompetition: new SimpleChange(null, league, true) });
    service.update.mockReturnValue(throwError(() => new HttpErrorResponse({
      status: 400, error: { errors: [{ propertyName: 'Name', errorMessage: 'A competition with this name already exists.' }] },
    })));

    component.onSubmit();

    expect(component.form.controls.name.getError('server')).toBe('A competition with this name already exists.');
    expect(toast.error).not.toHaveBeenCalled();
    expect(component.saving()).toBe(false);
  });
});
