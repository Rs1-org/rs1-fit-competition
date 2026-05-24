import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { GenericView } from '../../generic/generic-view/generic-view';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { pictureToDataUrl } from '../../core/models/person.models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-referee-view',
  imports: [CommonModule, GenericView, RouterModule],
  templateUrl: './referee-view.html',
  styleUrl: './referee-view.css',
})
export class RefereeView implements OnInit {
  selectedReferee: Record<string, unknown> | null = null;
  pictureUrl: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
    private router: Router,
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    fetch(`${environment.apiUrl}/api/referee/${id}`)
      .then((res) => res.json())
      .then((data) => {
        this.selectedReferee = {
          firstName: data.firstName,
          lastName: data.lastName,
          dateOfBirth: data.dateOfBirth,
          careerStart: data.careerStart || '—',
        };
        this.pictureUrl = pictureToDataUrl(data.picture);
        this.cdr.detectChanges();
      })
      .catch((err) => console.error('Greška pri dohvaćanju:', err));
  }

  goBack(): void {
    this.router.navigate(['/referee-list']);
  }
}
