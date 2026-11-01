import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ChartConfiguration } from 'chart.js';
import { ThemeService } from '../../../core/services/theme.service';
import { Chart, chartFactory, ChartSeries } from './chart';

describe('Chart', () => {
  const series: ChartSeries[] = [{ label: 'Goals', data: [3, 5], color: '--ac' }, { label: 'Per match', data: [1.5, 2.5], color: '--dg', type: 'line', axis: 'right' }];
  const isDark = signal(false);
  const destroy = vi.fn();
  let create: ReturnType<typeof vi.fn>;

  function render(labels: string[]) {
    TestBed.configureTestingModule({ providers: [{ provide: ThemeService, useValue: { isDark } }] });
    const fixture = TestBed.createComponent(Chart);
    fixture.componentRef.setInput('title', 'Goals per round');
    fixture.componentRef.setInput('labels', labels);
    fixture.componentRef.setInput('series', series);
    fixture.detectChanges();
    return fixture;
  }
  const lastConfig = () => create.mock.calls.at(-1)![1] as ChartConfiguration;

  beforeEach(() => {
    isDark.set(false); destroy.mockReset();
    document.documentElement.style.setProperty('--ac', '#111111');
    create = vi.fn().mockReturnValue({ destroy, update: vi.fn() });
    vi.spyOn(chartFactory, 'create').mockImplementation(create as never);
  });
  afterEach(() => { vi.restoreAllMocks(); document.documentElement.style.removeProperty('--ac'); });

  it('redraws with the new token colours when the theme changes', () => {
    const fixture = render(['Round 1', 'Round 2']);
    expect(lastConfig().data.datasets[0].backgroundColor).toBe('#111111');
    document.documentElement.style.setProperty('--ac', '#222222');
    isDark.set(true);
    fixture.detectChanges();
    expect(destroy).toHaveBeenCalledOnce();
    expect(lastConfig().data.datasets[0].backgroundColor).toBe('#222222');
  });

  it('shows its own empty state and no canvas when there is no data', () => {
    const element: HTMLElement = render([]).nativeElement;
    expect(element.querySelector('canvas')).toBeNull();
    expect(element.textContent).toContain('Goals per round');
    expect(element.textContent).toContain('No statistics recorded for this season yet.');
    expect(create).not.toHaveBeenCalled();
  });

  it('describes every label and value for screen readers', () => {
    const element: HTMLElement = render(['Round 1', 'Round 2']).nativeElement;
    const canvas = element.querySelector('canvas')!;
    const description = element.querySelector(`#${canvas.getAttribute('aria-describedby')}`);
    expect(description?.textContent).toBe('Round 1: Goals 3, Per match 1.5. Round 2: Goals 5, Per match 2.5');
  });

  it('uses a right axis for marked series and swaps axes when horizontal', () => {
    const fixture = render(['Round 1']);
    expect(lastConfig().options?.scales?.['y1']).toBeTruthy();
    fixture.componentRef.setInput('horizontal', true);
    fixture.detectChanges();
    expect(lastConfig().options?.indexAxis).toBe('y');
  });

  it('destroys the chart with the component', () => {
    const fixture = render(['Round 1']);
    fixture.destroy();
    expect(destroy).toHaveBeenCalledOnce();
  });
});
