import { Component, ViewChild } from '@angular/core';
import { ThemeService } from '../../../core/services/theme-service/theme-service';
import { AuthModalComponent } from 'src/app/features/auth/auth-modal/auth-modal.component';
import { TokenService } from 'src/app/core/services/token.service';
import { Router } from '@angular/router';
import { AuthRedirectService } from 'src/app/core/services/auth-redirect.service';
import { UserStateService } from 'src/app/core/services/user-state.service';
import { UserDto } from 'src/app/core/models/auth.models';
import { AuthService } from 'src/app/core/services/auth.service';

@Component({
  selector: 'app-navbar',
  imports: [ThemeService, AuthModalComponent],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {

  constructor(private tokenService:TokenService, private router:Router, private redirectSerive:AuthRedirectService, private userStateService:UserStateService, private authService:AuthService){}

  @ViewChild(AuthModalComponent)
  authModal!: AuthModalComponent;

  user: UserDto | null = null;
  token:string|null = null;

  ngOnInit(){
    this.token = localStorage.getItem('refreshToken');
        this.userStateService.user$.subscribe((userData)=>{
            this.user = userData
        })
    }

  openAuth() {
    this.authModal.openLogin();
  }

  logout(){
    this.authService.logout();
  }

  openProfile(){
  if(!this.tokenService.getAccessToken()){
    this.authModal.openLogin();
    this.redirectSerive.setRedirectUrl('/profile');
    return;
  }

  if(!this.user){
    this.authModal.openLogin();
    return;
  }

  this.router.navigate(['profile']);
}

}
