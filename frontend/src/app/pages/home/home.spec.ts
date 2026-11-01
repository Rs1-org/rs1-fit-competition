import { ComponentFixture, TestBed } from '@angular/core/testing';
import { registerLocaleData } from '@angular/common';
import localeBs from '@angular/common/locales/bs-Latn';
import { provideRouter } from '@angular/router';

registerLocaleData(localeBs);
import { of } from 'rxjs';
import { Home, isSameDay, startOfDay, tuesdayToSaturday, isOnPitch, isFinished } from './home';
import { MatchService } from '../../core/services/match.service';
import { StatisticService } from '../../core/services/statistic.service';
import { CompetitionService } from '../../core/services/competition.service';
import { SelectionService } from '../../core/services/selection.service';
import { emptyPage } from '../../core/models/paged-result';
import { MatchListDto } from '../../core/models/match';

describe('Home', () => {
  let fixture: ComponentFixture<Home>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Home],
      providers: [
        provideRouter([]),
        { provide: MatchService, useValue: { getAll: () => of(emptyPage()) } },
        { provide: StatisticService, useValue: { getCharts: () => of({ goalsPerRound: [], teamResults: [], teamShots: [] }) } },
        { provide: CompetitionService, useValue: { getAll: () => of(emptyPage()) } },
        { provide: SelectionService, useValue: { getAll: () => of(emptyPage()) } },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(Home);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
  });

  it('shows the dashboard heading and keeps favourites hidden', () => {
    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Matchday');
    expect(text).toContain('Live now');
    expect(text).not.toContain('Your teams');
  });
});

describe('home dates', () => {
  const match = (patch: Partial<MatchListDto>): MatchListDto => ({
    publicId: 'a', slug: 'a', date: '2026-07-19T18:00:00', homeScore: 1, awayScore: 0, isPostponed: false,
    statusName: 'Finished', stadiumName: null, homeSelectionName: 'Home', awaySelectionName: 'Away',
    roundPublicId: null, roundName: 'Round 1', seasonName: '2025/26', competitionName: 'League',
    ...patch,
  });

  it('builds Tuesday to Saturday of the selected week', () => {
    const days = tuesdayToSaturday(new Date(2026, 9, 29));
    expect(days.map((day) => day.getDay())).toEqual([2, 3, 4, 5, 6]);
    expect(isSameDay(days[2], startOfDay(new Date(2026, 9, 29)))).toBe(true);
  });

  it('treats live and half-time as on the pitch, and a scored match as finished', () => {
    expect(isOnPitch(match({ statusName: 'Live', homeScore: null, awayScore: null }))).toBe('live');
    expect(isOnPitch(match({ statusName: 'Half-time' }))).toBe('half-time');
    expect(isOnPitch(match({ statusName: 'Finished' }))).toBeNull();
    expect(isFinished(match({ statusName: 'Scheduled', homeScore: null, awayScore: null }))).toBe(false);
  });
});
