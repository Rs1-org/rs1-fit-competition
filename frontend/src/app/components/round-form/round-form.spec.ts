import { TestBed } from '@angular/core/testing';
import { SimpleChange } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { RoundForm } from './round-form';
import { RoundService } from '../../core/services/round.service';
import { ToastService } from '../../core/services/toast.service';

describe('RoundForm', () => {
  let component: RoundForm;
  let service: { create: ReturnType<typeof vi.fn>; update: ReturnType<typeof vi.fn> };
  let toast: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    service = { create: vi.fn(() => of({})), update: vi.fn(() => of({})) };
    toast = { success: vi.fn(), error: vi.fn() };
    TestBed.configureTestingModule({
      imports: [RoundForm],
      providers: [{ provide: RoundService, useValue: service }, { provide: ToastService, useValue: toast }],
    });
    component = TestBed.createComponent(RoundForm).componentInstance;
    component.seasonPublicId = 's1';
    component.nextNumber = 4;
    component.ngOnChanges({ isOpen: new SimpleChange(false, true, false) });
  });

  it('suggests the next free round number and name', () => {
    expect(component.form.getRawValue()).toEqual({ number: 4, name: 'Round 4' });
  });

  it('adds the round to its season and shows a toast', () => {
    component.onSubmit();

    expect(service.create).toHaveBeenCalledWith({ name: 'Round 4', number: 4, seasonPublicId: 's1' });
    expect(toast.success).toHaveBeenCalledWith('Round added');
  });

  it('puts a taken round number error on the field', () => {
    service.create.mockReturnValue(throwError(() => new HttpErrorResponse({
      status: 400, error: { errors: [{ propertyName: 'Number', errorMessage: 'Round 4 already exists in this season.' }] },
    })));

    component.onSubmit();

    expect(component.form.controls.number.getError('server')).toBe('Round 4 already exists in this season.');
    expect(component.saving()).toBe(false);
  });
});
