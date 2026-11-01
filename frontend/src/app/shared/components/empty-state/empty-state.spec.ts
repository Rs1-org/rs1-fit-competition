import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { EmptyState } from './empty-state';

@Component({
  imports: [EmptyState],
  template: `<app-empty-state title="No matches in this round yet" message="Schedule one."><button>Schedule</button></app-empty-state>`,
})
class Host {}

describe('EmptyState', () => {
  it('shows the title, sentence and projected staff action', () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const element: HTMLElement = fixture.nativeElement;
    expect(element.querySelector('h3')?.textContent).toBe('No matches in this round yet');
    expect(element.querySelector('p')?.textContent).toBe('Schedule one.');
    expect(element.querySelector('button')?.textContent).toBe('Schedule');
  });

  it('leaves the sentence out when there is none', () => {
    const fixture = TestBed.createComponent(EmptyState);
    fixture.componentRef.setInput('title', 'Nothing here');
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('p')).toBeNull();
  });
});
