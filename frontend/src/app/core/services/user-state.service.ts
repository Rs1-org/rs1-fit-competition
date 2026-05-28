import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { UserDto } from '../models/auth.models';

const ROLES_KEY = 'user_roles';
const MANAGE_PERSON_ROLES = ['Admin', 'Moderator'];

@Injectable({
  providedIn: 'root',
})
export class UserStateService {
  private userSubject = new BehaviorSubject<UserDto | null>(this.getStoredUser());
  private rolesSubject = new BehaviorSubject<string[]>(this.getStoredRoles());

  user$ = this.userSubject.asObservable();
  roles$ = this.rolesSubject.asObservable();

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
    return this.hasAnyRole(MANAGE_PERSON_ROLES);
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
