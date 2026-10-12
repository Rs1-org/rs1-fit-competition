import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { catchError, of, switchMap } from 'rxjs';
import { CLUB_AGES } from '../../core/models/club';
import { CityListDto } from '../../core/models/city';
import { StadiumListDto } from '../../core/models/stadium';
import { CityService } from '../../core/services/city-services';
import { StadiumService } from '../../core/services/stadium.service';
import { ClubService } from '../../core/services/club.service';
import { fetchAllPages } from '../../core/helpers/paging.helper';

export const foundingDateValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = control.value as string;
  if (!value) return null;
  const date = new Date(`${value}T00:00:00Z`);
  if (!/^\d{4}-\d{2}-\d{2}$/.test(value) || !Number.isFinite(date.getTime()) || date.toISOString().slice(0, 10) !== value)
    return { date: true };
  return value > new Date().toISOString().slice(0, 10) ? { future: true } : null;
};

export const clubNameValidator: ValidatorFn = control => {
  const value = String(control.value ?? '').toLowerCase().replace(/[đð]/g, 'd')
    .normalize('NFD').replace(/\p{M}/gu, '');
  return /[a-z0-9]/.test(value) ? null : { name: true };
};

@Component({
  selector: 'app-manage-club', imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './manage-club.html', styleUrl: './clubs.css',
})
export class ManageClub implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private clubs = inject(ClubService);
  private cityService = inject(CityService);
  private stadiumService = inject(StadiumService);
  private destroyRef = inject(DestroyRef);
  readonly ages = CLUB_AGES;
  readonly today = new Date().toISOString().slice(0, 10);
  readonly cities = signal<CityListDto[]>([]);
  readonly stadiums = signal<StadiumListDto[]>([]);
  readonly error = signal('');
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly optionsReady = signal(false);
  slug: string | null = null;
  readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100), clubNameValidator]],
    foundingDate: ['', [Validators.required, foundingDateValidator]],
    clubAge: [7, [Validators.required, Validators.min(0), Validators.max(7), Validators.pattern(/^[0-7]$/)]],
    cityId: ['', [Validators.required, Validators.pattern(/^[a-fA-F0-9]{32}$/)]],
    stadiumId: ['', [Validators.required, Validators.pattern(/^[a-fA-F0-9]{32}$/)]],
  });

  ngOnInit() {
    this.cityService.getAllForDropdown().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: cities => { this.cities.set(cities); this.optionsReady.set(true); },
      error: () => this.error.set('Could not load cities. Please reload the page.'),
    });
    this.form.controls.cityId.valueChanges.pipe(switchMap(cityId => {
      this.form.controls.stadiumId.setValue(''); this.stadiums.set([]);
      if (!cityId) return of([]);
      return fetchAllPages(page => this.stadiumService.getAll(page, 100, { cityId })).pipe(catchError(() => {
        this.error.set('Could not load stadiums. Please select the city again.'); return of([]);
      }));
    }), takeUntilDestroyed(this.destroyRef)).subscribe(stadiums => this.stadiums.set(stadiums));
    this.route.paramMap.pipe(switchMap(params => {
      this.slug = params.get('slug'); this.error.set('');
      this.form.reset(); this.stadiums.set([]);
      this.loading.set(!!this.slug);
      return this.slug ? this.clubs.getBySlug(this.slug).pipe(catchError(() => {
        this.error.set('Club was not found or could not be loaded.'); return of(null);
      })) : of(null);
    }), switchMap(club => {
      if (!club) { this.loading.set(false); return of(null); }
      this.form.patchValue(club, { emitEvent: false });
      return fetchAllPages(page => this.stadiumService.getAll(page, 100, { cityId: club.cityId })).pipe(catchError(() => {
        this.error.set('Could not load stadiums. Please reload the page.'); return of(null);
      }));
    }), takeUntilDestroyed(this.destroyRef)).subscribe(stadiums => {
      if (stadiums) this.stadiums.set(stadiums); this.loading.set(false);
    });
  }
  save() {
    if (this.form.invalid || this.saving() || this.loading() || !this.optionsReady()) {
      this.form.markAllAsTouched(); return;
    }
    this.saving.set(true); this.error.set('');
    const raw = this.form.getRawValue();
    const input = { ...raw, name: raw.name.trim(), clubAge: Number(raw.clubAge) };
    const request = this.slug ? this.clubs.update(this.slug, input) : this.clubs.create(input);
    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: club => { this.saving.set(false); this.router.navigate(['/club', club.slug]); },
      error: err => {
        this.saving.set(false);
        const body = err.error;
        const errors = body?.errors;
        const messages = Array.isArray(errors) ? errors.map((x: string | { errorMessage: string }) => typeof x === 'string' ? x : x.errorMessage)
          : errors && typeof errors === 'object' ? Object.values(errors).flat() : [];
        this.error.set(messages.length ? messages.join(' ') : body?.message || body?.title || 'Could not save the club. Please try again.');
      },
    });
  }
}
