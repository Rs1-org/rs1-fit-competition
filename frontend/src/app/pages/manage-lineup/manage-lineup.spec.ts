import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { UserStateService } from '../../core/services/user-state.service';
import { ManageLineup, lineupErrors, LineupDraft } from './manage-lineup';
import { MatchService } from '../../core/services/match.service';
import { LineupService } from '../../core/services/lineup.service';
import { PlayerService } from '../../core/services/player.service';
import { FormationService } from '../../core/services/formation.service';
import { PositionService } from '../../core/services/position.service';
import { SubPositionService } from '../../core/services/subposition.service';
import { RefereeService } from '../../core/services/referee.service';
import { HistoryService } from '../../core/services/history.service';
import { emptyPage } from '../../core/models/paged-result';
import { emptyLineup } from '../../core/models/lineup';
import { MatchDetailsDto } from '../../core/models/match';

const match: MatchDetailsDto = {
  publicId: 'm1', slug: 'derby', date: null, followersCount: 0,
  homeScore: null, awayScore: null, halfHomeScore: null, halfAwayScore: null, isPostponed: false,
  statusPublicId: 's', statusName: 'Scheduled',
  stadiumPublicId: 'st', stadiumName: 'Grbavica', stadiumSlug: 'grbavica',
  homeSelectionPublicId: 'home', homeSelectionName: 'Zeljeznicar', homeSelectionSlug: 'zeljeznicar',
  awaySelectionPublicId: 'away', awaySelectionName: 'Sarajevo', awaySelectionSlug: 'sarajevo',
  roundPublicId: null, roundName: null, roundSlug: null,
  seasonPublicId: null, seasonName: null, competitionPublicId: null, competitionName: null,
};

function page<T>(items: T[]) {
  return { ...emptyPage<T>(), items, totalCount: items.length };
}

function draft(over: Partial<LineupDraft> = {}): LineupDraft {
  return {
    playerPublicId: 'p', name: 'Player', selectionPublicId: 'home',
    included: true, isStarter: false, shirtNumber: 7, positionPublicId: '', subPositionPublicId: '',
    ...over,
  };
}

describe('lineupErrors', () => {
  it('rejects a repeated player, a shirt outside 1-99, a repeated shirt and a twelfth starter', () => {
    expect(lineupErrors([draft(), draft()])[0]).toContain('only once');
    expect(lineupErrors([draft({ shirtNumber: 0 })])[0]).toContain('1 and 99');
    expect(lineupErrors([
      draft({ playerPublicId: 'a', shirtNumber: 9 }),
      draft({ playerPublicId: 'b', shirtNumber: 9 }),
    ])).toContain('Shirt numbers must be unique for each team.');
    const starters = Array.from({ length: 12 }, (_, index) => draft({
      playerPublicId: `p${index}`, shirtNumber: index + 1, isStarter: true,
    }));
    expect(lineupErrors(starters)).toContain('A team can have at most 11 starters.');
    expect(lineupErrors([draft()])).toEqual([]);
  });
});

describe('ManageLineup', () => {
  it('lists only players linked to the selections and a referee picker', () => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { paramMap: of(convertToParamMap({ publicId: 'm1' })) } },
        { provide: MatchService, useValue: { getByPublicId: () => of(match) } },
        { provide: LineupService, useValue: { get: () => of(emptyLineup('m1')) } },
        { provide: PlayerService, useValue: { getAll: () => of(page([{ publicId: 'p1', firstName: 'Edin', lastName: 'Smailagic' }])) } },
        { provide: FormationService, useValue: { getAll: () => of(page([{ publicId: 'f1', code: '4-3-3' }])) } },
        { provide: PositionService, useValue: { getAll: () => of(emptyPage()) } },
        { provide: SubPositionService, useValue: { getAll: () => of(emptyPage()) } },
        { provide: RefereeService, useValue: { getAll: () => of(page([{ publicId: 'r1', firstName: 'Ana', lastName: 'Kovac' }])) } },
        {
          provide: HistoryService,
          useValue: {
            playerSelections: (id: string) => of(page(id === 'home' ? [{ playerPublicId: 'p1' }] : [])),
          },
        },
        { provide: UserStateService, useValue: { roles$: of(['Moderator']), hasAnyRole: () => true } },
      ],
    });
    const fixture = TestBed.createComponent(ManageLineup);
    fixture.detectChanges();
    const pageText = () => (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(pageText()).toContain('Add player');
    expect(pageText()).toContain('Ana Kovac');
    expect(pageText()).toContain('4-3-3');
    expect(pageText()).toContain('Search for a player from this selection.');

    fixture.componentInstance.addPlayer('home', { id: 'p1', label: 'Edin Smailagic' });
    fixture.detectChanges();
    expect(pageText()).toContain('Edin Smailagic');

    fixture.componentInstance.addPlayer('home', { id: 'p1', label: 'Edin Smailagic' });
    fixture.detectChanges();
    expect(pageText()).toContain('only once');

    fixture.componentInstance.addPlayer('away', { id: 'p1', label: 'Edin Smailagic' });
    fixture.detectChanges();
    expect(pageText()).toContain('Only players of that selection can be selected.');
  });
});
