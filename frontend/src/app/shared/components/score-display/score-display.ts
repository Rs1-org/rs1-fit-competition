import { Component, computed, input } from '@angular/core';
import { scoreText } from '../../match/match-display';

@Component({
  selector: 'app-score-display',
  templateUrl: './score-display.html',
  styleUrl: './score-display.css',
})
export class ScoreDisplay {
  home = input<number | null>(null);
  away = input<number | null>(null);
  size = input<'card' | 'hero' | 'inline'>('card');

  text = computed(() => scoreText(this.home(), this.away()));
}
