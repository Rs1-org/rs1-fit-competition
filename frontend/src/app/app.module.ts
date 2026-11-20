import { LOCALE_ID, NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { AppRoutingModule } from './app-routing.module';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { refreshInterceptor } from './core/interceptors/refresh.interceptors';

@NgModule({
  imports: [BrowserModule, AppRoutingModule],
  providers: [
    provideBrowserGlobalErrorListeners(),
    { provide: LOCALE_ID, useValue: 'bs-Latn' },
    provideAnimations(),
    provideHttpClient(withInterceptors([authInterceptor, refreshInterceptor])),
  ],
})
export class AppModule {}