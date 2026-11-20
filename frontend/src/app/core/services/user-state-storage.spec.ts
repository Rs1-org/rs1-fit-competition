import { TestBed } from '@angular/core/testing';
import { UserStateService } from './user-state.service';

describe('UserStateService stored auth', () => {
  const validUser = {
    id: '00000000-0000-0000-0000-000000000001',
    email: 'dev@example.invalid',
    firstName: 'Dev',
    lastName: 'User',
    phoneNumber: '061111111',
  };

  beforeEach(() => localStorage.clear());
  afterEach(() => localStorage.clear());

  it('loads a valid user and roles from localStorage', () => {
    localStorage.setItem('user', JSON.stringify(validUser));
    localStorage.setItem('user_roles', JSON.stringify(['User']));
    const service = TestBed.inject(UserStateService);
    expect(service.getUser()).toEqual(validUser);
    expect(service.getRoles()).toEqual(['User']);
    expect(service.isSignedIn()).toBe(true);
  });

  it('clears corrupted JSON and starts signed out', () => {
    localStorage.setItem('user', '{not-json');
    localStorage.setItem('access_token', 'stale');
    const service = TestBed.inject(UserStateService);
    expect(service.getUser()).toBeNull();
    expect(service.isSignedIn()).toBe(false);
    expect(localStorage.getItem('user')).toBeNull();
    expect(localStorage.getItem('access_token')).toBeNull();
  });

  it('rejects roles that are not a string array', () => {
    localStorage.setItem('user', JSON.stringify(validUser));
    localStorage.setItem('user_roles', JSON.stringify({ role: 'Admin' }));
    const service = TestBed.inject(UserStateService);
    expect(service.getUser()).toBeNull();
    expect(service.getRoles()).toEqual([]);
    expect(localStorage.getItem('user')).toBeNull();
  });

  it('starts signed out when storage is empty', () => {
    const service = TestBed.inject(UserStateService);
    expect(service.getUser()).toBeNull();
    expect(service.getRoles()).toEqual([]);
    expect(service.isSignedIn()).toBe(false);
  });
});