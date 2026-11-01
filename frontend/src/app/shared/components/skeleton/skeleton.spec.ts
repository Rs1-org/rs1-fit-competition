import { TestBed } from '@angular/core/testing';
import { Skeleton } from './skeleton';

describe('Skeleton', () => {
  it('renders the requested rows with an avatar and a loading label', () => {
    const fixture = TestBed.createComponent(Skeleton);
    fixture.componentRef.setInput('rows', 5);
    fixture.componentRef.setInput('avatar', true);
    fixture.detectChanges();
    const element: HTMLElement = fixture.nativeElement;
    expect(element.querySelectorAll('.skeleton-row').length).toBe(5);
    expect(element.querySelectorAll('.skeleton-avatar').length).toBe(5);
    expect(element.querySelector('[role="status"]')?.getAttribute('aria-label')).toBe('Loading');
  });
});
