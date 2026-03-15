import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CityList } from './city-list';

describe('CityList', () => {
  let component: CityList;
  let fixture: ComponentFixture<CityList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CityList],
    }).compileComponents();

    fixture = TestBed.createComponent(CityList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
