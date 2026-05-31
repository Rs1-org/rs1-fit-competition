import { Component, input, signal, computed, HostListener, effect } from '@angular/core';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-dropdown',
  imports: [ReactiveFormsModule],
  templateUrl: './dropdown.html',
  styleUrl: './dropdown.css'
})
export class Dropdown {
  label = input<string>('');
  placeholder = input<string>('Select...');
  options = input<any[]>([]);
  optionLabel = input<string>('name');
  optionValue = input<string>('id');
  control = input.required<FormControl>();

  isOpen = signal(false);
  selectedValue = signal<any>(null);

  constructor() {
    effect(() => {
      const opts = this.options();
      const val = this.control().value;
      if (val && opts.length > 0) {
        const found = opts.find((o: any) => o[this.optionValue()] === val);
        if (found) this.selectedValue.set(found);
      }
    });
  }

  toggle() {
    this.isOpen.update(v => !v);
  }

  selectOption(option: any) {
    this.control().setValue(option[this.optionValue()]);
    this.control().markAsTouched();
    this.selectedValue.set(option);
    this.isOpen.set(false);
  }

  selectedLabel = computed(() => {
    const option = this.selectedValue();
    return option ? option[this.optionLabel()] : this.placeholder();
  });

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (!this.isOpen()) return;
    const target = event.target as HTMLElement;
    if (!target.closest('app-dropdown')) {
      this.isOpen.set(false);
    }
  }
}