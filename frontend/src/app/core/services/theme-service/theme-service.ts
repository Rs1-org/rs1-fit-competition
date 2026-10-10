import { Component, inject } from '@angular/core';
import { ThemeService } from '../theme.service';

@Component({
  selector: 'app-theme-service',
  imports: [],
  templateUrl: './theme-service.html',
  styleUrl: './theme-service.css',
})
export class ThemeToggle {
  theme = inject(ThemeService);
}
