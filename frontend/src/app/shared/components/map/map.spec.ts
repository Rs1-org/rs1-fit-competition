import { ComponentFixture, TestBed } from '@angular/core/testing';
import * as L from 'leaflet';

import { Map, pinIcon } from './map';

describe('Map', () => {
  let component: Map;
  let fixture: ComponentFixture<Map>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Map],
    }).compileComponents();

    fixture = TestBed.createComponent(Map);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('uses the themed pin, which styles.css animates, for every marker', () => {
    expect(L.Marker.prototype.options.icon).toBe(pinIcon);
    expect(pinIcon.options.className).toBe('map-pin');
    expect(String(pinIcon.options.html)).toContain('<svg');
  });
});
