import { Component, Injectable } from '@angular/core';

@Component({
  selector: 'app-theme-service',
  imports: [],
  templateUrl: './theme-service.html',
  styleUrl: './theme-service.css',
})
@Injectable({
  providedIn:'root'
})
export class ThemeService {
  isDark=false;

  toggle():void{
    this.isDark=!this.isDark;
    document.documentElement.setAttribute('data-theme',
      this.isDark? 'dark':'light'
    );
  }
}
