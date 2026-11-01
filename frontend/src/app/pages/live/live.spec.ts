import { ComponentFixture, TestBed } from '@angular/core/testing';
import { registerLocaleData } from '@angular/common';
import localeBs from '@angular/common/locales/bs-Latn';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { LiveTable } from './live';
import { StatisticService } from '../../core/services/statistic.service';
import { CompetitionService } from '../../core/services/competition.service';
import { SeasonService } from '../../core/services/season.service';
import { SelectionService } from '../../core/services/selection.service';
import { emptyPage } from '../../core/models/paged-result';

registerLocaleData(localeBs);

const page = <T>(items: T[]) => ({ ...emptyPage<T>(), items, totalCount: items.length, totalPages: 1 });

describe('LiveTable', () => {
  let fixture: ComponentFixture<LiveTable>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LiveTable],
      providers: [
        provideRouter([]),
        { provide: CompetitionService, useValue: { getAll: () => of(page([{ publicId: 'c1', name: 'Premijer liga BiH', slug: 'pl', shortName: 'PL', countryName: null, seasonCount: 1 }])) } },
        { provide: SeasonService, useValue: { getAll: () => of(page([{ publicId: 's1', name: '2026/27', slug: 's', startDate: '', endDate: '', isCurrent: true, competitionName: 'Premijer liga BiH', roundCount: 1 }])) } },
        { provide: SelectionService, useValue: { getAll: () => of(page([{ publicId: 't1', name: 'Zrinjski', abbreviation: 'ZRI', slug: 'zrinjski', pictureUrl: null, followersCount: 0, clubPublicId: null, clubName: null, clubSlug: null, countryPublicId: null, countryName: null, homeStadiumPublicId: null, stadiumName: null, stadiumSlug: null, id: '1' }])) } },
        { provide: StatisticService, useValue: { getStandings: () => of({ matchPublicIds: [], table: [
          { selectionPublicId: 't1', selectionName: 'Zrinjski', played: 4, wins: 3, draws: 0, losses: 1, goalsFor: 8, goalsAgainst: 2, points: 9 },
        ] }) } },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(LiveTable);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
  });

  it('shows the competition table and links the team', () => {
    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Live table');
    expect(text).toContain('Zrinjski');
    expect(text).toContain('9');
    const link = fixture.nativeElement.querySelector('a[href="/selection/zrinjski"]');
    expect(link).toBeTruthy();
  });
});
