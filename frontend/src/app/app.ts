import { Component, inject } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { filter, map } from 'rxjs';
import { Navbar } from './shared/components/navbar/navbar';
import { ConfirmDialog } from './shared/components/confirm-dialog/confirm-dialog';
import { routeAnimations } from './core/animations/motion';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar, ConfirmDialog],
  templateUrl: './app.html',
  styleUrl: './app.css',
  animations: [routeAnimations],
})
export class App {
  private router = inject(Router);

  readonly routeKey = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map((event) => event.urlAfterRedirects),
    ),
    { initialValue: this.router.url || '/' },
  );
}
