import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManagePlayers } from './manage-players';

describe('ManagePlayers', () => {
  let component: ManagePlayers;
  let fixture: ComponentFixture<ManagePlayers>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManagePlayers],
    }).compileComponents();

    fixture = TestBed.createComponent(ManagePlayers);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
