import { Component, input, output } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-input-box',
  imports: [ReactiveFormsModule],
  templateUrl: './input-box.html',
  styleUrl: './input-box.css',
})
export class InputBox {
  private static nextId = 0;
  readonly inputId = `input-box-${InputBox.nextId++}`;
  readonly errorId = `${this.inputId}-error`;

  label = input<string>('');
  placeholder = input<string>('');
  control = input<FormControl>(new FormControl());
  type = input<string>('text');
  accept = input<string>('');
  fileChange = output<Event>();

  showError(): boolean {
    return this.control().invalid && this.control().touched;
  }

  errorMessage(): string {
    const errors = this.control().errors;
    if (!errors) return '';
    if (errors['required']) return 'This field is required.';
    if (errors['min']) return `Must be at least ${errors['min'].min}.`;
    if (errors['max']) return `Must be at most ${errors['max'].max}.`;
    if (errors['maxlength']) return `Maximum ${errors['maxlength'].requiredLength} characters.`;
    if (errors['minlength']) return `Minimum ${errors['minlength'].requiredLength} characters.`;
    if (errors['email']) return 'Enter a valid email address.';
    return 'Invalid value.';
  }
}
