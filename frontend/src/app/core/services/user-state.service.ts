import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { UserDto } from "../models/auth.models";

@Injectable({
  providedIn: 'root'
})
export class UserStateService {

  private userSubject = new BehaviorSubject<UserDto | null>(this.getStoredUser());
  user$ = this.userSubject.asObservable();

  setUser(user: UserDto){
    localStorage.setItem('user', JSON.stringify(user));
    this.userSubject.next(user);
  }

  clearUser(){
    localStorage.removeItem('user');
    this.userSubject.next(null);
  }

  getUser(){
    return this.userSubject.value;
  }

  private getStoredUser(): UserDto | null {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  }
}
