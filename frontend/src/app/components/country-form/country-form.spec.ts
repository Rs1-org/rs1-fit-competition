import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CountryForm } from './country-form';

describe('CountryForm', () => {
  let component: CountryForm;
  let fixture: ComponentFixture<CountryForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CountryForm],
    }).compileComponents();

    fixture = TestBed.createComponent(CountryForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
