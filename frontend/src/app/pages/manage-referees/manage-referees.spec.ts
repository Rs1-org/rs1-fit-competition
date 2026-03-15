import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageReferees } from './manage-referees';

describe('ManageReferees', () => {
  let component: ManageReferees;
  let fixture: ComponentFixture<ManageReferees>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageReferees],
    }).compileComponents();

    fixture = TestBed.createComponent(ManageReferees);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
