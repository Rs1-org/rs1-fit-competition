import { registerLocaleData } from '@angular/common';
import localeBs from '@angular/common/locales/bs-Latn';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { LeagueTable, positionZone } from './league-table';
import { TeamResults } from '../../../core/models/statistic';

registerLocaleData(localeBs);

function team(id: string, points: number, goalsFor = 10, goalsAgainst = 8): TeamResults {
  return { selectionPublicId: id, selectionName: `Team ${id}`, played: 10, wins: 3, draws: 2, losses: 5, goalsFor, goalsAgainst, points };
}

function render(inputs: Record<string, unknown>) {
  TestBed.configureTestingModule({ imports: [LeagueTable], providers: [provideRouter([])] });
  const fixture = TestBed.createComponent(LeagueTable);
  for (const [key, value] of Object.entries(inputs)) fixture.componentRef.setInput(key, value);
  fixture.detectChanges();
  return fixture.nativeElement as HTMLElement;
}

describe('positionZone', () => {
  it('marks the top three and the bottom two of a full league', () => {
    expect([0, 1, 2, 3, 6, 7].map((index) => positionZone(index, 8)))
      .toEqual(['champions', 'conference', 'conference', null, 'relegation', 'relegation']);
  });

  it('leaves small tables without zones', () => {
    expect(positionZone(0, 4)).toBeNull();
  });
});

describe('LeagueTable', () => {
  const rows = ['a', 'b', 'c', 'd', 'e', 'f'].map((id, index) => team(id, 20 - index * 3, 12 - index, 6 + index));

  it('links teams it knows and shows the goal difference with a sign', () => {
    const element = render({ rows, teams: [{ publicId: 'a', slug: 'team-a', abbreviation: 'TMA' }] });

    expect(element.querySelector('a[href="/selection/team-a"]')?.textContent).toContain('Team a');
    expect(element.querySelectorAll('tbody a').length).toBe(1);
    expect(element.querySelector('tbody tr td.plus')?.textContent?.trim()).toBe('+6');
    expect(element.querySelector('tbody tr:last-child td.minus')?.textContent?.trim()).toBe('-4');
  });

  it('colours the places and shows the legend only when asked', () => {
    const plain = render({ rows });
    expect(plain.querySelector('.legend')).toBeNull();
    expect(plain.querySelector('.pos-badge.champions')).toBeNull();

    TestBed.resetTestingModule();
    const zoned = render({ rows, showZones: true });
    expect(zoned.querySelector('.legend')?.textContent).toContain('Relegation');
    expect(zoned.querySelectorAll('.pos-badge.relegation').length).toBe(2);
  });

  it('adds the form column with chips when form is given', () => {
    const element = render({ rows, form: { a: ['W', 'D', 'L'] } });

    expect(element.querySelector('thead')?.textContent).toContain('Form');
    expect(element.querySelector('tbody tr app-form-chips')?.textContent).toContain('WDL');
  });

  it('highlights the given team', () => {
    const element = render({ rows, highlight: 'b' });

    expect(element.querySelectorAll('tr.highlight').length).toBe(1);
    expect(element.querySelector('tr.highlight')?.textContent).toContain('Team b');
  });
});
