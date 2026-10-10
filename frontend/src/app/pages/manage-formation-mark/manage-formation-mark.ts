import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { FormationMarkService } from '../../core/services/formation-mark.service';

@Component({
  selector: 'app-manage-formation-mark',
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-formation-mark.html',
  styleUrls: ['../manage-players/manage-players.css', '../manage-formation/manage-formation.css'],
})
export class ManageFormationMark implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private service = inject(FormationMarkService);

  defenders = 4;
  midfielders = 3;
  attackers = 3;
  slug: string | null = null;
  isEditMode = false;
  error = '';

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const slug = params.get('slug');
      this.slug = slug;
      this.isEditMode = !!slug;
      this.error = '';
      if (!slug) return;

      this.service.getBySlug(slug).subscribe({
        next: (mark) => {
          this.defenders = mark.defenders;
          this.midfielders = mark.midfielders;
          this.attackers = mark.attackers;
        },
        error: () => {
          this.error = 'Formation mark was not found.';
        },
      });
    });
  }

  get isInvalid(): boolean {
    const sum = Number(this.defenders) + Number(this.midfielders) + Number(this.attackers);
    return [this.defenders, this.midfielders, this.attackers].some((value) => value < 0 || value > 10) || sum < 1 || sum > 11;
  }

  save(): void {
    if (this.isInvalid) {
      this.error = 'Lines must be between 0 and 10, and their sum between 1 and 11.';
      return;
    }

    const body = {
      defenders: Number(this.defenders),
      midfielders: Number(this.midfielders),
      attackers: Number(this.attackers),
    };
    const request$ = this.isEditMode
      ? this.service.update(this.slug!, body)
      : this.service.create(body);

    request$.subscribe({
      next: () => this.router.navigate(['/formation-mark-list']),
      error: (err) => {
        this.error = err?.error?.message || err?.error?.title || 'Could not save the formation mark.';
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/formation-mark-list']);
  }
}
