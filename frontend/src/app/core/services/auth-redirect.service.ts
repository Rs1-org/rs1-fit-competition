import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthRedirectService {

  private redirectUrl: string | null = null;

  constructor(private router: Router) {}

  setRedirectUrl(url: string) {
    this.redirectUrl = url;
  }

  redirect() {
    if (this.redirectUrl) {
      this.router.navigate([this.redirectUrl]);
      this.redirectUrl = null;
    }
  }
}