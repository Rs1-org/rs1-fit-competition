import { Component, computed, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatchEvent } from '../../core/models/match-event';
import { AppNumberPipe } from '../../shared/pipes/regional-format.pipes';

@Component({ selector: 'app-match-event-timeline', imports: [RouterLink, AppNumberPipe],
  templateUrl: './match-event-timeline.html', styleUrl: '../../pages/match-events/events.css' })
export class MatchEventTimeline {
  readonly events = input.required<MatchEvent[]>();
  readonly canManage = input(false);
  readonly busy = input(false);
  readonly showMatch = input(false);
  readonly remove = output<MatchEvent>();
  readonly groups = computed(() => {
    const groups = new Map<string, { key: string; name: string; events: MatchEvent[] }>();
    for (const item of this.events()) {
      const key = `${item.matchPublicId}/${item.matchPart.publicId}`;
      if (!groups.has(key)) groups.set(key, { key, name: `${this.showMatch() ? item.matchName + ' · ' : ''}${item.matchPart.name}`, events: [] });
      groups.get(key)!.events.push(item);
    }
    return [...groups.values()];
  });
}
