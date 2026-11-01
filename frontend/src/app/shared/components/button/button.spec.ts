import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Button } from './button';

describe('Button', () => {
  let component: Button;
  let fixture: ComponentFixture<Button>;
  const button = (): HTMLButtonElement => fixture.nativeElement.querySelector('button');

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Button],
    }).compileComponents();

    fixture = TestBed.createComponent(Button);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('never submits a form unless asked to', () => {
    fixture.detectChanges();
    expect(button().type).toBe('button');
  });

  it('applies the variant class', () => {
    fixture.componentRef.setInput('variant', 'danger');
    fixture.detectChanges();
    expect(button().classList).toContain('btn-danger');
  });

  it('shows Saving... and blocks clicks while loading', () => {
    fixture.componentRef.setInput('text', 'Save');
    fixture.componentRef.setInput('loading', true);
    fixture.detectChanges();
    expect(button().disabled).toBe(true);
    expect(button().textContent).toContain('Saving...');
    expect(button().textContent).not.toContain('Save ');
    expect(button().getAttribute('aria-busy')).toBe('true');
  });

  it('names icon-only buttons for screen readers', () => {
    fixture.componentRef.setInput('variant', 'icon');
    fixture.componentRef.setInput('label', 'Edit player');
    fixture.detectChanges();
    expect(button().getAttribute('aria-label')).toBe('Edit player');
  });
});
