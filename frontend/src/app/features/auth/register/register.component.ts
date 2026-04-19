import { Component, inject, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from 'src/app/core/services/auth.service';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule],
  templateUrl: './register.component.html',
  styleUrls: ['../auth-modal.component.css'],
})
export class RegisterComponent {

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);

  @Input() show = false;

  @Output() switchToLogin = new EventEmitter<void>();
  @Output() closeModal = new EventEmitter<void>();

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    phoneNumber: ['', Validators.required]
  });

  submit() {
    if (this.form.invalid) return;

    this.authService.register(this.form.getRawValue()).subscribe({
      next: () => this.switchToLogin.emit()
    });
  }

  close() {
    this.closeModal.emit();
  }

  goToLogin() {
    this.switchToLogin.emit();
  }
}