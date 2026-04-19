import { Component, ViewChild } from '@angular/core';
import { ThemeService } from '../../../core/services/theme-service/theme-service';
import { AuthModalComponent } from 'src/app/features/auth/auth-modal/auth-modal.component';
import { TokenService } from 'src/app/core/services/token.service';
import { Router } from '@angular/router';
import { AuthRedirectService } from 'src/app/core/services/auth-redirect.service';
import { UserStateService } from 'src/app/core/services/user-state.service';
import { UserDto } from 'src/app/core/models/auth.models';
import { AuthService } from 'src/app/core/services/auth.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-navbar',
  imports: [ThemeService,CommonModule,RouterModule, AuthModalComponent],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
  constructor(private tokenService:TokenService, private router:Router, private redirectSerive:AuthRedirectService, private userStateService:UserStateService, private authService:AuthService,public themeService: ThemeService) {}
  openManageTab = false;
  openManage() {
    this.openManageTab = !this.openManageTab;

  }
  openManagePlayers(event:MouseEvent) {
    event.stopPropagation();
    this.router.navigate(['/manage-players']);
    this.openManageTab = !this.openManageTab;
  }
  openManageReferees(event:MouseEvent) {
    event.stopPropagation();
    this.router.navigate(['/manage-referees']);
    this.openManageTab = !this.openManageTab;
  }
  openManageManagers(event:MouseEvent) {
    event.stopPropagation();
    this.router.navigate(['/manage-managers']);
    this.openManageTab = !this.openManageTab;
  }
  openListPlayers() {
    this.router.navigate(['/player-list']);
  }
  openListReferees() {
    this.router.navigate(['/referee-list']);
  }
  openListManagers() {
    this.router.navigate(['/manager-list']);
  }

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
