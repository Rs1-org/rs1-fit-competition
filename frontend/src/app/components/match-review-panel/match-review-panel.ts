import { Component, inject, Input, OnChanges, signal, ViewChild } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, filter, map, of, startWith, switchMap, tap } from 'rxjs';
import { MatchReviewService } from '../../core/services/match-review.service';
import { UserStateService } from '../../core/services/user-state.service';
import { MatchReviewPage, Review } from '../../core/models/match-review';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { Pagination } from '../../shared/components/pagination/pagination';
import { AppDateTimePipe, AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { AuthModalComponent } from '../../features/auth/auth-modal/auth-modal.component';
import { apiErrorMessage } from '../../core/utils/api-error';

interface ReviewState { loading: boolean; data: MatchReviewPage | null; error: string; }

@Component({
  selector: 'app-match-review-panel',
  imports: [ReactiveFormsModule, Pagination, AppDateTimePipe, AppNumberPipe, AuthModalComponent],
  templateUrl: './match-review-panel.html', styleUrl: '../../pages/match-reviews/reviews.css',
})
export class MatchReviewPanel implements OnChanges {
  @Input({ required: true }) matchPublicId = '';
  @ViewChild(AuthModalComponent) authModal?: AuthModalComponent;
  private readonly service = inject(MatchReviewService);
  private readonly user = inject(UserStateService);
  private readonly confirm = inject(ConfirmDialogService);
  private readonly fb = inject(FormBuilder);
  readonly viewer = toSignal(this.user.user$, { initialValue: null });
  readonly ownId = signal<string | null>(null);
  readonly busy = signal(false);
  readonly actionError = signal('');
  readonly fieldErrors = signal<Record<string, string>>({});
  readonly form = this.fb.group({
    rating: this.fb.control<number | null>(null, [Validators.required, Validators.min(1), Validators.max(5), Validators.pattern(/^[1-5]$/)]),
    comment: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(2000), Validators.pattern(/\S/)]),
  });
  private readonly query$ = new BehaviorSubject({ id: '', page: 1 });
  readonly state = toSignal(combineLatest([
    this.query$,
    this.user.user$.pipe(tap(() => { this.form.reset(); this.ownId.set(null); })),
  ]).pipe(
    filter(([query]) => !!query.id),
    switchMap(([query]) => this.service.getReviews(query.id, query.page).pipe(
      tap(data => {
        if (this.ownId() !== (data.myReview?.publicId ?? null) || !this.form.dirty) {
          this.ownId.set(data.myReview?.publicId ?? null);
          this.form.reset({ rating: data.myReview?.rating ?? null, comment: data.myReview?.comment ?? '' });
        }
      }),
      map((data): ReviewState => ({ loading: false, data, error: '' })),
      startWith({ loading: true, data: null, error: '' } as ReviewState),
      catchError(err => of({ loading: false, data: null, error: apiErrorMessage(err, 'Reviews could not be loaded.') } as ReviewState)),
    )),
  ), { initialValue: { loading: true, data: null, error: '' } as ReviewState });

  ngOnChanges(): void { this.form.reset(); this.ownId.set(null); this.actionError.set(''); this.query$.next({ id: this.matchPublicId, page: 1 }); }
  page(page: number): void { this.query$.next({ id: this.matchPublicId, page }); }
  retry(): void { this.query$.next({ ...this.query$.value }); }
  login(): void { this.authModal?.openLogin(); }
  save(): void {
    if (!this.viewer() || this.busy() || this.state().loading || this.state().error) return;
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    const value = this.form.getRawValue();
    const input = { rating: value.rating!, comment: value.comment.trim() };
    this.busy.set(true); this.actionError.set(''); this.fieldErrors.set({});
    const ownId = this.ownId();
    (ownId ? this.service.update(ownId, input) : this.service.create(this.matchPublicId, input)).subscribe({
      next: () => { this.busy.set(false); this.form.markAsPristine(); this.page(1); },
      error: err => {
        this.failed(err);
        const errors = err.error?.errors;
        if (Array.isArray(errors)) this.fieldErrors.set(Object.fromEntries(errors.map((e: { propertyName: string; errorMessage: string }) => [e.propertyName.toLowerCase(), e.errorMessage])));
      },
    });
  }
  async remove(): Promise<void> {
    const id = this.ownId();
    if (!this.viewer() || !id || this.busy()) return;
    this.busy.set(true);
    this.actionError.set('');
    if (!await this.confirm.confirm({ title: 'Delete your review', message: 'Delete your rating and review for this match?', confirmLabel: 'Delete review' })) { this.busy.set(false); return; }
    this.service.delete(id).subscribe({
      next: () => { this.busy.set(false); this.form.reset(); this.ownId.set(null); this.page(1); }, error: err => this.failed(err),
    });
  }
  react(review: Review, value: -1 | 1): void {
    if (!this.viewer() || review.isMine || this.busy()) return;
    this.busy.set(true); this.actionError.set('');
    this.service.react(review.publicId, value).subscribe({ next: () => { this.busy.set(false); this.retry(); }, error: err => this.failed(err) });
  }
  private failed(error: unknown): void { this.busy.set(false); this.actionError.set(apiErrorMessage(error, 'The review could not be saved.')); }
}
