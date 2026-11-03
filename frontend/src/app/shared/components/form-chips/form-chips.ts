import { Component, input } from '@angular/core';
import { FormResult } from '../../match/match-display';

@Component({
  selector: 'app-form-chips',
  templateUrl: './form-chips.html',
  styleUrl: './form-chips.css',
})
export class FormChips {
  results = input<FormResult[]>([]);
  /** Smaller chips for table cells. */
  compact = input(false);
}
