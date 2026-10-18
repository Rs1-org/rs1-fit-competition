import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { emptyPage } from '../../core/models/paged-result';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CityList } from './city-list';

describe('CityList', () => {
  let component: CityList;
  let fixture: ComponentFixture<CityList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CityList],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        provideNoopAnimations(),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CityList);
    component = fixture.componentInstance;
    fixture.detectChanges();
    const http = TestBed.inject(HttpTestingController);
    http
      .match((request) => request.method === 'GET')
      .forEach((request) => request.flush(emptyPage()));
    await fixture.whenStable();
    http.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
