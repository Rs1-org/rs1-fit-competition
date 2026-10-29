import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { ManageMatch } from './manage-match';
import { MatchService } from '../../core/services/match.service';
import { MatchStatusService } from '../../core/services/match-status.service';
import { StadiumService } from '../../core/services/stadium.service';
import { SelectionService } from '../../core/services/selection.service';
import { CompetitionService } from '../../core/services/competition.service';
import { SeasonService } from '../../core/services/season.service';
import { RoundService } from '../../core/services/round.service';
import { emptyPage, PagedResult } from '../../core/models/paged-result';

function page<T>(items: T[]): PagedResult<T> {
  return { ...emptyPage<T>(), items, totalCount: items.length, totalPages: 1 };
}

function render(params: Record<string, string>, query: Record<string, string>) {
  const empty = { getAll: () => of(emptyPage()) };
  TestBed.configureTestingModule({
    imports: [ManageMatch],
    providers: [
      provideRouter([]),
      {
        provide: ActivatedRoute,
        useValue: { snapshot: { paramMap: convertToParamMap(params), queryParamMap: convertToParamMap(query) } },
      },
      { provide: MatchService, useValue: { getBySlug: () => of() } },
      {
        provide: MatchStatusService,
        useValue: { getAll: () => of(page([{ publicId: 'live', name: 'Live' }, { publicId: 'sched', name: 'Scheduled' }])) },
      },
      { provide: StadiumService, useValue: empty },
      { provide: SelectionService, useValue: empty },
      { provide: CompetitionService, useValue: empty },
      { provide: SeasonService, useValue: empty },
      { provide: RoundService, useValue: empty },
    ],
  });
  const fixture = TestBed.createComponent(ManageMatch);
  fixture.detectChanges();
  return fixture.componentInstance;
}

describe('ManageMatch', () => {
  it('picks the stadium from the query and defaults a new match to Scheduled', () => {
    const component = render({}, { stadium: 'st1' });

    expect(component.form.controls.stadiumPublicId.value).toBe('st1');
    expect(component.form.controls.statusPublicId.value).toBe('sched');
  });

  it('does not override the status of an edited match', () => {
    const component = render({ slug: 'derby' }, {});

    expect(component.isEditMode).toBe(true);
    expect(component.form.controls.statusPublicId.value).toBe('');
  });
});
