import { Component, inject, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/core/services/auth.service';
import { AuthRedirectService } from 'src/app/core/services/auth-redirect.service';
import { UserStateService } from 'src/app/core/services/user-state.service';
@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['../auth-modal.component.css']
})
export class LoginComponent {
  constructor(private router: Router, private redirectService: AuthRedirectService, private userStateService: UserStateService) { }
  private fb = inject(FormBuilder); 
  private authService = inject(AuthService);

  errors = [""];

  @Input() show = false;
  @Output() switchToRegister = new EventEmitter<void>();
  @Output() closeModal = new EventEmitter<void>();

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  submit() {
  if (this.form.invalid) return;

  const loginData = this.form.getRawValue();

  this.authService.login(loginData).subscribe({
    next: () => {
      this.closeModal.emit();
      this.redirectService.redirect();
    },
    error: (err) => {
      this.errors = [err.error?.message || 'Login failed'];
    }
  });
}

  close() {
    this.closeModal.emit();
  }

  openRegister() {
    this.switchToRegister.emit();
  }
}