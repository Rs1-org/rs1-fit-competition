import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { UserDto } from '../models/auth.models';
import { UserStateService } from './user-state.service';
import { ThemeService } from './theme.service';
import { LanguageService, AppLanguage } from './language.service';
import { TokenService } from './token.service';
import { REGIONAL_DATE_FORMAT, activeDateFormat } from '../../shared/pipes/regional-format.pipes';

export interface SettingsDto {
  firstName: string;
  lastName: string;
  email: string;
  theme: 'light' | 'dark';
  language: AppLanguage;
  dateFormat: string;
}

export interface ProfileUpdate {
  firstName: string;
  lastName: string;
  email: string;
}

export interface PasswordChange {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface PreferenceUpdate {
  theme: 'light' | 'dark';
  language: AppLanguage;
  dateFormat: string;
}

@Injectable({ providedIn: 'root' })
export class SettingsService {
  private http = inject(HttpClient);
  private userState = inject(UserStateService);
  private theme = inject(ThemeService);
  private language = inject(LanguageService);
  private tokens = inject(TokenService);
  private readonly api = `${environment.apiUrl}/api/settings`;

  constructor() {
    const saved = localStorage.getItem('dateFormat');
    if (saved === REGIONAL_DATE_FORMAT || saved === 'yyyy-MM-dd') activeDateFormat.set(saved);
    if (this.tokens.getAccessToken()) this.load().subscribe({ error: () => undefined });
  }

  load(): Observable<SettingsDto> {
    return this.http.get<SettingsDto>(this.api).pipe(tap((settings) => this.apply(settings)));
  }

  updateProfile(body: ProfileUpdate): Observable<UserDto> {
    return this.http.put<UserDto>(`${this.api}/profile`, body).pipe(
      tap((user) => this.userState.setUser(user, this.userState.getRoles())),
    );
  }

  changePassword(body: PasswordChange): Observable<void> {
    return this.http.put<void>(`${this.api}/password`, body);
  }

  updatePreferences(body: PreferenceUpdate): Observable<PreferenceUpdate> {
    return this.http.put<PreferenceUpdate>(`${this.api}/preferences`, body).pipe(
      tap((preference) => this.apply({ ...preference, firstName: '', lastName: '', email: '' })),
    );
  }

  private apply(settings: SettingsDto): void {
    if (settings.theme === 'light' || settings.theme === 'dark') this.theme.set(settings.theme);
    if (settings.language === 'bs' || settings.language === 'en') this.language.set(settings.language);
    if (settings.dateFormat === REGIONAL_DATE_FORMAT || settings.dateFormat === 'yyyy-MM-dd') {
      activeDateFormat.set(settings.dateFormat);
      localStorage.setItem('dateFormat', settings.dateFormat);
    }
  }
}
