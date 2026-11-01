import { Component, input, output } from '@angular/core';
import { Button } from '../button/button';
import { Icon } from '../icon/icon';

export interface FilterChip {
  key: string;
  label: string;
}

@Component({
  selector: 'app-filter-card',
  imports: [Button, Icon],
  templateUrl: './filter-card.html',
  styleUrl: './filter-card.css',
})
export class FilterCard {
  chips = input<FilterChip[]>([]);
  chipRemove = output<string>();
  resetFilters = output<void>();
  applyFilters = output<void>();

  onSubmit(event: Event): void {
    event.preventDefault();
    this.applyFilters.emit();
  }
}
