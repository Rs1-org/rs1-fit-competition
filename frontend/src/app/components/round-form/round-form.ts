import { Component, EventEmitter, inject, Input, OnChanges, Output, signal, SimpleChanges } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputBox } from '../../shared/components/input-box/input-box';
import { Modal } from '../../shared/components/modal/modal';
import { Button } from '../../shared/components/button/button';
import { RoundService } from '../../core/services/round.service';
import { ToastService } from '../../core/services/toast.service';
import { RoundListDto, SaveRoundDto } from '../../core/models/round';
import { showServerErrors } from '../../core/utils/api-error';

@Component({
  selector: 'app-round-form',
  imports: [InputBox, Modal, ReactiveFormsModule, Button],
  templateUrl: './round-form.html',
  styleUrl: '../competition-form/competition-form.css',
})
export class RoundForm implements OnChanges {
  @Input() isOpen = false;
  @Input({ required: true }) seasonPublicId = '';
  /** Suggested number for a new round (next free one). */
  @Input() nextNumber = 1;
  @Input() editRound: RoundListDto | null = null;
  @Output() saved = new EventEmitter<void>();
  @Output() closed = new EventEmitter<void>();

  private fb = inject(NonNullableFormBuilder);
  private roundService = inject(RoundService);
  private toast = inject(ToastService);

  errorMessage = signal('');
  saving = signal(false);

  form = this.fb.group({
    number: this.fb.control(1, { validators: [Validators.required, Validators.min(1), Validators.max(100)] }),
    name: this.fb.control('', { validators: [Validators.required, Validators.maxLength(50)] }),
  });

  get isEditMode(): boolean {
    return this.editRound !== null;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['editRound'] || changes['isOpen']) {
      this.errorMessage.set('');
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
    if (this.saving()) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    const payload: SaveRoundDto = { name: value.name.trim(), number: Number(value.number) };
    const editing = this.isEditMode;
    const request$ = editing
      ? this.roundService.update(this.editRound!.slug, payload)
      : this.roundService.create({ ...payload, seasonPublicId: this.seasonPublicId });

    this.saving.set(true);
    this.errorMessage.set('');
    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success(editing ? 'Round updated' : 'Round added');
        this.saved.emit();
      },
      error: (err) => {
        this.saving.set(false);
        const message = showServerErrors(this.form, err, 'The round could not be saved.');
        this.errorMessage.set(message);
        if (message) this.toast.error(message);
      },
    });
  }
}
