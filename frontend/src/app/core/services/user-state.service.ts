import { Injectable, computed } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject } from 'rxjs';
import { UserDto } from '../models/auth.models';

const ROLES_KEY = 'user_roles';
const MANAGE_ROLES = ['Admin', 'Moderator'];

@Injectable({
  providedIn: 'root',
})
export class UserStateService {
  private userSubject = new BehaviorSubject<UserDto | null>(this.getStoredUser());
  private rolesSubject = new BehaviorSubject<string[]>(this.getStoredRoles());

  user$ = this.userSubject.asObservable();
  roles$ = this.rolesSubject.asObservable();

  // Reactive role checks, mirroring the backend [Authorize(Roles = ...)] on write endpoints.
  private roles = toSignal(this.roles$, { requireSync: true });
  readonly canManage = computed(() => this.roles().some((role) => MANAGE_ROLES.includes(role)));
  readonly isAdmin = computed(() => this.roles().includes('Admin'));

  setUser(user: UserDto, roles: string[] = []) {
    localStorage.setItem('user', JSON.stringify(user));
    localStorage.setItem(ROLES_KEY, JSON.stringify(roles));
    this.userSubject.next(user);
    this.rolesSubject.next(roles);
  }

  clearUser() {
    localStorage.removeItem('user');
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

  private getStoredUser(): UserDto | null {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  }

  private getStoredRoles(): string[] {
    const roles = localStorage.getItem(ROLES_KEY);
    return roles ? JSON.parse(roles) : [];
  }
}
