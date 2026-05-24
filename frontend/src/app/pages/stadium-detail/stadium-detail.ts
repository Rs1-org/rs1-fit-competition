import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DecimalPipe, DatePipe } from '@angular/common';
import { StadiumService } from '../../core/services/stadium.service';
import { StadiumDetailsDto } from '../../core/models/stadium';
import { Map } from '../../shared/components/map/map';

@Component({
  selector: 'app-stadium-detail',
  imports: [DecimalPipe, DatePipe, Map],
  templateUrl: './stadium-detail.html',
  styleUrl: './stadium-detail.css',
})
export class StadiumDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private stadiumService = inject(StadiumService);

  stadium: StadiumDetailsDto | null = null;
  loading = true;

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug');
    if (!slug) { this.loading = false; return; }

    this.stadiumService.getBySlug(slug).subscribe({
      next: (data) => { this.stadium = data; this.loading = false; },
      error: (err) => { console.error(err); this.loading = false; }
    });
  }

  goBack(): void {
    this.router.navigate(['/stadium-list']);
  }
}
