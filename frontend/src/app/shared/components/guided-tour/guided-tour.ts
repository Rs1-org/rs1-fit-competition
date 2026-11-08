import { Component, DestroyRef, ElementRef, HostListener, afterNextRender, effect, inject, signal, viewChild } from '@angular/core';
import { TourService } from '../../../core/services/tour.service';
import { Button } from '../button/button';

interface Box { top: number; left: number; width: number; height: number; }

const GUTTER = 16;
const GAP = 12;
const PAD = 6;
const CARD_WIDTH = 340;

/** Overlay that highlights the current tour target and shows the step card next to it (centred when the target is not visible). */
@Component({
  selector: 'app-guided-tour',
  imports: [Button],
  templateUrl: './guided-tour.html',
  styleUrl: './guided-tour.css',
})
export class GuidedTour {
  readonly tour = inject(TourService);
  private destroyRef = inject(DestroyRef);
  private card = viewChild<ElementRef<HTMLElement>>('card');

  readonly highlight = signal<Box | null>(null);
  readonly cardPos = signal<{ top: number; left: number } | null>(null);

  /** Element that had focus before the tour opened; focus goes back to it when the tour closes. */
  private returnFocus: HTMLElement | null = null;

  constructor() {
    effect(() => {
      // Re-measure and move focus to the card whenever the step changes or the tour opens.
      this.tour.index();
      if (!this.tour.active()) {
        this.restoreFocus();
        return;
      }
      if (!this.returnFocus && document.activeElement instanceof HTMLElement) this.returnFocus = document.activeElement;
      // Two frames: the card is rendered before it is measured, so its real height is used.
      requestAnimationFrame(() => requestAnimationFrame(() => {
        this.place();
        this.card()?.nativeElement.focus();
      }));
    });
    afterNextRender(() => {
      const onScroll = () => this.tour.active() && this.place();
      window.addEventListener('scroll', onScroll, true);
      this.destroyRef.onDestroy(() => window.removeEventListener('scroll', onScroll, true));
    });
  }

  @HostListener('window:resize')
  onResize(): void {
    if (this.tour.active()) this.place();
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.tour.active()) this.tour.skip();
  }

  /** Keeps Tab inside the tour card while it is open (it is a modal dialog). */
  @HostListener('document:keydown.tab', ['$event'])
  @HostListener('document:keydown.shift.tab', ['$event'])
  onTab(event: Event): void {
    const card = this.card()?.nativeElement;
    if (!this.tour.active() || !card) return;
    const buttons = [...card.querySelectorAll<HTMLElement>('button:not([disabled])')];
    if (!buttons.length) return;
    const first = buttons[0];
    const last = buttons[buttons.length - 1];
    const backwards = (event as KeyboardEvent).shiftKey;
    const current = document.activeElement;
    const inside = !!current && card.contains(current);
    if (!inside || (backwards && current === first) || (!backwards && current === last)) {
      event.preventDefault();
      (backwards ? last : first).focus();
    }
  }

  private restoreFocus(): void {
    const element = this.returnFocus;
    this.returnFocus = null;
    if (element?.isConnected) element.focus();
  }

  /** Measures the target and puts the card below it, above it when there is no room, or centred when it is hidden. */
  place(): void {
    const target = document.querySelector<HTMLElement>(`[data-tour="${this.tour.step().target}"]`);
    const rect = target?.getBoundingClientRect();
    const vw = window.innerWidth;
    const vh = window.innerHeight;
    const visible = !!rect && rect.width > 0 && rect.height > 0 && rect.bottom > 0 && rect.top < vh;

    if (!rect || !visible) {
      this.highlight.set(null);
      this.cardPos.set(null);
    } else {
      this.highlight.set({ top: rect.top - PAD, left: rect.left - PAD, width: rect.width + PAD * 2, height: rect.height + PAD * 2 });
      const width = Math.min(CARD_WIDTH, vw - GUTTER * 2);
      const cardHeight = this.card()?.nativeElement.offsetHeight ?? 200;
      const below = rect.bottom + PAD + GAP;
      const top = below + cardHeight <= vh - GUTTER ? below : Math.max(GUTTER, rect.top - PAD - GAP - cardHeight);
      const left = Math.min(Math.max(GUTTER, rect.left + rect.width / 2 - width / 2), vw - GUTTER - width);
      this.cardPos.set({ top, left });
    }
  }
}
