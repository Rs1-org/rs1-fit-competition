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
  size = input(36);

  code = computed(() => teamCode(this.name(), this.abbreviation()));
  colour = computed(() => crestColour(this.code()));
  frame = computed(() => {
    const size = this.size();
    return {
      background: this.colour(),
      width: `${size}px`,
      height: `${Math.round(size * 1.16)}px`,
      flexBasis: `${size}px`,
      fontSize: `${Math.max(12, Math.round(size * 0.28))}px`,
    };
  });
}
