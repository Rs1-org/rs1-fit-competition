import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-error-banner',
  imports: [],
  templateUrl: './error-banner.html',
  styleUrl: './error-banner.css',
})
export class ErrorBanner {
  message = input.required<string>();
  retry = output<void>();
}
