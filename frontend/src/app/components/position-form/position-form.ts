import { Component, EventEmitter, inject, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Modal } from '../../shared/components/modal/modal';
import { InputBox } from '../../shared/components/input-box/input-box';
import { Button } from '../../shared/components/button/button';
import { PositionService } from '../../core/services/position.service';
import { Position } from '../../core/models/formation.models';

@Component({
  selector: 'app-position-form',
  imports: [Modal, InputBox, ReactiveFormsModule, Button],
  templateUrl: './position-form.html',
})
export class PositionForm implements OnChanges {
  @Input() isOpen = false;
  @Input() editItem: Position | null = null;
  @Output() saved = new EventEmitter<void>();
  @Output() closed = new EventEmitter<void>();

  private service = inject(PositionService);
  private fb = inject(NonNullableFormBuilder);

  form = this.fb.group({
    name: this.fb.control('', { validators: [Validators.required, Validators.maxLength(100)] }),
  });

  get isEditMode(): boolean {
    return this.editItem !== null;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!changes['editItem'] && !changes['isOpen']) return;
    if (this.editItem) this.form.patchValue({ name: this.editItem.name });
    else this.form.reset({ name: '' });
  }

  onClose(): void {
    this.closed.emit();
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const body = this.form.getRawValue();
    const request$ = this.isEditMode
      ? this.service.update(this.editItem!.slug, body)
      : this.service.create(body);
    request$.subscribe({
      next: () => this.saved.emit(),
      error: (err) => console.error(err),
    });
  }
}
