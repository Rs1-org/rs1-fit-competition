import { Component, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { Icon } from '../icon/icon';

export interface SectionTab { label: string; route: string; locked?: boolean; }

@Component({
  selector: 'app-section-tabs',
  imports: [RouterLink, RouterLinkActive, Icon],
  templateUrl: './section-tabs.html',
  styleUrl: './section-tabs.css',
})
export class SectionTabs {
  tabs = input.required<SectionTab[]>();
  label = input.required<string>();
}
