import { Component, EventEmitter, inject, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputBox } from '../../shared/components/input-box/input-box';
import { Modal } from '../../shared/components/modal/modal';
import { Button } from '../../shared/components/button/button';
import { RoundService } from '../../core/services/round.service';
import { RoundListDto, SaveRoundDto } from '../../core/models/round';
import { apiErrorMessage } from '../../core/utils/api-error';

@Component({
  selector: 'app-round-form',
  imports: [InputBox, Modal, ReactiveFormsModule, Button],
  templateUrl: './round-form.html',
  styleUrl: '../competition-form/competition-form.css',
})
export class RoundForm implements OnChanges {
  @Input() isOpen = false;
  @Input({ required: true }) seasonId = '';
  /** Suggested number for a new round (next free one). */
  @Input() nextNumber = 1;
  @Input() editRound: RoundListDto | null = null;
  @Output() saved = new EventEmitter<void>();
  @Output() closed = new EventEmitter<void>();

  private fb = inject(NonNullableFormBuilder);
  private roundService = inject(RoundService);

  errorMessage = '';
  saving = false;

  form = this.fb.group({
    number: this.fb.control(1, { validators: [Validators.required, Validators.min(1), Validators.max(100)] }),
    name: this.fb.control('', { validators: [Validators.required, Validators.maxLength(50)] }),
  });

  get isEditMode(): boolean {
    return this.editRound !== null;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['editRound'] || changes['isOpen']) {
      this.errorMessage = '';
      if (this.editRound) {
        this.form.reset({ number: this.editRound.number, name: this.editRound.name });
      } else {
        this.form.reset({ number: this.nextNumber, name: `Round ${this.nextNumber}` });
      }
    }
  }

  onClose(): void {
    this.closed.emit();
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    const payload: SaveRoundDto = { name: value.name.trim(), number: Number(value.number) };
    const request$ = this.isEditMode
      ? this.roundService.update(this.editRound!.slug, payload)
      : this.roundService.create({ ...payload, seasonId: this.seasonId });

    this.saving = true;
    this.errorMessage = '';
    request$.subscribe({
      next: () => {
        this.saving = false;
        this.saved.emit();
      },
      error: (err) => {
        this.saving = false;
        this.errorMessage = apiErrorMessage(err);
      },
    });
  }
}
