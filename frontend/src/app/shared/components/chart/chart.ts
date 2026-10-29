import { Component, ElementRef, OnDestroy, effect, inject, input, viewChild } from '@angular/core';
import { BarController, BarElement, CategoryScale, Chart as ChartJs, ChartDataset, Legend, LineController, LineElement,
  LinearScale, PointElement, Tooltip } from 'chart.js';
import { ThemeService } from '../../../core/services/theme.service';

ChartJs.register(BarController, BarElement, LineController, LineElement, PointElement, CategoryScale, LinearScale, Legend, Tooltip);

/** One series; `color` is a CSS variable name so the chart follows the light/dark theme. */
export interface ChartSeries { label: string; data: number[]; color: string; type?: 'bar' | 'line'; }

@Component({
  selector: 'app-chart',
  template: `<figure class="chart-box"><figcaption>{{ title() }}</figcaption>
    <div class="chart-canvas"><canvas #canvas role="img" [attr.aria-label]="title() + ': ' + summary()"></canvas></div></figure>`,
  styles: [`.chart-box { margin: 0; padding: 1rem; background: var(--bg-card); border: 1px solid var(--border-color); border-radius: 8px; min-width: 0; }
    figcaption { font-weight: 600; margin-bottom: .75rem; color: var(--text-primary); }
    .chart-canvas { position: relative; height: 320px; }
    @media (max-width: 480px) { .chart-canvas { height: 260px; } }`],
})
export class Chart implements OnDestroy {
  readonly title = input.required<string>();
  readonly labels = input.required<string[]>();
  readonly series = input.required<ChartSeries[]>();
  readonly stacked = input(false);
  readonly yTitle = input('');
  private readonly canvas = viewChild.required<ElementRef<HTMLCanvasElement>>('canvas');
  private readonly theme = inject(ThemeService);
  private chart?: ChartJs;

  constructor() {
    effect(() => {
      this.theme.isDark();
      this.render(this.labels(), this.series(), this.stacked(), this.yTitle());
    });
  }

  summary(): string {
    return this.series().map(s => `${s.label} ${s.data.join(', ')}`).join('; ');
  }

  private render(labels: string[], series: ChartSeries[], stacked: boolean, yTitle: string): void {
    this.chart?.destroy();
    const css = getComputedStyle(document.documentElement);
    const color = (name: string) => css.getPropertyValue(name).trim() || '#888';
    const text = color('--text-secondary'), grid = color('--border-color');
    const datasets = series.map(s => ({
      type: s.type ?? 'bar', label: s.label, data: s.data, order: s.type === 'line' ? 0 : 1,
      backgroundColor: color(s.color), borderColor: color(s.color), borderWidth: s.type === 'line' ? 2 : 0,
      borderRadius: s.type === 'line' ? 0 : 4, tension: .3,
    })) as ChartDataset<'bar'>[];
    this.chart = new ChartJs(this.canvas().nativeElement, {
      type: 'bar',
      data: { labels, datasets },
      options: {
        responsive: true, maintainAspectRatio: false,
        interaction: { mode: 'index', intersect: false },
        plugins: {
          legend: { position: 'bottom', labels: { color: text, usePointStyle: true, padding: 16 } },
          tooltip: { enabled: true },
        },
        scales: {
          x: { stacked, ticks: { color: text, autoSkip: true, maxRotation: 45 }, grid: { display: false } },
          y: { stacked, beginAtZero: true, ticks: { color: text, precision: 0 }, grid: { color: grid },
            title: { display: !!yTitle, text: yTitle, color: text } },
        },
      },
    });
  }

  ngOnDestroy(): void { this.chart?.destroy(); }
}
