import { Component } from '@angular/core';
import { LoginComponent } from '../login/login.component';
import { RegisterComponent } from '../register/register.component';

@Component({
  imports: [LoginComponent,RegisterComponent],
  selector: 'app-auth-modal',
  templateUrl: './auth-modal.component.html',
  styleUrls: ['../auth-modal.component.css'],
})
export class AuthModalComponent {

  showLogin = false;
  showRegister = false;

  redirectAfterLogin: string | null = null;

  openLogin(route?: string){
    this.showLogin = !this.showLogin;
    this.showRegister = false;
    this.redirectAfterLogin = route ?? null;
  }

  openRegister(){
    this.showLogin = false;
    this.showRegister = true;
  }

  close(){
    this.showLogin = false;
    this.showRegister = false;
  }

}