import { Component, input, output } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-input-box',
  imports: [ReactiveFormsModule],
  templateUrl: './input-box.html',
  styleUrl: './input-box.css',
})
export class InputBox {
  label = input<string>('');
  placeholder = input<string>('');
  control = input<FormControl>(new FormControl());
  type = input<string>('text');
  accept = input<string>('');
  fileChange = output<Event>();
}