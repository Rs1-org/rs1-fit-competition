import { Injectable, signal } from '@angular/core';

const THEME_KEY = 'theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly isDark = signal(false);

  constructor() {
    const saved = localStorage.getItem(THEME_KEY);
    this.isDark.set(saved === 'dark');
    this.apply();
  }

  toggle(): void {
    this.set(this.isDark() ? 'light' : 'dark');
  }

  set(theme: 'light' | 'dark'): void {
    this.isDark.set(theme === 'dark');
    localStorage.setItem(THEME_KEY, theme);
    this.apply();
  }

  private apply(): void {
    document.documentElement.setAttribute('data-theme', this.isDark() ? 'dark' : 'light');
  }
}
