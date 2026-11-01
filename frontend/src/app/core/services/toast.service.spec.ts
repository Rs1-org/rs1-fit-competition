import { TestBed } from '@angular/core/testing';
import { ToastService } from './toast.service';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    vi.useFakeTimers();
    service = TestBed.inject(ToastService);
  });

  afterEach(() => vi.useRealTimers());

  it('keeps toasts in order with their kind', () => {
    service.success('Saved');
    service.error('Failed');
    expect(service.toasts().map(toast => [toast.kind, toast.message])).toEqual([['success', 'Saved'], ['error', 'Failed']]);
  });

  it('dismisses one toast and leaves the rest', () => {
    service.success('First');
    service.success('Second');
    service.dismiss(service.toasts()[0].id);
    expect(service.toasts().map(toast => toast.message)).toEqual(['Second']);
  });

  it('does nothing when the timer fires after a manual dismiss', () => {
    service.success('Saved');
    service.dismiss(service.toasts()[0].id);
    vi.advanceTimersByTime(4000);
    expect(service.toasts()).toEqual([]);
  });
});
