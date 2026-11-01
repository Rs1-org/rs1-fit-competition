import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { pictureToDataUrl } from '../../core/models/person.models';
import { RefereeService } from '../../core/services/referee.service';
import { apiErrorMessage } from '../../core/utils/api-error';
import { AppDatePipe } from '../../shared/pipes/regional-format.pipes';
import { Breadcrumb } from '../../shared/components/breadcrumb/breadcrumb';
import { StatTile } from '../../shared/components/stat-tile/stat-tile';
import { Skeleton } from '../../shared/components/skeleton/skeleton';
import { ErrorBanner } from '../../shared/components/error-banner/error-banner';
import { ImageZoom } from '../../shared/components/image-zoom/image-zoom';

@Component({
  selector: 'app-referee-view',
  imports: [CommonModule, RouterModule, AppDatePipe, Breadcrumb, StatTile, Skeleton, ErrorBanner, ImageZoom],
  templateUrl: './referee-view.html',
  styleUrls: ['./referee-view.css', '../player-view/person-profile.css'],
})
export class RefereeView implements OnInit {
  private route = inject(ActivatedRoute);
  private cdr = inject(ChangeDetectorRef);
  private router = inject(Router);
  private refereeService = inject(RefereeService);

  selectedReferee: Record<string, unknown> | null = null;
  pictureUrl: string | null = null;
  loading = true;
  error = '';
  zoom = false;
  private refereeId = '';

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('publicId');
    if (!id) {
      this.loading = false;
      this.error = 'Referee could not be loaded.';
      return;
    }
    this.refereeId = id;
    this.loadReferee(id);
  }

  retry(): void {
    if (this.refereeId) this.loadReferee(this.refereeId);
  }

  age(value: unknown): number | null {
    const date = new Date(String(value ?? ''));
    if (Number.isNaN(date.getTime())) return null;
    const today = new Date();
    let years = today.getFullYear() - date.getFullYear();
    if (today.getMonth() < date.getMonth() || (today.getMonth() === date.getMonth() && today.getDate() < date.getDate())) years--;
    return years;
  }

  goBack(): void {
    this.router.navigate(['/referee-list']);
  }

  private loadReferee(id: string): void {
    this.loading = true;
    this.error = '';
    this.refereeService.getById(id).subscribe({
      next: (data) => {
        this.selectedReferee = {
          firstName: data.firstName,
          lastName: data.lastName,
          dateOfBirth: data.dateOfBirth,
          careerStart: data.careerStart || null,
        };
        this.pictureUrl = pictureToDataUrl(data.picture);
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.loading = false;
        this.error = apiErrorMessage(err, 'Referee could not be loaded.');
        this.cdr.detectChanges();
      },
    });
  }
}
