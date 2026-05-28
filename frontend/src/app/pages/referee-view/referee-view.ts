import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { GenericView } from '../../generic/generic-view/generic-view';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { pictureToDataUrl } from '../../core/models/person.models';
import { RefereeService } from '../../core/services/referee.service';

@Component({
  selector: 'app-referee-view',
  imports: [CommonModule, GenericView, RouterModule],
  templateUrl: './referee-view.html',
  styleUrl: './referee-view.css',
})
export class RefereeView implements OnInit {
  private route = inject(ActivatedRoute);
  private cdr = inject(ChangeDetectorRef);
  private router = inject(Router);
  private refereeService = inject(RefereeService);

  selectedReferee: Record<string, unknown> | null = null;
  pictureUrl: string | null = null;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    this.refereeService.getById(id).subscribe({
      next: (data) => {
        this.selectedReferee = {
          firstName: data.firstName,
          lastName: data.lastName,
          dateOfBirth: data.dateOfBirth,
          careerStart: data.careerStart || '—',
        };
        this.pictureUrl = pictureToDataUrl(data.picture);
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Greška pri dohvaćanju:', err),
    });
  }

  goBack(): void {
    this.router.navigate(['/referee-list']);
  }
}
