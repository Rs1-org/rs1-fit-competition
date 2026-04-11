import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GenericSearch } from './generic-search';

describe('GenericSearch', () => {
  let component: GenericSearch;
  let fixture: ComponentFixture<GenericSearch>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GenericSearch],
    }).compileComponents();

    fixture = TestBed.createComponent(GenericSearch);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
