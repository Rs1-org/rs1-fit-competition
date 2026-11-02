import { AfterViewInit, Component, ElementRef, input, output, viewChild } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-input-box',
  imports: [ReactiveFormsModule],
  templateUrl: './input-box.html',
  styleUrl: './input-box.css',
})
export class InputBox implements AfterViewInit {
  private static nextId = 0;
  readonly inputId = `input-box-${InputBox.nextId++}`;
  readonly errorId = `${this.inputId}-error`;

  label = input<string>('');
  placeholder = input<string>('');
  control = input<FormControl<any>>(new FormControl());
  type = input<string>('text');
  accept = input<string>('');
  step = input<string>('');
  autofocus = input(false);
  /** Page-specific text per error key, e.g. { pastDate: 'Pick a future date.' }. */
  messages = input<Record<string, string>>({});
  fileChange = output<Event>();
  private field = viewChild<ElementRef<HTMLElement>>('field');

  ngAfterViewInit(): void {
    if (this.autofocus()) queueMicrotask(() => this.field()?.nativeElement.focus());
  }

  showError(): boolean {
    return this.control().invalid && this.control().touched;
  }

  errorMessage(): string {
    const errors = this.control().errors;
    if (!errors) return '';
    if (errors['server']) return errors['server'];
    const custom = Object.keys(errors).find((key) => this.messages()[key]);
    if (custom) return this.messages()[custom];
    if (errors['required']) return 'This field is required.';
    if (errors['min']) return `Must be at least ${errors['min'].min}.`;
    if (errors['max']) return `Must be at most ${errors['max'].max}.`;
    if (errors['maxlength']) return `Maximum ${errors['maxlength'].requiredLength} characters.`;
    if (errors['minlength']) return `Minimum ${errors['minlength'].requiredLength} characters.`;
    if (errors['email']) return 'Enter a valid email address.';
    if (errors['pastDate']) return 'Date of birth must be in the past.';
    return 'Invalid value.';
  }
}
