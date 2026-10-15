import { Component, EventEmitter, inject, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Modal } from '../../shared/components/modal/modal';
import { InputBox } from '../../shared/components/input-box/input-box';
import { Button } from '../../shared/components/button/button';
import { Dropdown } from '../../shared/components/dropdown/dropdown';
import { FormationService } from '../../core/services/formation.service';
import { FormationMarkService } from '../../core/services/formation-mark.service';
import { Formation, FormationMark } from '../../core/models/formation.models';

@Component({
  selector: 'app-formation-form',
  imports: [Modal, InputBox, ReactiveFormsModule, Button, Dropdown],
  templateUrl: './formation-form.html',
})
export class FormationForm implements OnChanges, OnInit {
  @Input() isOpen = false;
  @Input() editItem: Formation | null = null;
  @Output() saved = new EventEmitter<void>();
  @Output() closed = new EventEmitter<void>();

  private service = inject(FormationService);
  private marks = inject(FormationMarkService);
  private fb = inject(NonNullableFormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  markOptions: Array<FormationMark & { label: string }> = [];
  error = '';

  form = this.fb.group({
    code: this.fb.control('', { validators: [Validators.required, Validators.maxLength(20), Validators.pattern(/^\d+-\d+-\d+$/)] }),
    formationMarkId: this.fb.control('', { validators: [Validators.required] }),
  });

  get isEditMode(): boolean {
    return this.editItem !== null;
  }

  ngOnInit(): void {
    this.isOpen = true;
    this.loadMarks();
    const slug = this.route.snapshot.paramMap.get('slug');
    if (!slug) return;
    this.service.getBySlug(slug).subscribe({
      next: (formation) => {
        this.editItem = formation;
        this.form.patchValue({ code: formation.code, formationMarkId: formation.formationMarkId });
      },
      error: () => {
        this.error = 'Formation was not found.';
      },
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isOpen']?.currentValue) {
      this.loadMarks();
    }
    if (!changes['editItem'] && !changes['isOpen']) return;
    if (this.editItem) {
      this.form.patchValue({ code: this.editItem.code, formationMarkId: this.editItem.formationMarkId });
    } else {
      this.form.reset({ code: '', formationMarkId: '' });
    }
  }

  onClose(): void {
    this.closed.emit();
    this.router.navigate(['/formation-list']);
  }

  private loadMarks(): void {
    this.marks.getAll(1, 100).subscribe((page) => {
      this.markOptions = page.items.map((mark) => ({
        ...mark,
        label: `${mark.defenders}-${mark.midfielders}-${mark.attackers}`,
      }));
    });
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
      next: () => {
        this.saved.emit();
        this.router.navigate(['/formation-list']);
      },
      error: (err) => {
        this.error = err?.error?.message || err?.error?.title || 'Could not save the formation. The code must match the mark, for example 4-3-3.';
      },
    });
  }
}
