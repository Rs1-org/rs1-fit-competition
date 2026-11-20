import { Injectable, computed } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject } from 'rxjs';
import { UserDto } from '../models/auth.models';

const USER_KEY = 'user';
const ROLES_KEY = 'user_roles';
const ACCESS_TOKEN_KEY = 'access_token';
const REFRESH_TOKEN_KEY = 'refresh_token';
const MANAGE_ROLES = ['Admin', 'Moderator'];

function isValidStoredUser(value: unknown): value is UserDto {
  if (!value || typeof value !== 'object') return false;
  const record = value as Record<string, unknown>;
  return (
    typeof record['id'] === 'string' &&
    typeof record['email'] === 'string' &&
    typeof record['firstName'] === 'string' &&
    typeof record['lastName'] === 'string' &&
    typeof record['phoneNumber'] === 'string'
  );
}

function isValidStoredRoles(value: unknown): value is string[] {
  return Array.isArray(value) && value.every((role) => typeof role === 'string');
}

function clearCorruptedAuthStorage(): void {
  localStorage.removeItem(USER_KEY);
  localStorage.removeItem(ROLES_KEY);
  localStorage.removeItem(ACCESS_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
}

function readStoredAuth(): { user: UserDto | null; roles: string[] } {
  const userRaw = localStorage.getItem(USER_KEY);
  const rolesRaw = localStorage.getItem(ROLES_KEY);

  let user: UserDto | null = null;
  let roles: string[] = [];

  if (userRaw) {
    try {
      const parsed: unknown = JSON.parse(userRaw);
      if (!isValidStoredUser(parsed)) {
        clearCorruptedAuthStorage();
        return { user: null, roles: [] };
      }
      user = parsed;
    } catch {
      clearCorruptedAuthStorage();
      return { user: null, roles: [] };
    }
  }

  if (rolesRaw) {
    try {
      const parsed: unknown = JSON.parse(rolesRaw);
      if (!isValidStoredRoles(parsed)) {
        clearCorruptedAuthStorage();
        return { user: null, roles: [] };
      }
      roles = parsed;
    } catch {
      clearCorruptedAuthStorage();
      return { user: null, roles: [] };
    }
  }

  return { user, roles };
}

@Injectable({
  providedIn: 'root',
})
export class UserStateService {
  private readonly userSubject: BehaviorSubject<UserDto | null>;
  private readonly rolesSubject: BehaviorSubject<string[]>;
  readonly user$;
  readonly roles$;

  // Reactive role checks, mirroring the backend [Authorize(Roles = ...)] on write endpoints.
  private readonly roles;
  readonly canManage;
  readonly isAdmin;

  // Display only: the backend checks premium itself and answers 403 once it has expired.
  private readonly currentUser;
  readonly isSignedIn;
  readonly isPremium;

  constructor() {
    const state = readStoredAuth();
    this.userSubject = new BehaviorSubject(state.user);
    this.rolesSubject = new BehaviorSubject(state.roles);
    this.user$ = this.userSubject.asObservable();
    this.roles$ = this.rolesSubject.asObservable();
    this.roles = toSignal(this.roles$, { requireSync: true });
    this.canManage = computed(() => this.roles().some((role) => MANAGE_ROLES.includes(role)));
    this.isAdmin = computed(() => this.roles().includes('Admin'));
    this.currentUser = toSignal(this.user$, { requireSync: true });
    this.isSignedIn = computed(() => this.currentUser() !== null);
    this.isPremium = computed(() => {
      const until = this.currentUser()?.premiumUntil;
      return !!until && new Date(until).getTime() > Date.now();
    });
  }

  setUser(user: UserDto, roles: string[] = []) {
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    localStorage.setItem(ROLES_KEY, JSON.stringify(roles));
    this.userSubject.next(user);
    this.rolesSubject.next(roles);
  }

  clearUser() {
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem(ROLES_KEY);
    this.userSubject.next(null);
    this.rolesSubject.next([]);
  }

  getUser() {
    return this.userSubject.value;
  }

  getRoles(): string[] {
    return this.rolesSubject.value;
  }

  hasAnyRole(roles: string[]): boolean {
    const userRoles = this.rolesSubject.value;
    return roles.some((role) => userRoles.includes(role));
  }

  canManagePersons(): boolean {
    return this.canManage();
  }

}
