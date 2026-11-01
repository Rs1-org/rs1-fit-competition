import { Injectable, signal } from '@angular/core';

export type AuthModalMode = 'login' | 'register' | null;

@Injectable({ providedIn: 'root' })
export class AuthModalService {
  readonly mode = signal<AuthModalMode>(null);

  openLogin(): void {
    this.mode.set('login');
  }

  openRegister(): void {
    this.mode.set('register');
  }

  close(): void {
    this.mode.set(null);
  }
}
