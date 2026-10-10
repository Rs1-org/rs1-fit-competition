import { Component, inject, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/core/services/auth.service';
import { AuthRedirectService } from 'src/app/core/services/auth-redirect.service';
import { UserStateService } from 'src/app/core/services/user-state.service';
import { popIn } from 'src/app/core/animations/motion';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['../auth-modal.component.css'],
  animations: [popIn],
})
export class LoginComponent {
  constructor(
    private router: Router,
    private redirectService: AuthRedirectService,
    private userStateService: UserStateService,
  ) {}

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);

  errors: string[] = [];
  isSubmitting = false;
  showPassword = false;

  @Input() show = false;
  @Output() switchToRegister = new EventEmitter<void>();
  @Output() closeModal = new EventEmitter<void>();

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  submit(): void {
    this.errors = [];

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.errors = this.getValidationErrors();
      return;
    }

    this.isSubmitting = true;
    const loginData = this.form.getRawValue();

    this.authService.login(loginData).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.closeModal.emit();
        this.redirectService.redirect();
      },
      error: (err) => {
        this.isSubmitting = false;
        this.errors = [err.error?.message || 'Login failed. Check your email and password.'];
      }
    });
  }

  close(): void {
    this.closeModal.emit();
  }

  openRegister(): void {
    this.switchToRegister.emit();
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
    if (this.form.controls.password.errors) {
      messages.push('Password is required.');
    }

    return messages;
  }
}
