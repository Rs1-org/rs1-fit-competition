import { Component, input } from '@angular/core';
import { Statistic, statisticMetrics } from '../../core/models/statistic';
import { AppNumberPipe } from '../../shared/pipes/regional-format.pipes';

@Component({ selector: 'app-statistic-comparison', imports: [AppNumberPipe], templateUrl: './statistic-comparison.html',
  styleUrl: '../../pages/statistics/statistics.css' })
export class StatisticComparison {
  readonly statistic = input.required<Statistic>();
  readonly metrics = statisticMetrics;
}
