import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CountryListDto } from '../../core/models/country';
import { CountryService } from '../../core/services/country-service';
import { SelectionService } from '../../core/services/selection.service';

@Component({
  selector: 'app-manage-selection',
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-selection.html',
  styleUrls: ['../manage-players/manage-players.css', '../manage-formation/manage-formation.css'],
})
export class ManageSelection implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private selections = inject(SelectionService);
  private countries = inject(CountryService);

  name = '';
  abbreviation = '';
  followersCount = 0;
  countryPublicId = '';
  countryOptions: CountryListDto[] = [];
  slug: string | null = null;
  isEditMode = false;
  error = '';

  ngOnInit(): void {
    this.countries.getAll(1, 100).subscribe((page) => {
      this.countryOptions = page.items;
    });

    this.route.paramMap.subscribe((params) => {
      const slug = params.get('slug');
      this.slug = slug;
      this.isEditMode = !!slug;
      this.error = '';
      if (!slug) return;

      this.selections.getBySlug(slug).subscribe({
        next: (selection) => {
          this.name = selection.name;
          this.abbreviation = selection.abbreviation;
          this.followersCount = selection.followersCount;
          this.countryPublicId = selection.countryPublicId ?? '';
        },
        error: () => {
          this.error = 'Selection was not found.';
        },
      });
    });
  }

  get isInvalid(): boolean {
    return !this.name.trim() || !this.abbreviation.trim() || !this.countryPublicId || this.followersCount < 0;
  }

  save(): void {
    if (this.isInvalid) {
      this.error = 'Name, abbreviation and country are required.';
      return;
    }

    const body = {
      name: this.name.trim(),
      abbreviation: this.abbreviation.trim(),
      followersCount: Number(this.followersCount) || 0,
      countryPublicId: this.countryPublicId,
    };
    const request$ = this.isEditMode
      ? this.selections.update(this.slug!, body)
      : this.selections.create(body);

    request$.subscribe({
      next: () => this.router.navigate(['/selection-list']),
      error: (err) => {
        this.error = err?.error?.message || err?.error?.title || 'Could not save the selection.';
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/selection-list']);
  }
}
