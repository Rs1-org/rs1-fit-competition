import { Component, Input, ContentChild, TemplateRef, Output, EventEmitter, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-generic-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './generic-list.component.html',
  styleUrl: './generic-list.component.css',
  encapsulation: ViewEncapsulation.None
})
export class GenericListComponent<T> {
  @Input() items: T[] = [];
  @Input() variant: 'default' | 'card' | 'compact' = 'default';
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  
  @ContentChild('itemTemplate', { static: false }) 
  itemTemplate!: TemplateRef<any>;

  @Output() delete=new EventEmitter<T>();
  @Output() update=new EventEmitter<T>();
  @Output() view=new EventEmitter<T>();
  
  onDelete(item: T) {
    this.delete.emit(item);
  }
  onUpdate(item: T) {
    this.update.emit(item);
  }

  onView(item: T) {
    this.view.emit(item);
  }
}