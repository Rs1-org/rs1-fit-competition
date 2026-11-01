import { Component, ElementRef, HostListener, effect, input, output, signal, viewChild } from '@angular/core';

@Component({
  selector: 'app-image-zoom',
  templateUrl: './image-zoom.html',
  styleUrl: './image-zoom.css',
})
export class ImageZoom {
  readonly src = input<string | null>(null);
  readonly open = input(false);
  readonly closed = output<void>();
  readonly scale = signal(1);
  private readonly dialog = viewChild<ElementRef<HTMLElement>>('dialog');

  constructor() {
    effect(() => {
      if (this.open()) {
        this.scale.set(1);
        queueMicrotask(() => this.dialog()?.nativeElement.focus());
      }
    });
  }

  @HostListener('document:keydown', ['$event'])
  onKey(event: KeyboardEvent): void {
    if (!this.open()) return;
    if (event.key === 'Escape') this.closed.emit();
    if (event.key === '+' || event.key === '=') this.zoom(0.25);
    if (event.key === '-' || event.key === '_') this.zoom(-0.25);
  }

  zoom(delta: number): void {
    this.scale.update((value) => Math.min(4, Math.max(1, value + delta)));
  }
}
