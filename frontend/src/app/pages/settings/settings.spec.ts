import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { Settings } from './settings';
import { TourService } from '../../core/services/tour.service';

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

  it('restarts the guided tour from Settings', async () => {
    const tour = TestBed.inject(TourService);
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    const restart = vi.spyOn(tour, 'restart');
    vi.useFakeTimers();

    const button = [...fixture.nativeElement.querySelectorAll('button')]
      .find((b: HTMLButtonElement) => b.textContent?.trim() === 'Restart tour') as HTMLButtonElement;
    button.click();
    await Promise.resolve();
    vi.runAllTimers();
    vi.useRealTimers();

    expect(router.navigateByUrl).toHaveBeenCalledWith('/');
    expect(restart).toHaveBeenCalled();
  });
});
