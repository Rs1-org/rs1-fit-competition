import { Component, EventEmitter, inject, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputBox } from '../../shared/components/input-box/input-box';
import { Modal } from '../../shared/components/modal/modal';
import { Button } from '../../shared/components/button/button';
import { Dropdown } from '../../shared/components/dropdown/dropdown';
import { CountryService } from '../../core/services/country-service';
import { CompetitionService } from '../../core/services/competition.service';
import { CompetitionDetailsDto, SaveCompetitionDto } from '../../core/models/competition';
import { apiErrorMessage } from '../../core/utils/api-error';

@Component({
  selector: 'app-competition-form',
  imports: [InputBox, Modal, ReactiveFormsModule, Button, Dropdown],
  templateUrl: './competition-form.html',
  styleUrl: './competition-form.css',
})
export class CompetitionForm implements OnInit, OnChanges {
  @Input() isOpen = false;
  @Input() editCompetition: CompetitionDetailsDto | null = null;
  @Output() saved = new EventEmitter<void>();
  @Output() closed = new EventEmitter<void>();

  private fb = inject(NonNullableFormBuilder);
  private countryService = inject(CountryService);
  private competitionService = inject(CompetitionService);

  countries: { id: string; name: string }[] = [];
  errorMessage = '';
  saving = false;

  form = this.fb.group({
    name: this.fb.control('', { validators: [Validators.required, Validators.minLength(2), Validators.maxLength(100)] }),
    shortName: this.fb.control('', { validators: Validators.maxLength(20) }),
    countryId: this.fb.control(''),
    description: this.fb.control('', { validators: Validators.maxLength(1000) }),
  });

  get isEditMode(): boolean {
    return this.editCompetition !== null;
  }

  ngOnInit(): void {
    this.countryService.getAll(1, 100).subscribe({
      next: (data) => (this.countries = [{ id: '', name: 'No country (international)' }, ...data.items]),
      error: (err) => console.error('Error loading countries:', err),
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['editCompetition'] || changes['isOpen']) {
      this.errorMessage = '';
      if (this.editCompetition) {
        this.form.reset({
          name: this.editCompetition.name,
          shortName: this.editCompetition.shortName ?? '',
          countryId: this.editCompetition.countryId ?? '',
          description: this.editCompetition.description ?? '',
        });
      } else {
        this.form.reset();
      }
    }
  }

  onClose(): void {
    this.form.reset();
    this.closed.emit();
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    const payload: SaveCompetitionDto = {
      name: value.name.trim(),
      shortName: value.shortName.trim() || null,
      description: value.description.trim() || null,
      countryId: value.countryId || null,
    };
    const request$ = this.isEditMode
      ? this.competitionService.update(this.editCompetition!.slug, payload)
      : this.competitionService.create(payload);

    this.saving = true;
    this.errorMessage = '';
    request$.subscribe({
      next: () => {
        this.saving = false;
        this.form.reset();
        this.saved.emit();
      },
      error: (err) => {
        this.saving = false;
        this.errorMessage = apiErrorMessage(err);
      },
    });
  }
}
