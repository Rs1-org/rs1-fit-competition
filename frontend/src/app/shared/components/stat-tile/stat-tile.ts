import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-stat-tile',
  imports: [RouterLink],
  templateUrl: './stat-tile.html',
  styleUrl: './stat-tile.css',
})
export class StatTile {
  label = input.required<string>();
  value = input.required<string | number>();
  link = input<string | null>(null);
}
