import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { UserStateService } from '../../core/services/user-state.service';
import { ProfileComponent } from './profile.component';

describe('ProfileComponent', () => {
  let fixture: ComponentFixture<ProfileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProfileComponent],
      providers: [
        provideRouter([]),
        {
          provide: UserStateService,
          useValue: {
            user$: of({ firstName: 'Edin', lastName: 'Smailagic', email: 'edin@competition.ba', phoneNumber: '', premiumUntil: null }),
            roles$: of(['User']),
            isPremium: () => false,
          },
        },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(ProfileComponent);
    fixture.detectChanges();
  });

  it('shows initials from the account name', () => {
    expect(fixture.componentInstance.initials()).toBe('ES');
    expect(fixture.nativeElement.textContent).toContain('Edit in settings');
  });
});
