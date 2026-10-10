import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { FormationMark } from '../../core/models/formation.models';
import { FormationMarkService } from '../../core/services/formation-mark.service';
import { FormationService } from '../../core/services/formation.service';

@Component({
  selector: 'app-manage-formation',
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-formation.html',
  styleUrls: ['../manage-players/manage-players.css', './manage-formation.css'],
})
export class ManageFormation implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private formations = inject(FormationService);
  private marks = inject(FormationMarkService);

  code = '';
  formationMarkId = '';
  markOptions: FormationMark[] = [];
  slug: string | null = null;
  isEditMode = false;
  error = '';

  ngOnInit(): void {
    this.marks.getAll(1, 100).subscribe((page) => {
      this.markOptions = page.items;
    });

    this.route.paramMap.subscribe((params) => {
      const slug = params.get('slug');
      this.slug = slug;
      this.isEditMode = !!slug;
      this.error = '';
      if (!slug) return;

      this.formations.getBySlug(slug).subscribe({
        next: (formation) => {
          this.code = formation.code;
          this.formationMarkId = formation.formationMarkId;
        },
        error: () => {
          this.error = 'Formation was not found.';
        },
      });
    });
  }

  get isInvalid(): boolean {
    return !/^\d+-\d+-\d+$/.test(this.code.trim()) || !this.formationMarkId;
  }

  save(): void {
    if (this.isInvalid) {
      this.error = 'Code must look like 4-3-3 and a formation mark is required.';
      return;
    }

    const body = { code: this.code.trim(), formationMarkId: this.formationMarkId };
    const request$ = this.isEditMode
      ? this.formations.update(this.slug!, body)
      : this.formations.create(body);

    request$.subscribe({
      next: () => this.router.navigate(['/formation-list']),
      error: (err) => {
        this.error = err?.error?.message || err?.error?.title || 'Could not save the formation. The code must match the mark, for example 4-3-3.';
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/formation-list']);
  }
}
