import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RefereeView } from './referee-view';

describe('RefereeView', () => {
  let component: RefereeView;
  let fixture: ComponentFixture<RefereeView>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RefereeView],
    }).compileComponents();

    fixture = TestBed.createComponent(RefereeView);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
