import { TestBed } from '@angular/core/testing';
import { ErrorBanner } from './error-banner';

describe('ErrorBanner', () => {
  it('shows the message and emits retry', () => {
    const fixture = TestBed.createComponent(ErrorBanner);
    fixture.componentRef.setInput('message', 'Could not load statistics.');
    fixture.detectChanges();
    const retry = vi.fn();
    fixture.componentInstance.retry.subscribe(retry);
    const element: HTMLElement = fixture.nativeElement;
    expect(element.querySelector('[role="alert"]')?.textContent).toContain('Could not load statistics.');
    element.querySelector('button')!.click();
    expect(retry).toHaveBeenCalledOnce();
  });
});
