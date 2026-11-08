import { Component, inject, OnInit } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { SettingsService } from '../../core/services/settings.service';
import { UserStateService } from '../../core/services/user-state.service';
import { ToastService } from '../../core/services/toast.service';
import { SectionCard } from '../../shared/components/section-card/section-card';
import { Button } from '../../shared/components/button/button';
import { InputBox } from '../../shared/components/input-box/input-box';
import { ErrorBanner } from '../../shared/components/error-banner/error-banner';
import { AppDatePipe } from '../../shared/pipes/regional-format.pipes';
import { Router } from '@angular/router';
import { TourService } from '../../core/services/tour.service';

function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const { newPassword, confirmPassword } = group.value;
  return newPassword && confirmPassword && newPassword !== confirmPassword ? { mismatch: true } : null;
}

@Component({
  selector: 'app-settings',
  imports: [ReactiveFormsModule, SectionCard, Button, InputBox, ErrorBanner, AppDatePipe],
  templateUrl: './settings.html',
  styleUrls: ['../staff-form.css', './settings.css'],
})
export class Settings implements OnInit {
  private settings = inject(SettingsService);
  private userState = inject(UserStateService);
  private toast = inject(ToastService);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private tour = inject(TourService);
  readonly account = toSignal(this.userState.user$, { initialValue: null });
  readonly premium = this.userState.isPremium;

  profileMessage = '';
  profileError = '';
  passwordMessage = '';
  passwordError = '';
  preferenceMessage = '';
  preferenceError = '';
  loadError = '';
  savingProfile = false;
  savingPassword = false;
  savingPreferences = false;

  profileForm = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
  });

  passwordForm = this.fb.nonNullable.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [
      Validators.required,
      Validators.minLength(6),
      Validators.pattern(/[A-Z]/),
      Validators.pattern(/[a-z]/),
      Validators.pattern(/[0-9]/),
      Validators.pattern(/[^a-zA-Z0-9]/),
    ]],
    confirmPassword: ['', Validators.required],
  }, { validators: passwordsMatch });

  preferenceForm = this.fb.nonNullable.group({
    theme: this.fb.nonNullable.control<'light' | 'dark'>('light', Validators.required),
    language: this.fb.nonNullable.control<'bs' | 'en'>('bs', Validators.required),
    dateFormat: ['dd.MM.yyyy', Validators.required],
  });

  private profileSnapshot = this.profileForm.getRawValue();
  private preferenceSnapshot = this.preferenceForm.getRawValue();

  ngOnInit(): void {
    this.settings.load().subscribe({
      next: (settings) => {
        this.profileForm.setValue({
          firstName: settings.firstName,
          lastName: settings.lastName,
          email: settings.email,
        });
        this.preferenceForm.setValue({
          theme: settings.theme,
          language: settings.language,
          dateFormat: settings.dateFormat,
        });
        this.profileSnapshot = this.profileForm.getRawValue();
        this.preferenceSnapshot = this.preferenceForm.getRawValue();
      },
      error: () => {
        this.loadError = 'Could not load settings.';
      },
    });
  }

  fieldError(form: 'profile' | 'password', name: string): string | null {
    const control = form === 'profile'
      ? this.profileForm.get(name)
      : this.passwordForm.get(name);
    if (!control || !control.touched || control.valid) return null;
    if (control.hasError('required')) return 'This field is required.';
    if (control.hasError('email')) return 'Enter a valid email address.';
    if (control.hasError('maxlength')) return 'The value is too long.';
    if (control.hasError('minlength') || control.hasError('pattern')) {
      return 'Use at least 6 characters with an uppercase letter, a lowercase letter, a digit and a symbol.';
    }
    return 'Invalid value.';
  }

  saveProfile(): void {
    this.profileMessage = '';
    this.profileError = '';
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }
    this.savingProfile = true;
    this.settings.updateProfile(this.profileForm.getRawValue()).subscribe({
      next: () => {
        this.savingProfile = false;
        this.profileSnapshot = this.profileForm.getRawValue();
        this.profileMessage = 'Profile saved.';
        this.toast.success('Profile saved.');
      },
      error: (err) => {
        this.savingProfile = false;
        this.profileError = err?.error?.errors?.[0]?.errorMessage || err?.error?.message || 'Could not save the profile.';
      },
    });
  }

  /** Goes to the home page and runs the tour again from the first step. */
  restartTour(): void {
    this.router.navigateByUrl('/').then(() => setTimeout(() => this.tour.restart(), 300));
  }

  cancelProfile(): void {
    this.profileForm.reset(this.profileSnapshot);
    this.profileMessage = '';
    this.profileError = '';
  }

  savePassword(): void {
    this.passwordMessage = '';
    this.passwordError = '';
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }
    this.savingPassword = true;
    this.settings.changePassword(this.passwordForm.getRawValue()).subscribe({
      next: () => {
        this.savingPassword = false;
        this.passwordForm.reset();
        this.passwordMessage = 'Password changed.';
        this.toast.success('Password changed.');
      },
      error: (err) => {
        this.savingPassword = false;
        this.passwordError = err?.error?.errors?.[0]?.errorMessage || err?.error?.message || 'Could not change the password.';
      },
    });
  }

  cancelPassword(): void {
    this.passwordForm.reset();
    this.passwordMessage = '';
    this.passwordError = '';
  }

  savePreferences(): void {
    this.preferenceMessage = '';
    this.preferenceError = '';
    if (this.preferenceForm.invalid) {
      this.preferenceForm.markAllAsTouched();
      return;
    }
    this.savingPreferences = true;
    this.settings.updatePreferences(this.preferenceForm.getRawValue()).subscribe({
      next: () => {
        this.savingPreferences = false;
        this.preferenceSnapshot = this.preferenceForm.getRawValue();
        this.preferenceMessage = 'Preferences saved.';
        this.toast.success('Preferences saved.');
      },
      error: (err) => {
        this.savingPreferences = false;
        this.preferenceError = err?.error?.errors?.[0]?.errorMessage || err?.error?.message || 'Could not save preferences.';
      },
    });
  }

  cancelPreferences(): void {
    this.preferenceForm.reset(this.preferenceSnapshot);
    this.preferenceMessage = '';
    this.preferenceError = '';
  }
}
