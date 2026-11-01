import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Icon } from './icon';

describe('Icon', () => {
  let fixture: ComponentFixture<Icon>;
  const svg = () => fixture.nativeElement.querySelector('svg') as SVGElement;

  beforeEach(() => {
    fixture = TestBed.createComponent(Icon);
    fixture.componentRef.setInput('name', 'home');
  });

  it('draws the named icon as a 2px round line', () => {
    fixture.detectChanges();
    expect(svg().querySelectorAll('path').length).toBeGreaterThan(0);
    expect(svg().getAttribute('stroke-width')).toBe('2');
    expect(svg().getAttribute('stroke-linecap')).toBe('round');
    expect(svg().getAttribute('stroke')).toBe('currentColor');
  });

  it('is hidden from screen readers without a label', () => {
    fixture.detectChanges();
    expect(svg().getAttribute('aria-hidden')).toBe('true');
    expect(svg().getAttribute('role')).toBeNull();
  });

  it('is announced when it has a label', () => {
    fixture.componentRef.setInput('label', 'Delete');
    fixture.detectChanges();
    expect(svg().getAttribute('role')).toBe('img');
    expect(svg().getAttribute('aria-label')).toBe('Delete');
    expect(svg().getAttribute('aria-hidden')).toBeNull();
  });
});
