import { Component, inject } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { filter, map } from 'rxjs';
import { Navbar } from './shared/components/navbar/navbar';
import { AuthModalComponent } from './features/auth/auth-modal/auth-modal.component';
import { SettingsService } from './core/services/settings.service';
import { ConfirmDialog } from './shared/components/confirm-dialog/confirm-dialog';
import { ToastHost } from './shared/components/toast/toast';
import { routeAnimations } from './core/animations/motion';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar, ConfirmDialog, ToastHost, AuthModalComponent],
  templateUrl: './app.html',
  styleUrl: './app.css',
  animations: [routeAnimations],
})
export class App {
  private router = inject(Router);
  private settings = inject(SettingsService);

  readonly routeKey = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map((event) => event.urlAfterRedirects),
    ),
    { initialValue: this.router.url || '/' },
  );
}
