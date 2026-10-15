import { AfterViewInit, Component, ElementRef, HostListener, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { ThemeService } from '../../../core/services/theme.service';
import { AppLanguage, LanguageService } from '../../../core/services/language.service';
import { AuthModalComponent } from 'src/app/features/auth/auth-modal/auth-modal.component';
import { Modal } from '../modal/modal';
import { TokenService } from 'src/app/core/services/token.service';
import { Router } from '@angular/router';
import { AuthRedirectService } from 'src/app/core/services/auth-redirect.service';
import { UserStateService } from 'src/app/core/services/user-state.service';
import { UserDto } from 'src/app/core/models/auth.models';
import { AuthService } from 'src/app/core/services/auth.service';
import { CommonModule } from '@angular/common';
import { HasRoleDirective } from '../../directives/has-role.directive';
import { popIn } from '../../../core/animations/motion';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-navbar',
  imports: [CommonModule, RouterModule, AuthModalComponent, Modal, HasRoleDirective],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
  animations: [popIn],
})
export class Navbar implements OnInit, AfterViewInit, OnDestroy {
  private host = inject(ElementRef<HTMLElement>);
  private navbarSize?: ResizeObserver;

  constructor(private tokenService:TokenService, private router:Router, private redirectSerive:AuthRedirectService, private userStateService:UserStateService, private authService:AuthService, public themeService: ThemeService, public language: LanguageService) {}
  openManageTab = false;
  accountOpen = false;
  profileOpen = false;
  settingsOpen = false;
  roles: string[] = [];
  openManage() {
    this.accountOpen = false;
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

  openCompetitions() {
    this.router.navigate(['/competitions']);
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
    this.userStateService.roles$.subscribe((roles) => {
      this.roles = roles;
      this.canManagePersons = this.userStateService.canManagePersons();
    });
  }

  @HostListener('document:click', ['$event'])
  closeOnOutsideClick(event: MouseEvent): void {
    if (!this.host.nativeElement.contains(event.target as Node)) {
      this.accountOpen = false;
      this.openManageTab = false;
    }
  }

  toggleAccount(event: MouseEvent): void {
    event.stopPropagation();
    if (!this.tokenService.getAccessToken() || !this.user) {
      this.authModal.openLogin();
      this.redirectSerive.setRedirectUrl('/profile');
      return;
    }
    this.openManageTab = false;
    this.accountOpen = !this.accountOpen;
  }

  openProfilePopup(event: MouseEvent): void {
    event.stopPropagation();
    this.accountOpen = false;
    this.settingsOpen = false;
    this.profileOpen = true;
  }

  openSettingsPopup(event: MouseEvent): void {
    event.stopPropagation();
    this.accountOpen = false;
    this.profileOpen = false;
    this.settingsOpen = false;
    this.router.navigate(['/settings']);
  }

  setLanguage(language: AppLanguage, event: MouseEvent): void {
    event.stopPropagation();
    this.language.set(language);
  }

  setTheme(theme: 'light' | 'dark', event: MouseEvent): void {
    event.stopPropagation();
    this.themeService.set(theme);
  }

  openAuth() {
    this.authModal.openLogin();
  }

  logout(event?: MouseEvent): void {
    event?.stopPropagation();
    this.accountOpen = false;
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
