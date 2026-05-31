import { Component, Input, ContentChild, TemplateRef, Output, EventEmitter, ViewEncapsulation, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Pagination } from '../../shared/components/pagination/pagination';

@Component({
  selector: 'app-generic-list',
  standalone: true,
  imports: [CommonModule, Pagination],
  templateUrl: './generic-list.component.html',
  styleUrl: './generic-list.component.css',
  encapsulation: ViewEncapsulation.None
})
export class GenericListComponent<T> implements OnChanges {
  @Input() items: T[] = [];
  @Input() variant: 'default' | 'card' | 'compact' = 'default';
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Input() pageSize = 0;

  /** When provided, activates server-side mode: items are not sliced locally. */
  @Input() serverTotalItems?: number;
  /** Current page controlled by the parent in server-side mode. */
  @Input() serverCurrentPage = 1;

  /** When false, hides the update and delete action buttons. */
  @Input() showActions = true;

  @ContentChild('itemTemplate', { static: false })
  itemTemplate!: TemplateRef<any>;

  @Output() delete     = new EventEmitter<T>();
  @Output() update     = new EventEmitter<T>();
  @Output() view       = new EventEmitter<T>();
  @Output() pageChange = new EventEmitter<number>();

  /** Internal page used only in client-side mode. */
  clientPage = 1;

  get isServerSide(): boolean {
    return this.serverTotalItems !== undefined && this.pageSize > 0;
  }

  get pagedItems(): T[] {
    if (this.pageSize <= 0 || this.isServerSide) return this.items;
    const start = (this.clientPage - 1) * this.pageSize;
    return this.items.slice(start, start + this.pageSize);
  }

  get effectiveTotalItems(): number {
    return this.isServerSide ? this.serverTotalItems! : this.items.length;
  }

  get effectiveCurrentPage(): number {
    return this.isServerSide ? this.serverCurrentPage : this.clientPage;
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['items'] && !this.isServerSide) this.clientPage = 1;
  }

  onPageChange(page: number) {
    if (this.isServerSide) {
      this.pageChange.emit(page);
    } else {
      this.clientPage = page;
    }
  }

  onDelete(item: T) { this.delete.emit(item); }
  onUpdate(item: T) { this.update.emit(item); }
  onView(item: T)   { this.view.emit(item); }
}
