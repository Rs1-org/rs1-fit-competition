import { Component, computed, input } from '@angular/core';
import { crestColour, teamCode } from '../../match/match-display';

@Component({
  selector: 'app-crest',
  templateUrl: './crest.html',
  styleUrl: './crest.css',
})
export class Crest {
  name = input<string | null>(null);
  abbreviation = input<string | null | undefined>(null);

  code = computed(() => teamCode(this.name(), this.abbreviation()));
  colour = computed(() => crestColour(this.code()));
}
