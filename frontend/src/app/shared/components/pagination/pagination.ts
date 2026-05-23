import { Component, computed, input, output } from '@angular/core';

@Component({
  selector: 'app-pagination',
  standalone: true,
  templateUrl: './pagination.html',
  styleUrl: './pagination.css',
})
export class Pagination {
  totalItems  = input.required<number>();
  pageSize    = input<number>(10);
  currentPage = input<number>(1);

  pageChange = output<number>();

  totalPages = computed(() => Math.max(1, Math.ceil(this.totalItems() / this.pageSize())));

  rangeStart = computed(() => Math.min((this.currentPage() - 1) * this.pageSize() + 1, this.totalItems()));
  rangeEnd   = computed(() => Math.min(this.currentPage() * this.pageSize(), this.totalItems()));

  pages = computed<(number | '...')[]>(() => {
    const total   = this.totalPages();
    const current = this.currentPage();

    if (total <= 7) {
      return Array.from({ length: total }, (_, i) => i + 1);
    }

    const result: (number | '...')[] = [1];

    if (current > 3) result.push('...');

    const start = Math.max(2, current - 1);
    const end   = Math.min(total - 1, current + 1);
    for (let i = start; i <= end; i++) result.push(i);

    if (current < total - 2) result.push('...');

    result.push(total);
    return result;
  });

  goTo(page: number | '...') {
    if (page === '...' || page === this.currentPage()) return;
    if (page < 1 || page > this.totalPages()) return;
    this.pageChange.emit(page);
  }

  prev() { this.goTo(this.currentPage() - 1); }
  next() { this.goTo(this.currentPage() + 1); }
}
