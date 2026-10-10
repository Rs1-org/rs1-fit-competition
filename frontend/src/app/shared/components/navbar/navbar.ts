import { AfterViewInit, Component, ElementRef, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { ThemeToggle } from '../../../core/services/theme-service/theme-service';
import { ThemeService } from '../../../core/services/theme.service';
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
  imports: [ThemeToggle,CommonModule,RouterModule, AuthModalComponent],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar implements OnInit, AfterViewInit, OnDestroy {
  private host = inject(ElementRef<HTMLElement>);
  private navbarSize?: ResizeObserver;

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
  openManageCountries(event:MouseEvent) {
    event.stopPropagation();
    this.router.navigate(['/countries']);
    this.openManageTab = false;
  }
  openManageCities(event:MouseEvent) {
    event.stopPropagation();
    this.router.navigate(['/cities']);
    this.openManageTab = false;
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

  openStadiums() {
    this.router.navigate(['/stadium-list']);
  }

  openCities() {
    this.router.navigate(['/city-list']);
  }

  openCountries() {
    this.router.navigate(['/country-list']);
  }

  openPositions(event: MouseEvent) {
    event.stopPropagation();
    this.router.navigate(['/position-list']);
    this.openManageTab = false;
  }

  openSubPositions(event: MouseEvent) {
    event.stopPropagation();
    this.router.navigate(['/subposition-list']);
    this.openManageTab = false;
  }

  openFormationMarks(event: MouseEvent) {
    event.stopPropagation();
    this.router.navigate(['/formation-mark-list']);
    this.openManageTab = false;
  }

  openSelections(event: MouseEvent) {
    event.stopPropagation();
    this.router.navigate(['/selection-list']);
    this.openManageTab = false;
  }

  openAddSelection(event: MouseEvent) {
    event.stopPropagation();
    this.router.navigate(['/manage-selections']);
    this.openManageTab = false;
  }

  openFormations(event: MouseEvent) {
    event.stopPropagation();
    this.router.navigate(['/formation-list']);
    this.openManageTab = false;
  }

  openAddFormationMark(event: MouseEvent) {
    event.stopPropagation();
    this.router.navigate(['/manage-formation-marks']);
    this.openManageTab = false;
  }

  openAddFormation(event: MouseEvent) {
    event.stopPropagation();
    this.router.navigate(['/manage-formations']);
    this.openManageTab = false;
  }

  openGallery(event: MouseEvent) {
    event.stopPropagation();
    this.router.navigate(['/image-gallery']);
    this.openManageTab = false;
  }

  @ViewChild(AuthModalComponent)
  authModal!: AuthModalComponent;

  user: UserDto | null = null;
  canManagePersons = false;

  ngAfterViewInit(): void {
    const bar = this.host.nativeElement;
    const syncHeight = () => {
      document.documentElement.style.setProperty('--navbar-height', `${bar.offsetHeight}px`);
    };
    syncHeight();
    if (typeof ResizeObserver === 'undefined') return;
    this.navbarSize = new ResizeObserver(syncHeight);
    this.navbarSize.observe(bar);
  }

  ngOnDestroy(): void {
    this.navbarSize?.disconnect();
  }

  ngOnInit(): void {
    this.userStateService.user$.subscribe((userData) => {
      this.user = userData;
    });
    this.userStateService.roles$.subscribe(() => {
      this.canManagePersons = this.userStateService.canManagePersons();
    });
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
