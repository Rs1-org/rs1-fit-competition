import { Component, computed, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AppDatePipe, AppNumberPipe } from '../../shared/pipes/regional-format.pipes';
import { StadiumService } from '../../core/services/stadium.service';
import { Map } from '../../shared/components/map/map';
import { toSignal } from '@angular/core/rxjs-interop';
import { map, switchMap, of } from 'rxjs';
import * as L from 'leaflet';

@Component({
  selector: 'app-stadium-detail',
  imports: [AppDatePipe, AppNumberPipe, Map],
  templateUrl: './stadium-detail.html',
  styleUrl: './stadium-detail.css',
})
export class StadiumDetail {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private stadiumService = inject(StadiumService);

  stadium = toSignal(
    this.route.paramMap.pipe(
      map(params => params.get('slug')),
      switchMap(slug => slug ? this.stadiumService.getBySlug(slug) : of(null))
    )
  );

  // Straight-line (Haversine) distance from the stadium to its city centre, in km.
  distanceToCityKm = computed(() => {
    const s = this.stadium();
    if (!s || s.cityLatitude == null || s.cityLongitude == null) return null;
    const meters = L.latLng(s.latitude, s.longitude).distanceTo(L.latLng(s.cityLatitude, s.cityLongitude));
    return Math.round(meters / 100) / 10;
  });

  goBack(): void {
    this.router.navigate(['/stadium-list']);
  }
}
