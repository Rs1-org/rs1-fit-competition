import { Component, computed, input } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-player-card',
  imports: [RouterLink, NgTemplateOutlet],
  templateUrl: './player-card.html',
  styleUrl: './player-card.css',
})
export class PlayerCard {
  name = input.required<string>();
  position = input<string | null>(null);
  number = input<number | null>(null);
  age = input<number | null>(null);
  value = input<string | null>(null);
  picture = input<string | null>(null);
  link = input<string | null>(null);

  initials = computed(() =>
    this.name()
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join(''),
  );
}
