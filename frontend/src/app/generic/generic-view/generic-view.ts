import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-generic-view',
  imports: [CommonModule],
  templateUrl: './generic-view.html',
  styleUrl: './generic-view.css',
})
export class GenericView {
  @Input() data: any;
  @Input() title: string = 'Details';

  get displayProperties() {
    if (!this.data) return [];
    const hiddenFields = ['id', 'deletedAt', 'id_key']; 
    return Object.keys(this.data)
      .filter(key => !hiddenFields.includes(key))
      .map(key => ({
        key: key.charAt(0).toUpperCase() + key.slice(1), 
        value: this.data[key]
      }));
  }
}
