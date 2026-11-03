import { TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { SeasonForm } from './season-form';
import { SeasonService } from '../../core/services/season.service';
import { ToastService } from '../../core/services/toast.service';

describe('SeasonForm', () => {
  let component: SeasonForm;
  let service: { create: ReturnType<typeof vi.fn>; update: ReturnType<typeof vi.fn> };
  let toast: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    service = { create: vi.fn(() => of({})), update: vi.fn(() => of({})) };
    toast = { success: vi.fn(), error: vi.fn() };
    TestBed.configureTestingModule({
      imports: [SeasonForm],
      providers: [{ provide: SeasonService, useValue: service }, { provide: ToastService, useValue: toast }],
    });
    component = TestBed.createComponent(SeasonForm).componentInstance;
    component.competitionPublicId = 'c1';
  });

  it('does not send an end date before the start date', () => {
    component.form.setValue({ name: '2026/27', startDate: '2026-08-01', endDate: '2026-07-01', isCurrent: false });

    component.onSubmit();

    expect(component.form.hasError('endBeforeStart')).toBe(true);
    expect(service.create).not.toHaveBeenCalled();
  });

  it('adds the season to its competition and shows a toast', () => {
    component.form.setValue({ name: '2026/27', startDate: '2026-07-26', endDate: '2027-05-30', isCurrent: true });

    component.onSubmit();

    expect(service.create).toHaveBeenCalledWith(expect.objectContaining({ competitionPublicId: 'c1', name: '2026/27', isCurrent: true }));
    expect(toast.success).toHaveBeenCalledWith('Season added');
  });

  it('shows an error without a field as a toast and a form message', () => {
    service.create.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 500, error: { message: 'Server is down.' } })));
    component.form.setValue({ name: '2026/27', startDate: '2026-07-26', endDate: '2027-05-30', isCurrent: false });

    component.onSubmit();

    expect(component.errorMessage()).toBe('Server is down.');
    expect(toast.error).toHaveBeenCalledWith('Server is down.');
  });
});
