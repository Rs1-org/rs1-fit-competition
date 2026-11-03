import { Component, input } from '@angular/core';
import { Crest } from '../crest/crest';

/** Header card for competition, season and round pages: crest, name, badge, subtitle, then tiles and actions. */
@Component({
  selector: 'app-entity-header',
  imports: [Crest],
  templateUrl: './entity-header.html',
  styleUrl: './entity-header.css',
})
export class EntityHeader {
  title = input.required<string>();
  /** Name and short code the crest is drawn from. */
  crestName = input<string | null>(null);
  crestCode = input<string | null | undefined>(null);
  badge = input<string>('');
  subtitle = input<string>('');
}
