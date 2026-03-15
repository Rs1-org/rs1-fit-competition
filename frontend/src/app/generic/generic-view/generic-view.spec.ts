import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GenericView } from './generic-view';

describe('GenericView', () => {
  let component: GenericView;
  let fixture: ComponentFixture<GenericView>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GenericView],
    }).compileComponents();

    fixture = TestBed.createComponent(GenericView);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
