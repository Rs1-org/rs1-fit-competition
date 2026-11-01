import { AfterViewInit, Directive, ElementRef, inject, Input } from '@angular/core';

@Directive({ selector: '[appAutofocus]' })
export class AutofocusDirective implements AfterViewInit {
  private element = inject(ElementRef<HTMLElement>);
  @Input() appAutofocus: boolean | '' = '';

  ngAfterViewInit(): void {
    if (this.appAutofocus === false) return;
    queueMicrotask(() => this.element.nativeElement.focus());
  }
}
