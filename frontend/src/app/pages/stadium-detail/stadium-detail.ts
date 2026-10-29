import { Component, computed, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AppDatePipe, AppDateTimePipe, AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { StadiumService } from '../../core/services/stadium.service';
import { MatchService } from '../../core/services/match.service';
import { UserStateService } from '../../core/services/user-state.service';
import { roundLabel } from '../../core/helpers/match.helper';
import { Map } from '../../shared/components/map/map';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { catchError, map, switchMap, of } from 'rxjs';
import * as L from 'leaflet';

/** How many matches the stadium page shows; the full list is one click away. */
const STADIUM_MATCHES_PAGE_SIZE = 10;

@Component({
  selector: 'app-stadium-detail',
  imports: [AppDatePipe, AppDateTimePipe, AppNumberPipe, Map, RouterLink],
  templateUrl: './stadium-detail.html',
  styleUrl: './stadium-detail.css',
})
export class StadiumDetail {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private stadiumService = inject(StadiumService);
  private matchService = inject(MatchService);
  canManage = inject(UserStateService).canManage;
  roundLabel = roundLabel;

  stadium = toSignal(
    this.route.paramMap.pipe(
      map(params => params.get('slug')),
      switchMap(slug => slug ? this.stadiumService.getBySlug(slug) : of(null))
    )
  );

  /** Matches played at this stadium, soonest first (the backend sorts by date). null = could not load. */
  matches = toSignal(
    toObservable(this.stadium).pipe(
      switchMap(s => s
        ? this.matchService.getAll(1, STADIUM_MATCHES_PAGE_SIZE, { stadiumPublicId: s.publicId }).pipe(catchError(() => of(null)))
        : of(undefined))
    )
  );

  // Straight-line (Haversine) distance from the stadium to its city centre, in km.
  distanceToCityKm = computed(() => {
    const s = this.stadium();
    if (!s || s.cityLatitude == null || s.cityLongitude == null) return null;
    const meters = L.latLng(s.latitude, s.longitude).distanceTo(L.latLng(s.cityLatitude, s.cityLongitude));
    return Math.round(meters / 100) / 10;
  });

  scheduleMatch(publicId: string): void {
    this.router.navigate(['/manage-matches'], { queryParams: { stadium: publicId } });
  }

  goBack(): void {
    this.router.navigate(['/stadium-list']);
  }
}
