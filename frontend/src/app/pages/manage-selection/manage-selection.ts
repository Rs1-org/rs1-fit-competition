import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CountryListDto } from '../../core/models/country';
import { CountryService } from '../../core/services/country-service';
import { SelectionService } from '../../core/services/selection.service';

@Component({
  selector: 'app-manage-selection',
  imports: [ReactiveFormsModule],
  templateUrl: './manage-selection.html',
  styleUrls: ['../manage-players/manage-players.css', '../manage-formation/manage-formation.css'],
})
export class ManageSelection implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private selections = inject(SelectionService);
  private countries = inject(CountryService);
  private fb = inject(FormBuilder);

  form = this.fb.group({
    name: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(120)]),
    abbreviation: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(10)]),
    countryPublicId: this.fb.nonNullable.control('', Validators.required),
    followersCount: this.fb.nonNullable.control(0, [Validators.required, Validators.min(0)]),
  });

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
      if (!slug) {
        this.form.reset({ name: '', abbreviation: '', countryPublicId: '', followersCount: 0 });
        return;
      }

      this.selections.getBySlug(slug).subscribe({
        next: (selection) => {
          this.form.patchValue({
            name: selection.name,
            abbreviation: selection.abbreviation,
            followersCount: selection.followersCount,
            countryPublicId: selection.countryPublicId ?? '',
          });
        },
        error: () => {
          this.error = 'Selection was not found.';
        },
      });
    });
  }

  fieldError(name: 'name' | 'abbreviation' | 'countryPublicId' | 'followersCount'): string | null {
    const control = this.form.controls[name];
    if (!control.touched || control.valid) return null;
    if (control.hasError('required')) return 'This field is required.';
    if (control.hasError('maxlength')) {
      return name === 'abbreviation'
        ? 'Maximum length is 10 characters.'
        : 'Maximum length is 120 characters.';
    }
    if (control.hasError('min')) return 'Followers cannot be negative.';
    return 'Invalid value.';
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const body = {
      name: value.name.trim(),
      abbreviation: value.abbreviation.trim(),
      followersCount: Number(value.followersCount) || 0,
      countryPublicId: value.countryPublicId,
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
