import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { GenericView } from '../../generic/generic-view/generic-view';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { formatPictureForView } from '../../core/models/person.models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-manager-view',
  imports: [CommonModule, GenericView, RouterModule],
  templateUrl: './manager-view.html',
  styleUrl: './manager-view.css',
})
export class ManagerView implements OnInit {
  selectedManager: Record<string, unknown> | null = null;

  constructor(
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
    private router: Router,
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    fetch(`${environment.apiUrl}/api/manager/${id}`)
      .then((res) => res.json())
      .then((data) => {
        this.selectedManager = {
          firstName: data.firstName,
          lastName: data.lastName,
          dateOfBirth: data.dateOfBirth,
          picture: formatPictureForView(data.picture),
          careerStart: data.careerStart || '—',
        };
        this.cdr.detectChanges();
      })
      .catch((err) => console.error('Greška pri dohvaćanju:', err));
  }

  goBack(): void {
    this.router.navigate(['/manager-list']);
  }
}
