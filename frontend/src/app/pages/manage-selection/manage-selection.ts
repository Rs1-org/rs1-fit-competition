import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CountryListDto } from '../../core/models/country';
import { CountryService } from '../../core/services/country-service';
import { SelectionService } from '../../core/services/selection.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { EMPTY, catchError, finalize, switchMap } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { ClubService } from '../../core/services/club.service';
import { ClubListDto } from '../../core/models/club';
import { StadiumService } from '../../core/services/stadium.service';
import { StadiumListDto } from '../../core/models/stadium';
import { fetchAllPages } from '../../core/helpers/paging.helper';
import { apiErrorMessage } from '../../core/utils/api-error';
import { AutofocusDirective } from '../../shared/directives/autofocus.directive';

@Component({
  selector: 'app-manage-selection',
  imports: [ReactiveFormsModule, AutofocusDirective],
  templateUrl: './manage-selection.html',
  styleUrls: ['../manage-players/manage-players.css', '../manage-formation/manage-formation.css'],
})
export class ManageSelection implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private selections = inject(SelectionService);
  private countries = inject(CountryService);
  private fb = inject(FormBuilder);
  private clubs = inject(ClubService);
  private stadiums = inject(StadiumService);
  private destroyRef = inject(DestroyRef);

  form = this.fb.group({
    name: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(120)]),
    abbreviation: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(10)]),
    countryPublicId: this.fb.nonNullable.control('', Validators.required),
    clubPublicId: this.fb.nonNullable.control('', Validators.required),
    // Optional: an empty value means the selection has no home stadium.
    homeStadiumPublicId: this.fb.nonNullable.control(''),
    followersCount: this.fb.nonNullable.control(0, [Validators.required, Validators.min(0)]),
  });

  countryOptions: CountryListDto[] = [];
  clubOptions: ClubListDto[] = [];
  clubsLoading = false;
  clubsError = '';
  stadiumOptions: StadiumListDto[] = [];
  stadiumsLoading = false;
  stadiumsError = '';
  saving = false;
  loading = false;
  loadFailed = false;
  slug: string | null = null;
  isEditMode = false;
  error = '';

  ngOnInit(): void {
    this.loadClubs();
    this.loadStadiums();
    fetchAllPages((page) => this.countries.getAll(page, 100))
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (countries) => (this.countryOptions = countries),
        error: (err) => (this.error = apiErrorMessage(err, 'Countries could not be loaded.')),
      });

    this.route.paramMap
      .pipe(
        switchMap((params) => {
          const slug = params.get('slug');
          this.slug = slug;
          this.isEditMode = !!slug;
          this.error = '';
          this.loadFailed = false;
          this.form.reset({
            name: '',
            abbreviation: '',
            countryPublicId: '',
            clubPublicId: '',
            homeStadiumPublicId: '',
            followersCount: 0,
          });
          if (!slug) {
            return EMPTY;
          }

          this.loading = true;
          return this.selections.getBySlug(slug).pipe(
            catchError((err) => {
              this.error = apiErrorMessage(err, 'Selection could not be loaded.');
              this.loadFailed = true;
              return EMPTY;
            }),
            finalize(() => (this.loading = false)),
          );
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (selection) => {
          this.form.patchValue({
            name: selection.name,
            abbreviation: selection.abbreviation,
            followersCount: selection.followersCount,
            countryPublicId: selection.countryPublicId ?? '',
            clubPublicId: selection.clubPublicId ?? '',
            homeStadiumPublicId: selection.homeStadiumPublicId ?? '',
          });
        },
      });
  }

  loadClubs(): void {
    this.clubsLoading = true;
    this.clubsError = '';
    fetchAllPages((page) => this.clubs.getAll(page, 100))
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => (this.clubsLoading = false)),
      )
      .subscribe({
        next: (clubs) => (this.clubOptions = clubs),
        error: (err) => (this.clubsError = apiErrorMessage(err, 'Clubs could not be loaded.')),
      });
  }

  loadStadiums(): void {
    this.stadiumsLoading = true;
    this.stadiumsError = '';
    fetchAllPages((page) => this.stadiums.getAll(page, 100))
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => (this.stadiumsLoading = false)),
      )
      .subscribe({
        next: (stadiums) => (this.stadiumOptions = stadiums),
        error: (err) =>
          (this.stadiumsError = apiErrorMessage(err, 'Stadiums could not be loaded.')),
      });
  }

  fieldError(
    name:
      | 'name'
      | 'abbreviation'
      | 'countryPublicId'
      | 'clubPublicId'
      | 'homeStadiumPublicId'
      | 'followersCount',
  ): string | null {
    const control = this.form.controls[name];
    if (!control.touched || control.valid) return null;
    if (control.hasError('required')) return 'This field is required.';
    if (control.hasError('server')) return control.getError('server');
    if (control.hasError('maxlength')) {
      return name === 'abbreviation'
        ? 'Maximum length is 10 characters.'
        : 'Maximum length is 120 characters.';
    }
    if (control.hasError('min')) return 'Followers cannot be negative.';
    return 'Invalid value.';
  }

  save(): void {
    if (this.saving || this.loading || this.loadFailed || this.clubsLoading || this.clubsError)
      return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const body = {
      name: value.name.trim(),
      abbreviation: value.abbreviation.trim(),
      followersCount: Number(value.followersCount) || 0,
      countryPublicId: value.countryPublicId,
      clubPublicId: value.clubPublicId,
      homeStadiumPublicId: value.homeStadiumPublicId || null,
    };
    const request$ = this.isEditMode
      ? this.selections.update(this.slug!, body)
      : this.selections.create(body);

    this.saving = true;
    this.error = '';
    request$
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => (this.saving = false)),
      )
      .subscribe({
        next: () => this.router.navigate(['/selection-list']),
        error: (err) => {
          this.error = apiErrorMessage(err, 'Could not save the selection.');
          if (err instanceof HttpErrorResponse) {
            const errors = err.error?.errors as
              | { propertyName: string; errorMessage: string }[]
              | undefined;
            const clubError = errors?.find((error) => error.propertyName === 'ClubPublicId');
            if (clubError) {
              this.form.controls.clubPublicId.setErrors({ server: clubError.errorMessage });
              this.form.controls.clubPublicId.markAsTouched();
            }
            const stadiumError = errors?.find(
              (error) => error.propertyName === 'HomeStadiumPublicId',
            );
            if (stadiumError) {
              this.form.controls.homeStadiumPublicId.setErrors({
                server: stadiumError.errorMessage,
              });
              this.form.controls.homeStadiumPublicId.markAsTouched();
            }
          }
        },
      });
  }

  cancel(): void {
    this.router.navigate(['/selection-list']);
  }
}
