import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { Component } from '@angular/core';
import { SectionTabs } from './section-tabs';

@Component({ template: '' })
class Blank {}

describe('SectionTabs', () => {
  it('renders a link per tab and marks the current one', async () => {
    TestBed.configureTestingModule({ providers: [provideRouter([{ path: 'a', component: Blank }, { path: 'b', component: Blank }])] });
    await TestBed.inject(Router).navigateByUrl('/b');
    const fixture = TestBed.createComponent(SectionTabs);
    fixture.componentRef.setInput('label', 'Sections');
    fixture.componentRef.setInput('tabs', [{ label: 'First', route: '/a' }, { label: 'Second', route: '/b', locked: true }]);
    fixture.detectChanges();
    await fixture.whenStable();
    const links: HTMLAnchorElement[] = Array.from(fixture.nativeElement.querySelectorAll('a'));
    expect(links.map(link => link.textContent?.trim())).toEqual(['First', 'Second']);
    expect(links[0].getAttribute('aria-current')).toBeNull();
    expect(links[1].getAttribute('aria-current')).toBe('page');
    expect(links[1].querySelector('app-icon')).toBeTruthy();
  });
});
