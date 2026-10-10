import { Component, EventEmitter, inject, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Modal } from '../../shared/components/modal/modal';
import { InputBox } from '../../shared/components/input-box/input-box';
import { Button } from '../../shared/components/button/button';
import { Dropdown } from '../../shared/components/dropdown/dropdown';
import { SubPositionService } from '../../core/services/subposition.service';
import { PositionService } from '../../core/services/position.service';
import { Position, SubPosition } from '../../core/models/formation.models';

@Component({
  selector: 'app-subposition-form',
  imports: [Modal, InputBox, ReactiveFormsModule, Button, Dropdown],
  templateUrl: './subposition-form.html',
})
export class SubPositionForm implements OnChanges {
  @Input() isOpen = false;
  @Input() editItem: SubPosition | null = null;
  @Output() saved = new EventEmitter<void>();
  @Output() closed = new EventEmitter<void>();

  private service = inject(SubPositionService);
  private positions = inject(PositionService);
  private fb = inject(NonNullableFormBuilder);

  positionOptions: Position[] = [];

  form = this.fb.group({
    name: this.fb.control('', { validators: [Validators.required, Validators.maxLength(100)] }),
    positionId: this.fb.control('', { validators: [Validators.required] }),
  });

  get isEditMode(): boolean {
    return this.editItem !== null;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isOpen']?.currentValue) {
      this.positions.getAll(1, 100).subscribe((page) => {
        this.positionOptions = page.items;
      });
    }
    if (!changes['editItem'] && !changes['isOpen']) return;
    if (this.editItem) {
      this.form.patchValue({ name: this.editItem.name, positionId: this.editItem.positionId });
    } else {
      this.form.reset({ name: '', positionId: '' });
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
