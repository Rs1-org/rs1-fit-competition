import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { EMPTY, Observable, catchError, distinctUntilChanged, finalize, of, switchMap } from 'rxjs';
import { MatchService } from '../../core/services/match.service';
import { MatchStatusService } from '../../core/services/match-status.service';
import { StadiumService } from '../../core/services/stadium.service';
import { SelectionOption, SelectionService } from '../../core/services/selection.service';
import { CompetitionService } from '../../core/services/competition.service';
import { SeasonService } from '../../core/services/season.service';
import { RoundService } from '../../core/services/round.service';
import { MatchStatusListDto } from '../../core/models/match-status';
import { StadiumListDto } from '../../core/models/stadium';
import { CompetitionListDto } from '../../core/models/competition';
import { SeasonListDto } from '../../core/models/season';
import { RoundListDto } from '../../core/models/round';
import { MatchDetailsDto, SaveMatchDto } from '../../core/models/match';
import { PagedResult } from '../../core/models/paged-result';
import { fetchAllPages, MAX_PAGE_SIZE } from '../../core/helpers/paging.helper';
import { apiErrorMessage } from '../../core/utils/api-error';
import {
  futureDateValidator,
  scoresValidator,
  teamsDifferValidator,
  toLocalInputValue,
} from './match-form.validators';

type FieldName =
  | 'date'
  | 'statusPublicId'
  | 'stadiumPublicId'
  | 'homeSelectionPublicId'
  | 'awaySelectionPublicId'
  | 'roundPublicId'
  | 'followersCount'
  | 'homeScore'
  | 'awayScore'
  | 'halfHomeScore'
  | 'halfAwayScore';

@Component({
  selector: 'app-manage-match',
  imports: [ReactiveFormsModule],
  templateUrl: './manage-match.html',
  styleUrls: ['../manage-players/manage-players.css', '../manage-formation/manage-formation.css'],
})
export class ManageMatch implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);
  private fb = inject(FormBuilder);
  private matches = inject(MatchService);
  private statuses = inject(MatchStatusService);
  private stadiums = inject(StadiumService);
  private selections = inject(SelectionService);
  private competitions = inject(CompetitionService);
  private seasons = inject(SeasonService);
  private rounds = inject(RoundService);

  form = this.fb.nonNullable.group(
    {
      date: ['', Validators.required],
      statusPublicId: ['', Validators.required],
      stadiumPublicId: ['', Validators.required],
      homeSelectionPublicId: [''],
      awaySelectionPublicId: [''],
      competitionPublicId: [''],
      seasonPublicId: [''],
      roundPublicId: [''],
      followersCount: [0, [Validators.required, Validators.min(0)]],
      isPostponed: [false],
      homeScore: this.fb.control<number | null>(null, Validators.min(0)),
      awayScore: this.fb.control<number | null>(null, Validators.min(0)),
      halfHomeScore: this.fb.control<number | null>(null, Validators.min(0)),
      halfAwayScore: this.fb.control<number | null>(null, Validators.min(0)),
    },
    { validators: [teamsDifferValidator, scoresValidator] },
  );

  statusOptions: MatchStatusListDto[] = [];
  stadiumOptions: StadiumListDto[] = [];
  selectionOptions: SelectionOption[] = [];
  competitionOptions: CompetitionListDto[] = [];
  seasonOptions: SeasonListDto[] = [];
  roundOptions: RoundListDto[] = [];

  slug: string | null = null;
  isEditMode = false;
  loading = false;
  loadFailed = false;
  saving = false;
  error = '';

  // Values to apply once the cascade options for an edited match have loaded.
  private pendingSeason = '';
  private pendingRound = '';

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug');
    this.slug = slug;
    this.isEditMode = !!slug;

    this.loadOptions((page) => this.statuses.getAll(page, MAX_PAGE_SIZE), (items) => {
      this.statusOptions = items;
      // A new match starts as Scheduled unless the user already picked a status.
      const scheduled = items.find((s) => s.name.toLowerCase() === 'scheduled');
      if (!this.isEditMode && scheduled && !this.form.controls.statusPublicId.value) {
        this.form.controls.statusPublicId.setValue(scheduled.publicId);
      }
    });
    this.loadOptions((page) => this.stadiums.getAll(page, MAX_PAGE_SIZE), (items) => (this.stadiumOptions = items));
    this.loadOptions((page) => this.selections.getAll(page, MAX_PAGE_SIZE), (items) => (this.selectionOptions = items));
    this.loadOptions((page) => this.competitions.getAll(page, MAX_PAGE_SIZE), (items) => (this.competitionOptions = items));
    this.setupCascade();

    if (!slug) {
      // A new match must be scheduled in the future (same rule as the backend).
      this.form.controls.date.addValidators(futureDateValidator);
      // "Schedule match here" on the stadium page passes the stadium along.
      const stadium = this.route.snapshot.queryParamMap.get('stadium');
      if (stadium) this.form.controls.stadiumPublicId.setValue(stadium);
      return;
    }

    this.loading = true;
    this.matches
      .getBySlug(slug)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => (this.loading = false)),
      )
      .subscribe({
        next: (match) => this.patch(match),
        error: (err) => {
          this.loadFailed = true;
          this.error = apiErrorMessage(err, 'Match could not be loaded.');
        },
      });
  }

  private loadOptions<T>(getPage: (page: number) => Observable<PagedResult<T>>, assign: (items: T[]) => void): void {
    fetchAllPages(getPage)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: assign,
        error: (err) => (this.error = apiErrorMessage(err, 'Some options could not be loaded.')),
      });
  }

  /** Competition → season → round: each pick reloads the next list and clears stale picks. */
  private setupCascade(): void {
    const { competitionPublicId, seasonPublicId } = this.form.controls;

    competitionPublicId.valueChanges
      .pipe(
        distinctUntilChanged(),
        switchMap((id): Observable<SeasonListDto[]> =>
          id ? fetchAllPages((page) => this.seasons.getAll(page, MAX_PAGE_SIZE, id)).pipe(catchError(() => of([]))) : of([]),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((seasons) => {
        this.seasonOptions = seasons;
        const keep = seasons.some((s) => s.publicId === this.pendingSeason) ? this.pendingSeason : '';
        this.pendingSeason = '';
        seasonPublicId.setValue(keep);
      });

    seasonPublicId.valueChanges
      .pipe(
        distinctUntilChanged(),
        switchMap((id): Observable<RoundListDto[]> =>
          id ? fetchAllPages((page) => this.rounds.getAll(page, MAX_PAGE_SIZE, id)).pipe(catchError(() => of([]))) : of([]),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((rounds) => {
        this.roundOptions = rounds;
        const keep = rounds.some((r) => r.publicId === this.pendingRound) ? this.pendingRound : '';
        this.pendingRound = '';
        this.form.controls.roundPublicId.setValue(keep);
      });
  }

  private patch(match: MatchDetailsDto): void {
    this.pendingSeason = match.seasonPublicId ?? '';
    this.pendingRound = match.roundPublicId ?? '';
    this.form.patchValue({
      date: toLocalInputValue(match.date),
      statusPublicId: match.statusPublicId,
      stadiumPublicId: match.stadiumPublicId,
      homeSelectionPublicId: match.homeSelectionPublicId ?? '',
      awaySelectionPublicId: match.awaySelectionPublicId ?? '',
      followersCount: match.followersCount,
      isPostponed: match.isPostponed,
      homeScore: match.homeScore,
      awayScore: match.awayScore,
      halfHomeScore: match.halfHomeScore,
      halfAwayScore: match.halfAwayScore,
    });
    // Triggers the cascade; season and round are applied from the pending values once loaded.
    this.form.controls.competitionPublicId.setValue(match.competitionPublicId ?? '');
  }

  fieldError(name: FieldName): string | null {
    const control = this.form.controls[name];
    if (!control.touched || control.valid) return null;
    if (control.hasError('required')) return 'This field is required.';
    if (control.hasError('server')) return control.getError('server');
    if (control.hasError('min')) return 'Cannot be negative.';
    if (control.hasError('pastDate')) return 'A new match must be scheduled in the future.';
    return 'Invalid value.';
  }

  save(): void {
    if (this.saving || this.loading || this.loadFailed) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const body: SaveMatchDto = {
      date: new Date(value.date).toISOString(),
      followersCount: Number(value.followersCount) || 0,
      isPostponed: value.isPostponed,
      statusPublicId: value.statusPublicId,
      stadiumPublicId: value.stadiumPublicId,
      homeSelectionPublicId: value.homeSelectionPublicId || null,
      awaySelectionPublicId: value.awaySelectionPublicId || null,
      roundPublicId: value.roundPublicId || null,
    };
    if (this.isEditMode) {
      body.homeScore = value.homeScore;
      body.awayScore = value.awayScore;
      body.halfHomeScore = value.halfHomeScore;
      body.halfAwayScore = value.halfAwayScore;
    }

    const request$ = this.isEditMode ? this.matches.update(this.slug!, body) : this.matches.create(body);
    this.saving = true;
    this.error = '';
    request$
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => (this.saving = false)),
        catchError((err) => {
          this.showServerErrors(err);
          return EMPTY;
        }),
      )
      .subscribe((saved) => this.router.navigate(['/matches', saved.slug]));
  }

  /** Puts backend field errors next to their inputs; anything else goes to the form-level message. */
  private showServerErrors(err: unknown): void {
    this.error = apiErrorMessage(err, 'Could not save the match.');
    if (!(err instanceof HttpErrorResponse)) return;
    const errors = err.error?.errors as { propertyName: string; errorMessage: string }[] | undefined;
    for (const e of errors ?? []) {
      const name = (e.propertyName.charAt(0).toLowerCase() + e.propertyName.slice(1)) as FieldName;
      const control = this.form.controls[name];
      if (control) {
        control.setErrors({ server: e.errorMessage });
        control.markAsTouched();
      }
    }
  }

  cancel(): void {
    this.router.navigate(this.slug ? ['/matches', this.slug] : ['/matches']);
  }
}
