import { Component, ElementRef, OnDestroy, computed, effect, inject, input, viewChild } from '@angular/core';
import { BarController, BarElement, CategoryScale, Chart as ChartJs, ChartConfiguration, ChartDataset, Legend, LineController, LineElement,
  LinearScale, PointElement, Tooltip } from 'chart.js';
import { ThemeService } from '../../../core/services/theme.service';
import { EmptyState } from '../empty-state/empty-state';

ChartJs.register(BarController, BarElement, LineController, LineElement, PointElement, CategoryScale, LinearScale, Legend, Tooltip);

/** One series; `color` is a CSS variable name so the chart follows the light/dark theme. */
export interface ChartSeries { label: string; data: number[]; color: string; type?: 'bar' | 'line'; axis?: 'left' | 'right'; }

/** Seam for tests: jsdom has no canvas context. */
export const chartFactory = { create: (canvas: HTMLCanvasElement, config: ChartConfiguration) => new ChartJs(canvas, config) };

let nextId = 0;

@Component({
  selector: 'app-chart',
  imports: [EmptyState],
  template: `<figure class="chart-box"><figcaption><span class="chart-title">{{ title() }}</span>
      @if (subtitle()) { <span class="chart-subtitle">{{ subtitle() }}</span> }</figcaption>
    @if (labels().length) {
      <div class="chart-canvas" [style.height.px]="height()"><canvas #canvas role="img" [attr.aria-label]="title()" [attr.aria-describedby]="descriptionId"></canvas></div>
      <p class="visually-hidden" [id]="descriptionId">{{ description() }}</p>
    } @else { <app-empty-state icon="chart" [title]="emptyMessage()" /> }</figure>`,
  styles: [`.chart-box { margin: 0; padding: 1.25rem; background: var(--card); border: 1px solid var(--bd); border-radius: var(--r-card); box-shadow: var(--shadow); min-width: 0; }
    figcaption { display: flex; flex-wrap: wrap; align-items: baseline; gap: .25rem .75rem; margin-bottom: .75rem; }
    .chart-title { font-family: var(--font-title); font-size: 22px; font-weight: 700; color: var(--tx); }
    .chart-subtitle { font-size: 13px; color: var(--mut); }
    .chart-canvas { position: relative; height: 320px; }
    @media (max-width: 768px) { .chart-canvas { height: 280px; } }`],
})
export class Chart implements OnDestroy {
  readonly title = input.required<string>();
  readonly labels = input.required<string[]>();
  readonly series = input.required<ChartSeries[]>();
  readonly stacked = input(false);
  readonly horizontal = input(false);
  readonly yTitle = input('');
  readonly subtitle = input('');
  readonly emptyMessage = input('No statistics recorded for this season yet.');
  readonly height = input(320);
  readonly descriptionId = `chart-description-${nextId++}`;
  private readonly canvas = viewChild<ElementRef<HTMLCanvasElement>>('canvas');
  private readonly theme = inject(ThemeService);
  private chart?: ChartJs;

  readonly description = computed(() => this.labels().map((label, index) =>
    `${label}: ${this.series().map(s => `${s.label} ${s.data[index]}`).join(', ')}`).join('. '));

  constructor() {
    effect(() => {
      this.theme.isDark();
      const canvas = this.canvas()?.nativeElement;
      const config = this.config(this.labels(), this.series(), this.stacked(), this.horizontal(), this.yTitle());
      this.destroyChart();
      if (canvas) this.chart = chartFactory.create(canvas, config);
    });
    // The first draw can happen before Rajdhani has loaded.
    document.fonts?.ready.then(() => this.chart?.update());
  }

  private config(labels: string[], series: ChartSeries[], stacked: boolean, horizontal: boolean, yTitle: string): ChartConfiguration {
    const css = getComputedStyle(document.documentElement);
    const color = (name: string) => css.getPropertyValue(name).trim();
    const text = color('--tx2'), grid = color('--bd');
    const font = { family: color('--font-title'), size: 13, weight: 600 };
    const datasets = series.map(s => ({
      type: s.type ?? 'bar', label: s.label, data: s.data, order: s.type === 'line' ? 0 : 1,
      backgroundColor: color(s.color), borderColor: color(s.color), borderWidth: s.type === 'line' ? 2 : 0,
      borderRadius: s.type === 'line' ? 0 : 4, pointBackgroundColor: color('--card'), pointBorderWidth: 2, pointRadius: 4,
      ...(s.axis === 'right' ? { yAxisID: 'y1' } : {}),
    })) as ChartDataset<'bar'>[];
    const category = { stacked, ticks: { color: text, font, autoSkip: !horizontal, maxRotation: 45 }, grid: { display: false } };
    const value = { stacked, beginAtZero: true, ticks: { color: text, font, precision: 0 }, grid: { color: grid },
      title: { display: !!yTitle, text: yTitle, color: text, font } };
    const right = { position: 'right' as const, beginAtZero: true, ticks: { color: text, font }, grid: { display: false } };
    return {
      type: 'bar',
      data: { labels, datasets },
      options: {
        indexAxis: horizontal ? 'y' : 'x',
        responsive: true, maintainAspectRatio: false,
        interaction: { mode: 'index', intersect: false, axis: horizontal ? 'y' : 'x' },
        plugins: {
          legend: { position: 'bottom', labels: { color: text, font, usePointStyle: true, padding: 16 } },
          tooltip: { enabled: true, titleFont: font, bodyFont: font },
        },
        scales: horizontal ? { y: category, x: value }
          : { x: category, y: value, ...(series.some(s => s.axis === 'right') ? { y1: right } : {}) },
      },
    };
  }

  private destroyChart(): void { this.chart?.destroy(); this.chart = undefined; }

  ngOnDestroy(): void { this.destroyChart(); }
}
