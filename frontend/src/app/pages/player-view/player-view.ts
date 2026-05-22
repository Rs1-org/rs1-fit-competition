import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { GenericView } from '../../generic/generic-view/generic-view';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { formatPictureForView } from '../../core/models/person.models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-player-view',
  imports: [CommonModule, GenericView, RouterModule],
  templateUrl: './player-view.html',
  styleUrl: './player-view.css',
})
export class PlayerView implements OnInit {
  selectedPlayer: Record<string, unknown> | null = null;

  constructor(
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
    private router: Router,
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    fetch(`${environment.apiUrl}/api/player/${id}`)
      .then((res) => res.json())
      .then((data) => {
        this.selectedPlayer = {
          firstName: data.firstName,
          lastName: data.lastName,
          dateOfBirth: data.dateOfBirth,
          picture: formatPictureForView(data.picture),
          height: data.height,
          shirtNumber: data.shirtNumber,
          transferValue: data.transferValue,
        };
        this.cdr.detectChanges();
      })
      .catch((err) => console.error('Greška pri dohvaćanju:', err));
  }

  goBack(): void {
    this.router.navigate(['/player-list']);
  }
}
