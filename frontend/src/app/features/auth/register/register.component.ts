import { Component, inject, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from 'src/app/core/services/auth.service';
import { RegisterErrorResponse } from 'src/app/core/models/auth.models';
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
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    phoneNumber: ['', Validators.required]
  });

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  submit(): void {
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
        this.errors = this.extractErrors(err.error as RegisterErrorResponse);
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
      messages.push('Password must be at least 6 characters.');
    }
    if (this.form.controls.firstName.errors) {
      messages.push('First name is required.');
    }
    if (this.form.controls.lastName.errors) {
      messages.push('Last name is required.');
    }
    if (this.form.controls.phoneNumber.errors) {
      messages.push('Phone number is required.');
    }

    return messages.length > 0 ? messages : ['Please fill in all required fields.'];
  }

  private extractErrors(errorBody: RegisterErrorResponse | null | undefined): string[] {
    if (!errorBody) {
      return ['Registration failed. Please try again.'];
    }

    if (errorBody.errors) {
      const messages = Object.values(errorBody.errors).flat();
      if (messages.length > 0) {
        return messages;
      }
    }

    return [errorBody.message || 'Registration failed. Please try again.'];
  }
}
