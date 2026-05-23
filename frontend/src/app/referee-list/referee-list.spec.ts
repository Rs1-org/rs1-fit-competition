import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RefereeList } from './referee-list';

describe('RefereeList', () => {
  let component: RefereeList;
  let fixture: ComponentFixture<RefereeList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RefereeList],
    }).compileComponents();

    fixture = TestBed.createComponent(RefereeList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
