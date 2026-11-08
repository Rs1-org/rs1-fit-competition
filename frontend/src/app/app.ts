import { Component, inject } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { filter, map, take } from 'rxjs';
import { Navbar } from './shared/components/navbar/navbar';
import { AuthModalComponent } from './features/auth/auth-modal/auth-modal.component';
import { SettingsService } from './core/services/settings.service';
import { ConfirmDialog } from './shared/components/confirm-dialog/confirm-dialog';
import { ToastHost } from './shared/components/toast/toast';
import { routeAnimations } from './core/animations/motion';
import { GuidedTour } from './shared/components/guided-tour/guided-tour';
import { TourService } from './core/services/tour.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar, ConfirmDialog, ToastHost, AuthModalComponent, GuidedTour],
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

  constructor() {
    // Start the first-visit tour once the first page has rendered, so the navbar targets exist.
    const tour = inject(TourService);
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd), take(1))
      .subscribe(() => setTimeout(() => tour.startIfFirstVisit(), 400));
  }
}
