import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CityForm } from './city-form';

describe('CityForm', () => {
  let component: CityForm;
  let fixture: ComponentFixture<CityForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CityForm],
    }).compileComponents();

    fixture = TestBed.createComponent(CityForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
