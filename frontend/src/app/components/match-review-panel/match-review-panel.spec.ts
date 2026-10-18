import { TestBed } from '@angular/core/testing';
import { BehaviorSubject, of, Subject, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { MatchReviewPanel } from './match-review-panel';
import { MatchReviewService } from '../../core/services/match-review.service';
import { UserStateService } from '../../core/services/user-state.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { MatchReviewPage, Review } from '../../core/models/match-review';
import { emptyPage } from '../../core/models/paged-result';

describe('Match review interactions', () => {
  const own: Review = { publicId: 'b'.repeat(32), rating: 4, comment: 'Existing review', authorName: 'Dev T.',
    createdAt: '2026-01-01T12:00:00Z', isMine: true, likeCount: 0, dislikeCount: 0, myReaction: null };
  const getReviews = vi.fn(), create = vi.fn(), update = vi.fn(), remove = vi.fn(), react = vi.fn(), confirm = vi.fn();
  let component: MatchReviewPanel;
  const summary = (myReview: Review | null = null): MatchReviewPage => ({
    averageRating: myReview ? 4 : null, reviewCount: myReview ? 1 : 0,
    reviews: { ...emptyPage<Review>(), items: myReview ? [myReview] : [], totalCount: myReview ? 1 : 0 }, myReview,
  });
  beforeEach(() => {
    getReviews.mockReset().mockReturnValue(of(summary()));
    create.mockReset().mockReturnValue(of({ publicId: own.publicId }));
    update.mockReset().mockReturnValue(of({ publicId: own.publicId }));
    remove.mockReset().mockReturnValue(of({ publicId: own.publicId }));
    react.mockReset().mockReturnValue(of({ value: 1 })); confirm.mockReset().mockResolvedValue(true);
    TestBed.configureTestingModule({ providers: [
      { provide: MatchReviewService, useValue: { getReviews, create, update, delete: remove, react } },
      { provide: UserStateService, useValue: { user$: new BehaviorSubject({ email: 'dev@example.invalid' }) } },
      { provide: ConfirmDialogService, useValue: { confirm } },
    ] });
    component = TestBed.runInInjectionContext(() => new MatchReviewPanel());
    component.matchPublicId = 'a'.repeat(32); component.ngOnChanges();
  });

  it('requires an explicit 1–5 rating and nonblank comment before sending', () => {
    component.form.setValue({ rating: null, comment: 'Comment' }); component.save();
    component.form.setValue({ rating: 4, comment: '   ' }); component.save();
    expect(create).not.toHaveBeenCalled(); expect(component.form.invalid).toBe(true);
  });

  it('blocks duplicate saves and retains the draft and inline messages after validation failure', () => {
    const response = new Subject<{ publicId: string }>(); create.mockReturnValue(response);
    component.form.setValue({ rating: 4, comment: ' My review ' }); component.form.markAsDirty();
    component.save(); component.save();
    expect(create).toHaveBeenCalledExactlyOnceWith('a'.repeat(32), { rating: 4, comment: 'My review' });
    response.error(new HttpErrorResponse({ status: 400, error: { errors: [{ propertyName: 'Comment', errorMessage: 'Review rejected.' }] } }));
    expect(component.form.controls.comment.value).toBe(' My review ');
    expect(component.fieldErrors()['comment']).toBe('Review rejected.'); expect(component.busy()).toBe(false);
  });

  it('preserves an unsaved edit across reaction/page reloads and sends public review identifiers', () => {
    getReviews.mockReturnValue(of(summary(own))); component.retry();
    component.form.controls.comment.setValue('Unsaved edit'); component.form.markAsDirty();
    component.react({ ...own, publicId: 'c'.repeat(32), isMine: false }, -1);
    expect(react).toHaveBeenCalledExactlyOnceWith('c'.repeat(32), -1);
    expect(component.form.controls.comment.value).toBe('Unsaved edit');
    component.react(own, 1); expect(react).toHaveBeenCalledOnce();
    component.save(); expect(update).toHaveBeenCalledWith(own.publicId, { rating: 4, comment: 'Unsaved edit' });
  });

  it('requires confirmation for deletion and supports loading retry', async () => {
    getReviews.mockReturnValue(of(summary(own))); component.retry();
    confirm.mockResolvedValue(false); await component.remove(); expect(remove).not.toHaveBeenCalled();
    confirm.mockResolvedValue(true); await component.remove(); expect(remove).toHaveBeenCalledExactlyOnceWith(own.publicId);
    getReviews.mockReturnValueOnce(throwError(() => new Error('offline'))); component.retry();
    expect(component.state().data).toBeNull(); expect(component.state().error).toBeTruthy();
    component.retry(); expect(component.state().error).toBe('');
  });
});
