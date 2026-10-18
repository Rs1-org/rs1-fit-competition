import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, combineLatest, forkJoin, map, of, switchMap, throwError } from 'rxjs';
import { Statistic, statisticMetrics } from '../../core/models/statistic';
import { ReviewMatchDetails } from '../../core/models/match-review';
import { StatisticService } from '../../core/services/statistic.service';
import { MatchReviewService } from '../../core/services/match-review.service';
import { apiErrorMessage } from '../../core/utils/api-error';
import { completeTeam, possessionTotal, statisticTeamForm } from './statistic-validators';

@Component({ selector: 'app-statistic-form', imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './statistic-form.html', styleUrl: '../../pages/statistics/statistics.css' })
export class StatisticForm {
  private readonly service = inject(StatisticService);
  private readonly matches = inject(MatchReviewService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly refresh$ = new BehaviorSubject(0);
  readonly metrics = statisticMetrics;
  readonly sides = ['home', 'away'] as const;
  readonly match = signal<ReviewMatchDetails | null>(null);
  readonly existing = signal<Statistic | null>(null);
  readonly loading = signal(true);
  readonly loadError = signal('');
  readonly error = signal('');
  readonly fieldErrors = signal<Record<string, string>>({});
  readonly busy = signal(false);
  readonly form = this.fb.group({ home: statisticTeamForm(this.fb), away: statisticTeamForm(this.fb) }, { validators: possessionTotal });
  constructor() {
    combineLatest([this.route.paramMap, this.refresh$]).pipe(switchMap(([params]) => {
      const id = params.get('matchPublicId') ?? '';
      this.loading.set(true); this.loadError.set(''); this.error.set(''); this.match.set(null); this.existing.set(null); this.fieldErrors.set({});
      return forkJoin({ match: this.matches.getMatch(id), statistic: this.service.getForMatch(id).pipe(
        catchError(err => err instanceof HttpErrorResponse && err.status === 404 ? of(null) : throwError(() => err)),
      ) }).pipe(map(data => ({ data, error: '' })), catchError(err => of({ data: null, error: apiErrorMessage(err, 'The statistics form could not be loaded.') })));
    }), takeUntilDestroyed()).subscribe(result => {
      this.loading.set(false); this.loadError.set(result.error); this.form.reset();
      if (!result.data) return;
      this.match.set(result.data.match); this.existing.set(result.data.statistic);
      if (result.data.statistic) this.form.patchValue({ home: result.data.statistic.home ?? {}, away: result.data.statistic.away ?? {} });
    });
  }
  retry(): void { this.refresh$.next(this.refresh$.value + 1); }
  hasTeams(): boolean { const match = this.match(); return !!match?.homeSelectionPublicId && !!match.awaySelectionPublicId && match.homeSelectionPublicId !== match.awaySelectionPublicId; }
  save(): void {
    if (this.busy() || this.loading() || this.loadError() || !this.hasTeams()) return;
    this.form.markAllAsTouched(); this.error.set(''); this.fieldErrors.set({});
    const values = this.form.getRawValue();
    if (this.form.invalid || !completeTeam(values.home) || !completeTeam(values.away)) { this.error.set('Check the highlighted statistics before saving.'); return; }
    const match = this.match(); if (!match) return;
    const input = { home: values.home, away: values.away }, existing = this.existing();
    this.busy.set(true);
    (existing ? this.service.update(existing.publicId, input) : this.service.create(match.publicId, input)).subscribe({
      next: () => { this.busy.set(false); void this.router.navigate(['/match-statistics', match.publicId]); },
      error: err => {
        this.busy.set(false); this.error.set(apiErrorMessage(err, 'Statistics could not be saved.'));
        if (Array.isArray(err.error?.errors)) this.fieldErrors.set(Object.fromEntries(err.error.errors.map((e: { propertyName: string; errorMessage: string }) => [e.propertyName.toLowerCase(), e.errorMessage])));
      },
    });
  }
}
