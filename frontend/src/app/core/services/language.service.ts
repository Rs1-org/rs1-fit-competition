import { Injectable, signal } from '@angular/core';

export type AppLanguage = 'bs' | 'en';

const LANGUAGE_KEY = 'language';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  readonly language = signal<AppLanguage>('bs');

  constructor() {
    const saved = localStorage.getItem(LANGUAGE_KEY);
    if (saved === 'en' || saved === 'bs') this.language.set(saved);
    document.documentElement.lang = this.language();
  }

  set(language: AppLanguage): void {
    this.language.set(language);
    localStorage.setItem(LANGUAGE_KEY, language);
    document.documentElement.lang = language;
  }
}
