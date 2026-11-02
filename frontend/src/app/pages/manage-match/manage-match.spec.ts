import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { ManageMatch } from './manage-match';
import { ToastService } from '../../core/services/toast.service';
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

function render(params: Record<string, string>, query: Record<string, string>, matches: object = {}, toast: object = {}) {
  const empty = { getAll: () => of(emptyPage()) };
  TestBed.configureTestingModule({
    imports: [ManageMatch],
    providers: [
      provideRouter([]),
      {
        provide: ActivatedRoute,
        useValue: { snapshot: { paramMap: convertToParamMap(params), queryParamMap: convertToParamMap(query) } },
      },
      { provide: MatchService, useValue: { getBySlug: () => of(), ...matches } },
      { provide: ToastService, useValue: toast },
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

  it('shows a toast and opens the match after saving', () => {
    const create = vi.fn(() => of({ slug: 'derby' }));
    const toast = { success: vi.fn() };
    const component = render({}, { stadium: 'st1' }, { create }, toast);
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    component.form.controls.date.setValue('2999-01-01T18:00');
    component.save();

    expect(create).toHaveBeenCalledTimes(1);
    expect(toast.success).toHaveBeenCalledWith('Match saved');
    expect(navigate).toHaveBeenCalledWith(['/matches', 'derby']);
  });

  it('marks fields as touched instead of saving an invalid form', () => {
    const create = vi.fn();
    const component = render({}, {}, { create });

    component.save();

    expect(create).not.toHaveBeenCalled();
    expect(component.form.controls.date.touched).toBe(true);
    expect(component.fieldError('stadiumPublicId')).toBe('This field is required.');
  });

  it('does not override the status of an edited match', () => {
    const component = render({ slug: 'derby' }, {});

    expect(component.isEditMode).toBe(true);
    expect(component.form.controls.statusPublicId.value).toBe('');
  });
});
