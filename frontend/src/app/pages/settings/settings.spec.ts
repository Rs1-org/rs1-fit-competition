import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Settings } from './settings';

describe('Settings', () => {
  let fixture: ComponentFixture<Settings>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Settings],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    fixture = TestBed.createComponent(Settings);
    fixture.detectChanges();
    TestBed.inject(HttpTestingController).match(() => true).forEach((request) => request.flush({
      firstName: 'Edin', lastName: 'Smailagic', email: 'edin@competition.ba',
      theme: 'light', language: 'en', dateFormat: 'dd.MM.yyyy',
    }));
    await fixture.whenStable();
  });

  it('shows the settings sections', () => {
    expect(fixture.nativeElement.textContent).toContain('Premium is not active');
    expect(fixture.nativeElement.textContent).toContain('Appearance');
  });
});
