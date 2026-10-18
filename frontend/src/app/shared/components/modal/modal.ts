import { AfterViewInit, Component, ElementRef, EventEmitter, Input, OnDestroy, Output, ViewChild } from '@angular/core';
import { fadeIn } from '../../../core/animations/motion';

@Component({
  selector: 'app-modal',
  imports: [],
  templateUrl: './modal.html',
  styleUrl: './modal.css',
  animations: [fadeIn],
  host: { '[@fadeIn]': 'in' },
})
export class Modal implements AfterViewInit, OnDestroy {
  private static nextId = 0;
  readonly titleId = `modal-title-${Modal.nextId++}`;
  @ViewChild('dialog', { static: true }) private dialog!: ElementRef<HTMLElement>;
  private previousFocus: HTMLElement | null = null;
  @Input() title:string='';
  @Input() width:string='500px';
  @Input() height:string='auto';
  @Output() close=new EventEmitter();

  ngAfterViewInit(): void {
    this.previousFocus = document.activeElement instanceof HTMLElement ? document.activeElement : null;
    this.dialog.nativeElement.focus();
  }

  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Escape') {
      event.preventDefault();
      event.stopPropagation();
      this.close.emit();
    } else if (event.key === 'Tab') {
      const controls = Array.from(this.dialog.nativeElement.querySelectorAll<HTMLElement>(
        'button:not([disabled]), a[href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex="0"]',
      )).filter(element => !element.hidden && element.getAttribute('aria-hidden') !== 'true');
      const first = controls[0];
      const last = controls[controls.length - 1];
      if (!first) { event.preventDefault(); return; }
      if (event.shiftKey && (document.activeElement === first || document.activeElement === this.dialog.nativeElement)) {
        event.preventDefault(); last.focus();
      } else if (!event.shiftKey && (document.activeElement === last || document.activeElement === this.dialog.nativeElement)) {
        event.preventDefault(); first.focus();
      }
    }
  }

  ngOnDestroy(): void { this.previousFocus?.focus(); }

}
