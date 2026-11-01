import { Component, input } from '@angular/core';

export type ButtonVariant = 'primary' | 'secondary' | 'danger' | 'ghost' | 'icon';

@Component({
  selector: 'app-button',
  imports: [],
  templateUrl: './button.html',
  styleUrl: './button.css',
})
export class Button {
  text = input<string>('');
  type = input<string>('button');
  form = input<string>('');
  variant = input<ButtonVariant>('primary');
  loading = input<boolean>(false);
  loadingText = input<string>('Saving...');
  disabled = input<boolean>(false);
  /** Screen reader text for icon-only buttons. */
  label = input<string>('');
}
