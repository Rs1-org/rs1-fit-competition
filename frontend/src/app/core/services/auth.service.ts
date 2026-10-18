import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { LoginRequest, RegisterRequest, RegisterResponse, AuthResponse, AuthApiResponse } from '../models/auth.models';
import { tap, map, finalize, shareReplay, Observable } from 'rxjs';
import { TokenService } from './token.service';
import { UserStateService } from './user-state.service';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private api = environment.apiUrl + '/api/auth';
  private refreshRequest$?: Observable<AuthResponse>;

  constructor(
    private http: HttpClient,
    private tokenService: TokenService,
    private userState: UserStateService,
    private router: Router,
  ) { }

 login(request: LoginRequest) {
  return this.http.post<AuthApiResponse>(`${this.api}/login`, request).pipe(
    tap(res => {
      this.tokenService.setTokens(res.accessToken, res.refreshToken);
      this.userState.setUser(res.user, res.roles ?? []);
    }),
    map(res => res.user)
  );
}

  register(request: RegisterRequest) {
    return this.http.post<RegisterResponse>(`${this.api}/register`, request);
  }

  refreshToken() {
    if (this.refreshRequest$) return this.refreshRequest$;
    const refreshToken = this.tokenService.getRefreshToken();

    this.refreshRequest$ = this.http.post<AuthResponse>(
      `${this.api}/refresh-token`,
      refreshToken,
      { headers: { 'Content-Type': 'application/json' } }
    ).pipe(
      tap(res => {
        this.tokenService.setTokens(res.accessToken, res.refreshToken);
        if (res.user) {
          this.userState.setUser(res.user, res.roles ?? []);
        }
      }),
      finalize(() => this.refreshRequest$ = undefined),
      shareReplay({ bufferSize: 1, refCount: false }),
    );
    return this.refreshRequest$;
  }


  logout() {
  const token = this.tokenService.getRefreshToken();
  if (!token) {
    this.clearUserData();
    return;
  }

  this.http.post(`${this.api}/logout`, { token }).subscribe({
    next: () => this.clearUserData(),
    error: () => this.clearUserData(),
  });
}

private clearUserData() {
  this.tokenService.clear();
  this.userState.clearUser();
  this.router.navigate(['']);
}
}
