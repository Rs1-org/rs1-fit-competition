import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ToastService } from '../../../core/services/toast.service';
import { ToastHost } from './toast';

describe('ToastHost', () => {
  let fixture: ComponentFixture<ToastHost>;
  let service: ToastService;

  beforeEach(() => {
    vi.useFakeTimers();
    TestBed.configureTestingModule({ imports: [ToastHost] });
    service = TestBed.inject(ToastService);
    fixture = TestBed.createComponent(ToastHost);
  });

  afterEach(() => vi.useRealTimers());

  it('shows success as status and errors as alert', () => {
    service.success('Match saved');
    service.error('Could not delete');
    fixture.detectChanges();
    const toasts: HTMLElement[] = Array.from(fixture.nativeElement.querySelectorAll('.toast'));
    expect(toasts.map(toast => toast.getAttribute('role'))).toEqual(['status', 'alert']);
    expect(toasts[0].textContent).toContain('Match saved');
  });

  it('dismisses with the close button', () => {
    service.success('Match saved');
    fixture.detectChanges();
    fixture.nativeElement.querySelector('.toast-close').click();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.toast')).toBeNull();
  });

  it('removes toasts by themselves, errors stay longer', () => {
    service.success('Match saved');
    service.error('Could not delete');
    vi.advanceTimersByTime(4000);
    expect(service.toasts().map(toast => toast.kind)).toEqual(['error']);
    vi.advanceTimersByTime(2000);
    expect(service.toasts()).toEqual([]);
  });
});
