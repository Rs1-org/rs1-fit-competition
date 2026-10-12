import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { StadiumDetail } from './stadium-detail';
import { StadiumService } from '../../core/services/stadium.service';
import { StadiumDetailsDto } from '../../core/models/stadium';

describe('StadiumDetail', () => {
  const kosevo: StadiumDetailsDto = {
    id: 's1',
    name: 'Asim Ferhatović Hase',
    capacity: 34500,
    dateOfConstruction: '1947-01-01',
    slug: 'kosevo',
    pictureUrl: '',
    cityId: 'c1',
    cityName: 'Sarajevo',
    latitude: 43.8686,
    longitude: 18.4093,
    geoDescription: '',
    cityLatitude: 43.8563,
    cityLongitude: 18.4131
  };

  async function setup(stadium: StadiumDetailsDto): Promise<ComponentFixture<StadiumDetail>> {
    await TestBed.configureTestingModule({
      imports: [StadiumDetail],
      providers: [
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { paramMap: of(convertToParamMap({ slug: stadium.slug })) } },
        { provide: StadiumService, useValue: { getBySlug: () => of(stadium) } }
      ],
    })
      // The Leaflet map is covered by its own spec; keep it out of this one.
      .overrideComponent(StadiumDetail, { set: { imports: [DecimalPipe, DatePipe], schemas: [CUSTOM_ELEMENTS_SCHEMA] } })
      .compileComponents();

    const fixture = TestBed.createComponent(StadiumDetail);
    fixture.detectChanges();
    await fixture.whenStable();
    return fixture;
  }

  it('shows the distance to the city centre in km with 1 decimal', async () => {
    const fixture = await setup(kosevo);

    expect(fixture.componentInstance.distanceToCityKm()).toBe(1.4);
    expect(fixture.nativeElement.querySelector('.distance').textContent).toContain('1.4 km');
  });

  it('hides the distance when the city has no coordinates', async () => {
    const fixture = await setup({ ...kosevo, cityLatitude: null, cityLongitude: null });

    expect(fixture.componentInstance.distanceToCityKm()).toBeNull();
    expect(fixture.nativeElement.querySelector('.distance')).toBeNull();
  });
});
