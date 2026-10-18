import { Component, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, map, of, switchMap } from 'rxjs';
import { ReviewMatchDetails } from '../../core/models/match-review';
import { MatchReviewService } from '../../core/services/match-review.service';
import { PollService } from '../../core/services/poll.service';
import { apiErrorMessage } from '../../core/utils/api-error';

export const distinctPollOptions: ValidatorFn = (control: AbstractControl) => {
  const options: string[] = control.value;
  return new Set(options.map(value => value.trim().toLowerCase())).size === options.length ? null : { distinctOptions: true };
};
export const pollWindow: ValidatorFn = (control: AbstractControl) => {
  const opens: string = control.get('opensAt')?.value, closes: string = control.get('closesAt')?.value;
  if (!opens || !closes) return null;
  const from = new Date(opens).getTime(), to = new Date(closes).getTime();
  return !Number.isFinite(from) || !Number.isFinite(to) || to <= from || to <= Date.now() ? { pollWindow: true } : null;
};
@Component({ selector: 'app-poll-form', imports: [ReactiveFormsModule, RouterLink], templateUrl: './poll-form.html', styleUrl: '../match-polls/polls.css' })
export class PollForm {
  private readonly service = inject(PollService);
  private readonly matches = inject(MatchReviewService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly refresh$ = new BehaviorSubject(0);
  readonly match = signal<ReviewMatchDetails | null>(null);
  readonly loading = signal(true);
  readonly loadError = signal('');
  readonly error = signal('');
  readonly fieldErrors = signal<string[]>([]);
  readonly busy = signal(false);
  private option() { return this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(100), Validators.pattern(/\S/)]); }
  readonly form = this.fb.nonNullable.group({ question: ['', [Validators.required, Validators.maxLength(200), Validators.pattern(/\S/)]],
    opensAt: ['', Validators.required], closesAt: ['', Validators.required],
    options: this.fb.array([this.option(), this.option()], [Validators.minLength(2), Validators.maxLength(8), distinctPollOptions]),
  }, { validators: pollWindow });
  constructor() {
    combineLatest([this.route.paramMap, this.refresh$]).pipe(switchMap(([params]) => {
      this.loading.set(true); this.loadError.set('');
      return this.matches.getMatch(params.get('matchPublicId') ?? '').pipe(map(data => ({ data, error: '' })),
        catchError(err => of({ data: null, error: apiErrorMessage(err, 'The match could not be loaded.') })));
    }), takeUntilDestroyed()).subscribe(result => { this.loading.set(false); this.match.set(result.data); this.loadError.set(result.error); });
  }
  retry(): void { this.refresh$.next(this.refresh$.value + 1); }
  addOption(): void { if (!this.busy() && this.form.controls.options.length < 8) this.form.controls.options.push(this.option()); }
  removeOption(index: number): void { if (!this.busy() && this.form.controls.options.length > 2) this.form.controls.options.removeAt(index); }
  save(): void {
    const match = this.match(); if (!match || this.loading() || this.loadError() || this.busy()) return;
    this.form.markAllAsTouched(); this.form.updateValueAndValidity(); this.error.set(''); this.fieldErrors.set([]);
    if (this.form.invalid) { this.error.set('Enter a question, 2–8 different options and a future closing time after the opening time.'); return; }
    const value = this.form.getRawValue();
    this.busy.set(true);
    this.service.create(match.publicId, { question: value.question.trim(), options: value.options.map(x => x.trim()),
      opensAt: new Date(value.opensAt).toISOString(), closesAt: new Date(value.closesAt).toISOString() }).subscribe({
      next: () => { this.busy.set(false); void this.router.navigate(['/live-matches', match.publicId]); },
      error: err => { this.busy.set(false); this.error.set(apiErrorMessage(err, 'The poll could not be created.'));
        if (Array.isArray(err.error?.errors)) this.fieldErrors.set(err.error.errors.map((e: { errorMessage: string }) => e.errorMessage)); },
    });
  }
}
