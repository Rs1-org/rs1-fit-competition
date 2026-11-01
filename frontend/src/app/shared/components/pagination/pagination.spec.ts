import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Pagination } from './pagination';

describe('Pagination', () => {
  let fixture: ComponentFixture<Pagination>;
  let component: Pagination;

  beforeEach(() => {
    fixture = TestBed.createComponent(Pagination);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('totalItems', 134);
    fixture.componentRef.setInput('currentPage', 2);
    fixture.detectChanges();
  });

  it('shows the range of the current page', () => {
    expect(fixture.nativeElement.querySelector('.pagination-info').textContent.replace(/\s+/g, ' ').trim()).toBe('Showing 11–20 of 134');
  });

  it('shortens long page lists', () => {
    expect(component.pages()).toEqual([1, 2, 3, '...', 14]);
  });

  it('emits the next page and ignores the current one', () => {
    const changed = vi.fn();
    component.pageChange.subscribe(changed);
    component.next();
    component.goTo(2);
    expect(changed).toHaveBeenCalledExactlyOnceWith(3);
  });
});
