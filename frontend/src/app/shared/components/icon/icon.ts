import { Component, computed, inject, input } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';

const ICONS = {
  home: '<path d="M3 11l9-8 9 8"/><path d="M5 10v10h14V10"/><path d="M10 20v-6h4v6"/>',
  trophy: '<path d="M8 4h8v6a4 4 0 0 1-8 0z"/><path d="M8 6H4v2a4 4 0 0 0 4 4"/><path d="M16 6h4v2a4 4 0 0 1-4 4"/><path d="M12 14v4"/><path d="M8 20h8"/>',
  heart: '<path d="M12 20s-8-5-8-11a4.5 4.5 0 0 1 8-2.8A4.5 4.5 0 0 1 20 9c0 6-8 11-8 11z"/>',
  player: '<circle cx="12" cy="5" r="2"/><path d="M12 7v7"/><path d="M6 11l6-2 6 2"/><path d="M12 14l-3 7"/><path d="M12 14l3 7"/>',
  whistle: '<circle cx="9" cy="14" r="5"/><path d="M9 9h12v3l-7 2"/>',
  manager: '<circle cx="12" cy="7" r="4"/><path d="M4 21a8 8 0 0 1 16 0"/><path d="M12 14v4"/>',
  settings: '<circle cx="12" cy="12" r="3"/><path d="M12 3v3"/><path d="M12 18v3"/><path d="M3 12h3"/><path d="M18 12h3"/><path d="M5.6 5.6l2.1 2.1"/><path d="M16.3 16.3l2.1 2.1"/><path d="M5.6 18.4l2.1-2.1"/><path d="M16.3 7.7l2.1-2.1"/>',
  user: '<circle cx="12" cy="8" r="4"/><path d="M4 21a8 8 0 0 1 16 0"/>',
  edit: '<path d="M4 20l1-4L16 5l3 3L8 19z"/><path d="M14 7l3 3"/>',
  trash: '<path d="M4 7h16"/><path d="M9 7V4h6v3"/><path d="M6 7l1 13h10l1-13"/><path d="M10 11v5"/><path d="M14 11v5"/>',
  eye: '<path d="M2 12s4-7 10-7 10 7 10 7-4 7-10 7-10-7-10-7z"/><circle cx="12" cy="12" r="3"/>',
  'eye-off': '<path d="M4 4l16 16"/><path d="M10 5.2A10 10 0 0 1 12 5c6 0 10 7 10 7a18 18 0 0 1-3 3.7"/><path d="M6.5 7.5A18 18 0 0 0 2 12s4 7 10 7a10 10 0 0 0 4.5-1.1"/><path d="M10 10a3 3 0 0 0 4 4"/>',
  file: '<path d="M6 3h8l4 4v14H6z"/><path d="M14 3v4h4"/><path d="M9 13h6"/><path d="M9 17h6"/>',
  upload: '<path d="M12 16V4"/><path d="M7 9l5-5 5 5"/><path d="M4 20h16"/>',
  check: '<path d="M5 12l5 5 9-10"/>',
  x: '<path d="M6 6l12 12"/><path d="M18 6L6 18"/>',
  inbox: '<path d="M3 13l3-9h12l3 9v7H3z"/><path d="M3 13h5l2 3h4l2-3h5"/>',
  'chevron-left': '<path d="M15 5l-7 7 7 7"/>',
  'chevron-right': '<path d="M9 5l7 7-7 7"/>',
  lock: '<rect x="5" y="11" width="14" height="10" rx="2"/><path d="M8 11V7a4 4 0 0 1 8 0v4"/>',
  chart: '<path d="M4 20h16"/><path d="M7 20v-7"/><path d="M12 20V5"/><path d="M17 20v-10"/>',
  clock: '<circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/>',
  ball: '<circle cx="12" cy="12" r="8"/><path d="M12 8.2l2.1 1.6-.8 2.5h-2.6l-.8-2.5z"/>',
  'penalty-missed': '<circle cx="12" cy="12" r="8"/><path d="M9 9l6 6"/><path d="M15 9l-6 6"/>',
  card: '<rect x="8" y="3" width="8" height="16" rx="1.5" transform="rotate(8 12 12)"/>',
  'sub-in': '<path d="M12 19V7"/><path d="M7 12l5-5 5 5"/>',
  'sub-out': '<path d="M12 5v12"/><path d="M7 12l5 5 5-5"/>',
} as const;

export type IconName = keyof typeof ICONS;

@Component({
  selector: 'app-icon',
  imports: [],
  templateUrl: './icon.html',
  styleUrl: './icon.css',
})
export class Icon {
  private sanitizer = inject(DomSanitizer);

  name = input.required<IconName>();
  label = input<string>('');
  size = input<number>(18);

  paths = computed(() => this.sanitizer.bypassSecurityTrustHtml(ICONS[this.name()]));
}
