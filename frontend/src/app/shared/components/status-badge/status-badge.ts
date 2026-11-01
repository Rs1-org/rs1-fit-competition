import { Component, computed, input } from '@angular/core';
import { matchPhase, statusLabel, statusShort } from '../../match/match-display';

@Component({
  selector: 'app-status-badge',
  templateUrl: './status-badge.html',
  styleUrl: './status-badge.css',
})
export class StatusBadge {
  status = input<string | null>(null);
  postponed = input(false);
  minute = input<string | number | null>(null);

  phase = computed(() => matchPhase(this.status(), this.postponed()));
  short = computed(() => statusShort(this.phase(), this.minute() == null ? null : String(this.minute())));
  full = computed(() => statusLabel(this.phase()));
}
