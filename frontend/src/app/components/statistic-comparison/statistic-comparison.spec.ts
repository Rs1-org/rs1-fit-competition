import { registerLocaleData } from '@angular/common';
import localeBs from '@angular/common/locales/bs-Latn';
import { TestBed } from '@angular/core/testing';
import { StatisticComparison } from './statistic-comparison';
import { Statistic } from '../../core/models/statistic';

registerLocaleData(localeBs);
describe('Statistic comparison rendering', () => {
  it('shows real zeroes and regional numbers, and labels missing teams without inventing values', () => {
    const fixture = TestBed.createComponent(StatisticComparison);
    const statistic: Statistic = { publicId: 'a'.repeat(32), matchPublicId: 'b'.repeat(32), matchDate: '', matchSlug: null,
      home: { publicId: 'c'.repeat(32), selectionPublicId: 'd'.repeat(32), selectionName: 'Home team',
        totalShots: 1234, shotsOnTarget: 0, totalRedCards: 0, totalYellowCards: 0, corners: 0, bigChances: 0, possession: 50 }, away: null };
    fixture.componentRef.setInput('statistic', statistic); fixture.detectChanges();
    const text = (fixture.nativeElement as HTMLElement).textContent;
    expect(text).toContain('Home team'); expect(text).toContain('1.234'); expect(text).toContain('Not recorded');
    const targetRow = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('tr')).find(row => row.textContent?.includes('Shots on target'));
    expect(targetRow?.querySelector('td')?.textContent?.trim()).toBe('0');
  });
});
