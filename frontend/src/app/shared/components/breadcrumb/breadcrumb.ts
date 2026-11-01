import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

export interface Crumb { label: string; route?: string | null; }

@Component({
  selector: 'app-breadcrumb',
  imports: [RouterLink],
  templateUrl: './breadcrumb.html',
  styleUrl: './breadcrumb.css',
})
export class Breadcrumb {
  items = input.required<Crumb[]>();
}
