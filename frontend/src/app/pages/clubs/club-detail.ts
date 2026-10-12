import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, of, switchMap } from 'rxjs';
import { Club, CLUB_AGES } from '../../core/models/club';
import { ClubService } from '../../core/services/club.service';
import { UserStateService } from '../../core/services/user-state.service';

@Component({
  selector: 'app-club-detail', imports: [CommonModule, RouterLink], styleUrl: './clubs.css',
  template: `<section class="clubs"><a routerLink="/club-list">Back to clubs</a>
    @if (error()) { <p role="alert">{{ error() }}</p> }
    @if (loading()) { <p role="status">Loading club…</p> }
    @if (club(); as item) { <h1>{{ item.name }}</h1><dl><dt>City</dt><dd>{{ item.cityName }}</dd><dt>Stadium</dt><dd>{{ item.stadiumName }}</dd><dt>Founded</dt><dd>{{ item.foundingDate | date:'mediumDate':'UTC' }}</dd><dt>Age group</dt><dd>{{ ages[item.clubAge] }}</dd></dl>
    @if (user.canManage()) { <a [routerLink]="['/manage-clubs', item.slug]">Edit club</a> } }
  </section>`,
})
export class ClubDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private service = inject(ClubService);
  private destroyRef = inject(DestroyRef);
  readonly user = inject(UserStateService);
  readonly ages = CLUB_AGES;
  readonly club = signal<Club | null>(null);
  readonly error = signal('');
  readonly loading = signal(true);
  ngOnInit() {
    this.route.paramMap.pipe(switchMap(params => {
      this.loading.set(true); this.error.set(''); this.club.set(null);
      return this.service.getBySlug(params.get('slug')!).pipe(catchError(() => {
        this.error.set('Club was not found or could not be loaded.'); return of(null);
      }));
    }), takeUntilDestroyed(this.destroyRef)).subscribe(club => { this.club.set(club); this.loading.set(false); });
  }
}
