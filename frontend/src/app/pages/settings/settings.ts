import { Component, inject, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { SettingsService } from '../../core/services/settings.service';

function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const { newPassword, confirmPassword } = group.value;
  return newPassword && confirmPassword && newPassword !== confirmPassword ? { mismatch: true } : null;
}

@Component({
  selector: 'app-settings',
  imports: [ReactiveFormsModule],
  templateUrl: './settings.html',
  styleUrl: './settings.css',
})
export class Settings implements OnInit {
  private settings = inject(SettingsService);
  private fb = inject(FormBuilder);

  profileMessage = '';
  profileError = '';
  passwordMessage = '';
  passwordError = '';
  preferenceMessage = '';
  preferenceError = '';

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
        this.profileError = 'Could not load settings.';
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
    this.settings.updateProfile(this.profileForm.getRawValue()).subscribe({
      next: () => {
        this.profileSnapshot = this.profileForm.getRawValue();
        this.profileMessage = 'Profile saved.';
      },
      error: (err) => {
        this.profileError = err?.error?.errors?.[0]?.errorMessage || err?.error?.message || 'Could not save the profile.';
      },
    });
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
    this.settings.changePassword(this.passwordForm.getRawValue()).subscribe({
      next: () => {
        this.passwordForm.reset();
        this.passwordMessage = 'Password changed.';
      },
      error: (err) => {
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
    this.settings.updatePreferences(this.preferenceForm.getRawValue()).subscribe({
      next: () => {
        this.preferenceSnapshot = this.preferenceForm.getRawValue();
        this.preferenceMessage = 'Preferences saved.';
      },
      error: (err) => {
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
