import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FilterCard } from './filter-card';

describe('FilterCard', () => {
  let fixture: ComponentFixture<FilterCard>;
  let element: HTMLElement;

  beforeEach(() => {
    fixture = TestBed.createComponent(FilterCard);
    fixture.componentRef.setInput('chips', [{ key: 'season', label: 'Premijer liga BiH 2025/26' }]);
    fixture.detectChanges();
    element = fixture.nativeElement;
  });

  it('lists active chips and emits the removed key', () => {
    const removed = vi.fn();
    fixture.componentInstance.chipRemove.subscribe(removed);
    expect(element.querySelector('.chip')?.textContent).toContain('Premijer liga BiH 2025/26');
    element.querySelector<HTMLButtonElement>('.chip-remove')!.click();
    expect(removed).toHaveBeenCalledExactlyOnceWith('season');
  });

  it('hides the chip row when no filter is active', () => {
    fixture.componentRef.setInput('chips', []);
    fixture.detectChanges();
    expect(element.querySelector('.filter-chips')).toBeNull();
  });

  it('emits apply on submit without reloading the page', () => {
    const applied = vi.fn();
    fixture.componentInstance.applyFilters.subscribe(applied);
    const event = new Event('submit', { cancelable: true });
    element.querySelector('form')!.dispatchEvent(event);
    expect(applied).toHaveBeenCalledOnce();
    expect(event.defaultPrevented).toBe(true);
  });

  it('emits reset without submitting', () => {
    const reset = vi.fn(), applied = vi.fn();
    fixture.componentInstance.resetFilters.subscribe(reset);
    fixture.componentInstance.applyFilters.subscribe(applied);
    element.querySelector<HTMLButtonElement>('.btn-secondary')!.click();
    expect(reset).toHaveBeenCalledOnce();
    expect(applied).not.toHaveBeenCalled();
  });
});
