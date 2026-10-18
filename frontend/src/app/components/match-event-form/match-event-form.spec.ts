import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { BehaviorSubject, of, Subject } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { MatchEventForm } from './match-event-form';
import { MatchEventService } from '../../core/services/match-event.service';
import { EventContext, EventLookups } from '../../core/models/match-event';

describe('Event form behavior', () => {
  const id = (c: string) => c.repeat(32);
  const lookups: EventLookups = { eventTypes: [
    { publicId: id('b'), code: 'goal', name: 'Goal', sortOrder: 0 },
    { publicId: id('c'), code: 'substitution', name: 'Substitution', sortOrder: 1 },
  ], matchParts: [{ publicId: id('d'), code: 'first-half', name: '1st half', sortOrder: 0 }] };
  const context: EventContext = { matchPublicId: id('a'), homeName: 'Home', awayName: 'Away', selections: [
    { publicId: id('e'), name: 'Home', players: [{ publicId: id('f'), name: 'First' }, { publicId: id('1'), name: 'Second' }] },
  ] };
  const create = vi.fn(), update = vi.fn(), navigate = vi.fn();
  let component: MatchEventForm;
  beforeEach(() => {
    create.mockReset().mockReturnValue(of({ publicId: id('2') })); update.mockReset(); navigate.mockReset().mockResolvedValue(true);
    TestBed.configureTestingModule({ providers: [
      { provide: ActivatedRoute, useValue: { paramMap: new BehaviorSubject(convertToParamMap({ id: 'new' })), queryParamMap: new BehaviorSubject(convertToParamMap({ match: id('a') })) } },
      { provide: Router, useValue: { navigate } },
      { provide: MatchEventService, useValue: { lookups: () => of(lookups), context: () => of(context), create, update } },
    ] });
    component = TestBed.runInInjectionContext(() => new MatchEventForm());
    component.form.patchValue({ eventTypePublicId: id('b'), matchPartPublicId: id('d'), selectionPublicId: id('e'), minute: 45, addedMinute: 2 });
  });
  it('mirrors minute and required-reference validation before sending', () => {
    component.form.controls.minute.setValue(131); component.save();
    component.form.controls.minute.setValue(45.5); component.save();
    component.form.controls.minute.setValue(45); component.form.controls.matchPartPublicId.setValue(''); component.save();
    expect(create).not.toHaveBeenCalled(); expect(component.form.invalid).toBe(true);
  });
  it('requires two different substitution players and clears players on team change', () => {
    component.form.controls.eventTypePublicId.setValue(id('c')); component.save(); expect(create).not.toHaveBeenCalled();
    component.form.patchValue({ playerPublicId: id('f'), relatedPlayerPublicId: id('f') }); component.save(); expect(create).not.toHaveBeenCalled();
    component.form.controls.relatedPlayerPublicId.setValue(id('1')); component.save();
    expect(create).toHaveBeenCalledOnce(); expect(create.mock.calls[0][0].relatedPlayerPublicId).toBe(id('1'));
    component.form.controls.selectionPublicId.setValue(''); expect(component.form.controls.playerPublicId.value).toBe('');
    expect(component.form.controls.relatedPlayerPublicId.value).toBe('');
  });
  it('prevents duplicate submissions and retains draft with backend errors', () => {
    const response = new Subject<{ publicId: string }>(); create.mockReturnValue(response);
    component.form.controls.note.setValue(' A note '); component.save(); component.save();
    expect(create).toHaveBeenCalledOnce(); expect(create.mock.calls[0][0]).toMatchObject({ note: 'A note', playerPublicId: null, relatedPlayerPublicId: null, matchPublicId: id('a') });
    response.error(new HttpErrorResponse({ status: 400, error: { errors: [{ propertyName: 'SelectionPublicId', errorMessage: 'Team changed.' }] } }));
    expect(component.form.controls.note.value).toBe(' A note '); expect(component.fieldErrors()).toEqual(['Team changed.']);
    expect(component.busy()).toBe(false); expect(navigate).not.toHaveBeenCalled();
  });
});
