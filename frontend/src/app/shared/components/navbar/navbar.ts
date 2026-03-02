import { Component } from '@angular/core';
import { ThemeService } from '../../../core/services/theme-service/theme-service';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-navbar',
  imports: [ThemeService,CommonModule,RouterModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
  constructor(public themeService: ThemeService, private router: Router) {}
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
}
