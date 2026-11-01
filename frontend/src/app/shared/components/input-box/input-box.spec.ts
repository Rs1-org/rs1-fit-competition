import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormControl, Validators } from '@angular/forms';

import { InputBox } from './input-box';

describe('InputBox', () => {
  let component: InputBox;
  let fixture: ComponentFixture<InputBox>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InputBox],
    }).compileComponents();

    fixture = TestBed.createComponent(InputBox);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('ties the label to the input', () => {
    fixture.detectChanges();
    const label: HTMLLabelElement = fixture.nativeElement.querySelector('label');
    expect(label.htmlFor).toBe(fixture.nativeElement.querySelector('input').id);
  });

  it('marks a touched invalid field and points to the inline error', () => {
    const control = new FormControl('', Validators.required);
    control.markAsTouched();
    fixture.componentRef.setInput('control', control);
    fixture.detectChanges();
    const field: HTMLInputElement = fixture.nativeElement.querySelector('input');
    const error: HTMLElement = fixture.nativeElement.querySelector('span');
    expect(field.classList).toContain('invalid');
    expect(field.getAttribute('aria-invalid')).toBe('true');
    expect(field.getAttribute('aria-describedby')).toBe(error.id);
    expect(error.textContent).toBe('This field is required.');
  });

  it('shows no error before the field is touched', () => {
    fixture.componentRef.setInput('control', new FormControl('', Validators.required));
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('input').getAttribute('aria-invalid')).toBeNull();
    expect(fixture.nativeElement.querySelector('span')).toBeNull();
  });

  it('updates the message when the error changes', () => {
    const control = new FormControl('', [Validators.required, Validators.maxLength(3)]);
    expect((fixture.componentRef.setInput('control', control), component.errorMessage())).toBe('This field is required.');
    control.setValue('too long');
    expect(component.errorMessage()).toBe('Maximum 3 characters.');
  });
});
