import { Component, computed, input } from '@angular/core';
import { Icon, IconName } from '../icon/icon';

const EVENTS: Record<string, { icon: IconName; label: string; tone: string }> = {
  goal: { icon: 'ball', label: 'Goal', tone: 'accent' },
  'own-goal': { icon: 'ball', label: 'Own goal', tone: 'danger' },
  penalty: { icon: 'ball', label: 'Penalty scored', tone: 'accent' },
  'penalty-missed': { icon: 'penalty-missed', label: 'Penalty missed', tone: 'muted' },
  'yellow-card': { icon: 'card', label: 'Yellow card', tone: 'yellow' },
  'red-card': { icon: 'card', label: 'Red card', tone: 'danger' },
  'second-yellow': { icon: 'card', label: 'Second yellow', tone: 'yellow' },
  substitution: { icon: 'sub-in', label: 'Substitution', tone: 'accent' },
};

@Component({
  selector: 'app-event-icon',
  imports: [Icon],
  templateUrl: './event-icon.html',
  styleUrl: './event-icon.css',
})
export class EventIcon {
  code = input.required<string>();
  event = computed(() => EVENTS[this.code()] ?? { icon: 'ball' as IconName, label: this.code(), tone: 'muted' });
}
