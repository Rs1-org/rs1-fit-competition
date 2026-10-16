import { Component, EventEmitter, inject, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { AbstractControl, NonNullableFormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { InputBox } from '../../shared/components/input-box/input-box';
import { Modal } from '../../shared/components/modal/modal';
import { Button } from '../../shared/components/button/button';
import { SeasonService } from '../../core/services/season.service';
import { SaveSeasonDto, SeasonDetailsDto } from '../../core/models/season';
import { apiErrorMessage } from '../../core/utils/api-error';

/** End date must be after the start date (mirrors the backend validator). */
function endAfterStart(group: AbstractControl): ValidationErrors | null {
  const start = group.get('startDate')?.value;
  const end = group.get('endDate')?.value;
  return start && end && end <= start ? { endBeforeStart: true } : null;
}

@Component({
  selector: 'app-season-form',
  imports: [InputBox, Modal, ReactiveFormsModule, Button],
  templateUrl: './season-form.html',
  styleUrl: '../competition-form/competition-form.css',
})
export class SeasonForm implements OnChanges {
  @Input() isOpen = false;
  @Input({ required: true }) competitionPublicId = '';
  @Input() editSeason: SeasonDetailsDto | null = null;
  @Output() saved = new EventEmitter<void>();
  @Output() closed = new EventEmitter<void>();

  private fb = inject(NonNullableFormBuilder);
  private seasonService = inject(SeasonService);

  errorMessage = '';
  saving = false;

  form = this.fb.group(
    {
      name: this.fb.control('', { validators: [Validators.required, Validators.maxLength(50)] }),
      startDate: this.fb.control('', { validators: Validators.required }),
      endDate: this.fb.control('', { validators: Validators.required }),
      isCurrent: this.fb.control(false),
    },
    { validators: endAfterStart }
  );

  get isEditMode(): boolean {
    return this.editSeason !== null;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['editSeason'] || changes['isOpen']) {
      this.errorMessage = '';
      if (this.editSeason) {
        this.form.reset({
          name: this.editSeason.name,
          startDate: this.editSeason.startDate,
          endDate: this.editSeason.endDate,
          isCurrent: this.editSeason.isCurrent,
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
    const payload: SaveSeasonDto = {
      name: value.name.trim(),
      startDate: value.startDate,
      endDate: value.endDate,
      isCurrent: value.isCurrent,
    };
    const request$ = this.isEditMode
      ? this.seasonService.update(this.editSeason!.slug, payload)
      : this.seasonService.create({ ...payload, competitionPublicId: this.competitionPublicId });

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
