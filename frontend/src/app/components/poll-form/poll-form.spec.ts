import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { BehaviorSubject, of, Subject } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { PollForm } from './poll-form';
import { PollService } from '../../core/services/poll.service';
import { MatchReviewService } from '../../core/services/match-review.service';
import { routes } from '../../app.routes';
import { AdminModeratorGuard } from '../../core/guards/admin-moderator.guard';

describe('Moderator poll form', () => {
  const id = 'a'.repeat(32), create = vi.fn(), navigate = vi.fn(); let component: PollForm;
  const local = (date: Date) => new Date(date.getTime() - date.getTimezoneOffset() * 60000).toISOString().slice(0, 16);
  beforeEach(() => {
    vi.useFakeTimers(); vi.setSystemTime(new Date('2026-10-18T12:00:00Z'));
    create.mockReset().mockReturnValue(of({ publicId: 'b'.repeat(32) })); navigate.mockReset().mockResolvedValue(true);
    TestBed.configureTestingModule({ providers: [
      { provide: ActivatedRoute, useValue: { paramMap: new BehaviorSubject(convertToParamMap({ matchPublicId: id })) } },
      { provide: Router, useValue: { navigate } }, { provide: PollService, useValue: { create } },
      { provide: MatchReviewService, useValue: { getMatch: () => of({ publicId: id }) } },
    ] });
    component = TestBed.runInInjectionContext(() => new PollForm());
    component.form.setValue({ question: ' Who wins? ', options: [' Home ', 'Away'], opensAt: local(new Date(Date.now() - 60000)), closesAt: local(new Date(Date.now() + 3600000)) });
  });
  afterEach(() => { vi.clearAllTimers(); vi.useRealTimers(); });
  it('validates distinct choices, schedule and the option-count limits', () => {
    component.form.controls.options.setValue(['Home', ' home ']); component.save(); expect(create).not.toHaveBeenCalled();
    component.form.controls.options.setValue(['Home', 'Away']); component.form.controls.closesAt.setValue(component.form.controls.opensAt.value); component.save();
    expect(create).not.toHaveBeenCalled();
    for (let index = 0; index < 10; index++) component.addOption(); expect(component.form.controls.options.length).toBe(8);
    for (let index = 0; index < 10; index++) component.removeOption(0); expect(component.form.controls.options.length).toBe(2);
  });
  it('submits trimmed text and ISO timestamps behind the management guard', () => {
    const value = component.form.getRawValue(); component.save();
    expect(create).toHaveBeenCalledExactlyOnceWith(id, { question: 'Who wins?', options: ['Home', 'Away'],
      opensAt: new Date(value.opensAt).toISOString(), closesAt: new Date(value.closesAt).toISOString() });
    expect(navigate).toHaveBeenCalledWith(['/live-matches', id]);
    expect(routes.find(route => route.path === 'manage-polls/:matchPublicId')?.canActivate).toContain(AdminModeratorGuard);
  });
  it('blocks duplicate saves and keeps input after a backend validation error', () => {
    const response = new Subject<{ publicId: string }>(); create.mockReturnValue(response); component.save(); component.save(); expect(create).toHaveBeenCalledOnce();
    response.error(new HttpErrorResponse({ status: 400, error: { errors: [{ propertyName: 'ClosesAt', errorMessage: 'Choose a future closing time.' }] } }));
    expect(component.form.controls.question.value).toBe(' Who wins? '); expect(component.fieldErrors()).toEqual(['Choose a future closing time.']); expect(component.busy()).toBe(false);
  });
});
