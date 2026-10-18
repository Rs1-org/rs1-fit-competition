import { Component, DestroyRef, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin, of } from 'rxjs';
import { ClubService } from '../../core/services/club.service';
import { CityService } from '../../core/services/city-services';
import { StadiumService } from '../../core/services/stadium.service';
import { CLUB_AGES, SaveClubDto } from '../../core/models/club';
import { CityListDto } from '../../core/models/city';
import { StadiumListDto } from '../../core/models/stadium';
import { fetchAllPages } from '../../core/helpers/paging.helper';
import { apiErrorMessage } from '../../core/utils/api-error';
import { clubNameValidator, foundingDateValidator, todayIso } from './club-validation';

@Component({
  selector: 'app-club-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './club-form.html',
  styleUrl: '../../pages/club-list/club-pages.css',
})
export class ClubForm {
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly service = inject(ClubService);
  private readonly cityService = inject(CityService);
  private readonly stadiumService = inject(StadiumService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly ages = CLUB_AGES;
  readonly today = todayIso();
  readonly cities = signal<CityListDto[]>([]);
  readonly stadiums = signal<StadiumListDto[]>([]);
  readonly loading = signal(true);
  readonly loadError = signal('');
  readonly saving = signal(false);
  readonly saveError = signal('');
  readonly fieldErrors = signal<Record<string, string>>({});
  readonly slug = signal<string | null>(null);
  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100), clubNameValidator]],
    foundingDate: ['', foundingDateValidator],
    clubAge: [
      7,
      [Validators.required, Validators.min(0), Validators.max(7), Validators.pattern(/^\d$/)],
    ],
    cityPublicId: ['', Validators.pattern(/^[a-fA-F0-9]{32}$/)],
    stadiumPublicId: ['', Validators.pattern(/^[a-fA-F0-9]{32}$/)],
  });

  constructor() {
    this.route.paramMap.pipe(takeUntilDestroyed()).subscribe((params) => {
      this.slug.set(params.get('slug'));
      this.load();
    });
    this.form.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => {
      this.fieldErrors.set({});
      this.saveError.set('');
    });
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set('');
    const slug = this.slug();
    forkJoin({
      cities: this.cityService.getAllForDropdown(),
      stadiums: fetchAllPages((page) => this.stadiumService.getAll(page, 100)),
      club: slug ? this.service.getBySlug(slug) : of(null),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ cities, stadiums, club }) => {
          if (slug !== this.slug()) return;
          this.cities.set(cities);
          this.stadiums.set(stadiums);
          this.form.reset(
            club
              ? {
                  name: club.name,
                  foundingDate: club.foundingDate ?? '',
                  clubAge: club.clubAge,
                  cityPublicId: club.cityPublicId ?? '',
                  stadiumPublicId: club.stadiumPublicId ?? '',
                }
              : { name: '', foundingDate: '', clubAge: 7, cityPublicId: '', stadiumPublicId: '' },
          );
          this.loading.set(false);
        },
        error: (err) => {
          if (slug === this.slug()) {
            this.loading.set(false);
            this.loadError.set(apiErrorMessage(err, 'The club form could not be loaded.'));
          }
        },
      });
  }

  errorFor(field: keyof SaveClubDto): string {
    const serverError = this.fieldErrors()[field];
    if (serverError) return serverError;
    const control = this.form.controls[field];
    if (!control.touched || !control.invalid) return '';
    if (field === 'name')
      return 'Enter a name with 2–100 characters and at least one letter or number supported in a URL.';
    if (field === 'foundingDate') return 'Enter a valid founding date that is not in the future.';
    if (field === 'clubAge') return 'Select a valid age group.';
    return 'Select a valid option.';
  }

  onSubmit(): void {
    if (this.saving() || this.loading() || this.loadError()) return;
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    const value = this.form.getRawValue();
    const body: SaveClubDto = {
      ...value,
      name: value.name.trim(),
      foundingDate: value.foundingDate || null,
      cityPublicId: value.cityPublicId || null,
      stadiumPublicId: value.stadiumPublicId || null,
    };
    const slug = this.slug();
    this.saving.set(true);
    this.saveError.set('');
    this.fieldErrors.set({});
    (slug ? this.service.update(slug, body) : this.service.create(body)).subscribe({
      next: (club) => {
        this.saving.set(false);
        this.router.navigate(['/clubs', club.slug]);
      },
      error: (err: unknown) => {
        this.saving.set(false);
        this.saveError.set(apiErrorMessage(err));
        if (err instanceof HttpErrorResponse) {
          const errors =
            (err.error as { errors?: { propertyName: string; errorMessage: string }[] } | null)
              ?.errors ?? [];
          this.fieldErrors.set(
            Object.fromEntries(
              errors.map((e) => [
                e.propertyName.charAt(0).toLowerCase() + e.propertyName.slice(1),
                e.errorMessage,
              ]),
            ),
          );
        }
      },
    });
  }
}
