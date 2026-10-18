import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';

import { Modal } from './modal';

describe('Modal', () => {
  let component: Modal;
  let fixture: ComponentFixture<Modal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Modal],
      providers: [provideNoopAnimations()],
    }).compileComponents();

    fixture = TestBed.createComponent(Modal);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('names the dialog and closes on Escape', () => {
    fixture.detectChanges();
    const dialog: HTMLElement = fixture.nativeElement.querySelector('[role="dialog"]');
    expect(dialog.getAttribute('aria-modal')).toBe('true');
    expect(dialog.getAttribute('aria-labelledby')).toBe(component.titleId);
    const close = vi.spyOn(component.close, 'emit');
    dialog.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
    expect(close).toHaveBeenCalledOnce();
  });

  it('keeps Tab focus inside the dialog', () => {
    fixture.detectChanges();
    const dialog: HTMLElement = fixture.nativeElement.querySelector('[role="dialog"]');
    const close: HTMLButtonElement = dialog.querySelector('button')!;
    const last = document.createElement('button');
    last.textContent = 'Confirm';
    dialog.append(last);
    const nextFocus = vi.spyOn(close, 'focus');
    const previousFocus = vi.spyOn(last, 'focus');
    component.onKeydown(new KeyboardEvent('keydown', { key: 'Tab' }));
    expect(nextFocus).toHaveBeenCalled();
    component.onKeydown(new KeyboardEvent('keydown', { key: 'Tab', shiftKey: true }));
    expect(previousFocus).toHaveBeenCalled();
  });
});
