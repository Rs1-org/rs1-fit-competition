import { Component, inject } from '@angular/core';
import { LoginComponent } from '../login/login.component';
import { RegisterComponent } from '../register/register.component';
import { AuthModalService } from '../../../core/services/auth-modal.service';

@Component({
  imports: [LoginComponent, RegisterComponent],
  selector: 'app-auth-modal',
  templateUrl: './auth-modal.component.html',
  styleUrls: ['../auth-modal.component.css'],
})
export class AuthModalComponent {
  readonly modal = inject(AuthModalService);

  openLogin(): void {
    this.modal.openLogin();
  }

  openRegister(): void {
    this.modal.openRegister();
  }

  close(): void {
    this.modal.close();
  }
}