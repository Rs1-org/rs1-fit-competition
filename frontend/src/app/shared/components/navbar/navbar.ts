import { Component } from '@angular/core';
import { ThemeService } from '../../../core/services/theme-service/theme-service';

@Component({
  selector: 'app-navbar',
  imports: [ThemeService],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {}
