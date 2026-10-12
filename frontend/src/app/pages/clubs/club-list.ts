import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subject, switchMap } from 'rxjs';
import { catchError, of } from 'rxjs';
import { CLUB_AGES, ClubFilter } from '../../core/models/club';
import { CityListDto } from '../../core/models/city';
import { StadiumListDto } from '../../core/models/stadium';
import { Club } from '../../core/models/club';
import { PagedResult } from '../../core/models/paged-result';
import { CityService } from '../../core/services/city-services';
import { StadiumService } from '../../core/services/stadium.service';
import { ClubService } from '../../core/services/club.service';
import { UserStateService } from '../../core/services/user-state.service';
import { fetchAllPages } from '../../core/helpers/paging.helper';

@Component({
  selector: 'app-club-list', imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './club-list.html', styleUrl: './clubs.css',
})
export class ClubList implements OnInit {
  private clubs = inject(ClubService);
  private cityService = inject(CityService);
  private stadiumService = inject(StadiumService);
  private destroyRef = inject(DestroyRef);
  private requests = new Subject<void>();
  private activeFilter: ClubFilter = {};
  readonly user = inject(UserStateService);
  readonly ages = CLUB_AGES;
  readonly cities = signal<CityListDto[]>([]);
  readonly stadiums = signal<StadiumListDto[]>([]);
  readonly result = signal<PagedResult<Club> | null>(null);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly deleting = signal(false);
  readonly pendingDelete = signal<Club | null>(null);
  page = 1;
  readonly filters = inject(FormBuilder).nonNullable.group({
    name: ['', Validators.maxLength(100)], cityId: [''], stadiumId: [''],
    foundingYear: ['', [Validators.pattern(/^[0-9]+$/), Validators.min(1), Validators.max(9999)]], clubAge: [''],
  });

  ngOnInit() {
    this.cityService.getAllForDropdown().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: x => this.cities.set(x), error: () => this.error.set('Could not load city filters.'),
    });
    fetchAllPages(page => this.stadiumService.getAll(page, 100)).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: x => this.stadiums.set(x), error: () => this.error.set('Could not load stadium filters.'),
    });
    this.requests.pipe(switchMap(() => {
      this.loading.set(true);
      this.error.set('');
      return this.clubs.getAll(this.page, 10, this.activeFilter).pipe(catchError(() => {
        this.error.set('Could not load clubs. Please try again.');
        return of(null);
      }));
    }), takeUntilDestroyed(this.destroyRef)).subscribe(x => { this.result.set(x); this.loading.set(false); });
    this.load();
  }
  load() { this.requests.next(); }
  apply() {
    if (this.filters.invalid) { this.filters.markAllAsTouched(); return; }
    const f = this.filters.getRawValue();
    this.activeFilter = { name: f.name.trim(), cityId: f.cityId, stadiumId: f.stadiumId,
      foundingYear: f.foundingYear ? Number(f.foundingYear) : undefined,
      clubAge: f.clubAge !== '' ? Number(f.clubAge) : undefined };
    this.page = 1;
    this.load();
  }
  reset() { this.filters.reset(); this.apply(); }
  changePage(page: number) { this.page = page; this.load(); }
  confirmDelete() {
    const club = this.pendingDelete();
    if (!club || !this.user.isAdmin() || this.deleting()) return;
    this.deleting.set(true);
    this.clubs.delete(club.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.pendingDelete.set(null); this.deleting.set(false);
        if (this.result()?.items.length === 1 && this.page > 1) this.page--;
        this.load();
      },
      error: () => { this.deleting.set(false); this.error.set('Could not delete the club. Please try again.'); },
    });
  }
}
