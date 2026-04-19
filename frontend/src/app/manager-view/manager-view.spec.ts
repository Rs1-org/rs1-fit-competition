import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManagerView } from './manager-view';

describe('ManagerView', () => {
  let component: ManagerView;
  let fixture: ComponentFixture<ManagerView>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManagerView],
    }).compileComponents();

    fixture = TestBed.createComponent(ManagerView);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
