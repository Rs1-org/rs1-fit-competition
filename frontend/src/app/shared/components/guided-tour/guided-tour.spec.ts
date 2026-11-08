import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TourService } from '../../../core/services/tour.service';
import { GuidedTour } from './guided-tour';

describe('GuidedTour', () => {
  let fixture: ComponentFixture<GuidedTour>;
  let tour: TourService;
  let target: HTMLElement;

  const text = () => fixture.nativeElement.textContent as string;
  const click = (label: string) => {
    const button = [...fixture.nativeElement.querySelectorAll('button')].find(b => b.textContent.trim() === label) as HTMLButtonElement;
    button.click();
    fixture.detectChanges();
  };

  beforeEach(async () => {
    localStorage.clear();
    target = document.createElement('a');
    target.setAttribute('data-tour', 'live');
    target.textContent = 'Live';
    document.body.appendChild(target);

    await TestBed.configureTestingModule({ imports: [GuidedTour] }).compileComponents();
    fixture = TestBed.createComponent(GuidedTour);
    tour = TestBed.inject(TourService);
    fixture.detectChanges();
  });

  afterEach(() => {
    target.remove();
    localStorage.clear();
  });

  it('renders nothing until the tour starts', () => {
    expect(fixture.nativeElement.querySelector('[role="dialog"]')).toBeNull();
  });

  it('shows the current step with its position', () => {
    tour.startIfFirstVisit();
    fixture.detectChanges();
    expect(text()).toContain('1 of 5');
    expect(text()).toContain('Live');
    expect(text()).not.toContain('Back');
  });

  it('moves forward and back with the buttons', () => {
    tour.startIfFirstVisit();
    fixture.detectChanges();
    click('Next');
    expect(text()).toContain('2 of 5');
    click('Back');
    expect(text()).toContain('1 of 5');
  });

  it('closes when skipped and on the last step shows Finish', () => {
    tour.startIfFirstVisit();
    fixture.detectChanges();
    for (let i = 0; i < 4; i++) click('Next');
    expect(text()).toContain('Finish');
    click('Skip tour');
    expect(fixture.nativeElement.querySelector('[role="dialog"]')).toBeNull();
    expect(localStorage.getItem('tour_status')).toBe('done');
  });

  it('skips on Escape', () => {
    tour.startIfFirstVisit();
    fixture.detectChanges();
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
    fixture.detectChanges();
    expect(tour.active()).toBe(false);
  });

  it('keeps Tab inside the card', () => {
    tour.startIfFirstVisit();
    tour.next(); // Skip tour, Back, Next
    fixture.detectChanges();
    const buttons = [...fixture.nativeElement.querySelectorAll('.tour-card button')] as HTMLButtonElement[];
    buttons[buttons.length - 1].focus();
    const tab = new KeyboardEvent('keydown', { key: 'Tab', cancelable: true });
    document.dispatchEvent(tab);
    expect(tab.defaultPrevented).toBe(true);
    expect(document.activeElement).toBe(buttons[0]);

    const shiftTab = new KeyboardEvent('keydown', { key: 'Tab', shiftKey: true, cancelable: true });
    document.dispatchEvent(shiftTab);
    expect(document.activeElement).toBe(buttons[buttons.length - 1]);
  });

  it('gives focus back to the element that had it before the tour', () => {
    const before = document.createElement('button');
    document.body.appendChild(before);
    before.focus();
    tour.startIfFirstVisit();
    fixture.detectChanges();
    tour.skip();
    fixture.detectChanges();
    expect(document.activeElement).toBe(before);
    before.remove();
  });

  it('centres the card when the target is not on the page', () => {
    tour.startIfFirstVisit();
    tour.next(); // "matches" has no element in this test
    fixture.detectChanges();
    fixture.componentInstance.place();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.tour-card').classList).toContain('centered');
  });
});
