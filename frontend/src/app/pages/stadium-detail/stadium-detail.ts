import { Component, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DecimalPipe, DatePipe } from '@angular/common';
import { StadiumService } from '../../core/services/stadium.service';
import { Map } from '../../shared/components/map/map';
import { toSignal } from '@angular/core/rxjs-interop';
import { map, switchMap, of } from 'rxjs';

@Component({
  selector: 'app-stadium-detail',
  imports: [DecimalPipe, DatePipe, Map],
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

  goBack(): void {
    this.router.navigate(['/stadium-list']);
  }
}
