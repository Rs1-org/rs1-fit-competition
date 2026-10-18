import { Component, inject, Input, Output, EventEmitter } from '@angular/core';
import { AbstractControl, FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from 'src/app/core/services/auth.service';
import { apiErrorMessage } from 'src/app/core/utils/api-error';
import { popIn } from 'src/app/core/animations/motion';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule],
  templateUrl: './register.component.html',
  styleUrls: ['../auth-modal.component.css'],
  animations: [popIn],
})
export class RegisterComponent {

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);

  @Input() show = false;

  @Output() switchToLogin = new EventEmitter<void>();
  @Output() closeModal = new EventEmitter<void>();

  errors: string[] = [];
  successMessage = '';
  isSubmitting = false;
  showPassword = false;

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(128),
      Validators.pattern(/^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[^a-zA-Z0-9])[\s\S]*$/)]],
    confirmPassword: ['', Validators.required],
    firstName: ['', [Validators.required, Validators.maxLength(100), Validators.pattern(/\S/)]],
    lastName: ['', [Validators.required, Validators.maxLength(100), Validators.pattern(/\S/)]],
    phoneNumber: ['', [Validators.required, Validators.maxLength(30), Validators.pattern(/\S/)]]
  }, { validators: (group: AbstractControl) =>
    group.get('password')?.value === group.get('confirmPassword')?.value ? null : { passwordMismatch: true } });

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  submit(): void {
    if (this.isSubmitting) return;
    this.errors = [];
    this.successMessage = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.errors = this.getValidationErrors();
      return;
    }

    this.isSubmitting = true;

    this.authService.register(this.form.getRawValue()).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.successMessage = 'Account created successfully. You can log in now.';
        setTimeout(() => this.switchToLogin.emit(), 1200);
      },
      error: (err) => {
        this.isSubmitting = false;
        this.errors = [apiErrorMessage(err, 'Registration failed. Please try again.')];
      }
    });
  }

  close(): void {
    this.closeModal.emit();
  }

  goToLogin(): void {
    this.switchToLogin.emit();
  }

  isInvalid(controlName: keyof typeof this.form.controls): boolean {
    const control = this.form.controls[controlName];
    return control.invalid && control.touched;
  }

  private getValidationErrors(): string[] {
    const messages: string[] = [];

    if (this.form.controls.email.errors) {
      messages.push('Enter a valid email address.');
    }
    if (this.form.controls.password.errors?.['required']) {
      messages.push('Password is required.');
    } else if (this.form.controls.password.errors?.['minlength']) {
      messages.push('Password must be at least 8 characters.');
    } else if (this.form.controls.password.errors) {
      messages.push('Password must be 8–128 characters and include uppercase, lowercase, a digit and a special character.');
    }
    if (this.form.controls.confirmPassword.errors || this.form.hasError('passwordMismatch')) {
      messages.push('Confirm your password. Both passwords must match.');
    }
    if (this.form.controls.firstName.errors) {
      messages.push('First name is required and must be at most 100 characters.');
    }
    if (this.form.controls.lastName.errors) {
      messages.push('Last name is required and must be at most 100 characters.');
    }
    if (this.form.controls.phoneNumber.errors) {
      messages.push('Phone number is required and must be at most 30 characters.');
    }

    return messages.length > 0 ? messages : ['Please fill in all required fields.'];
  }

}
