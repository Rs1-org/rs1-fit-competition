import { Component, EventEmitter, inject, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Modal } from '../../shared/components/modal/modal';
import { InputBox } from '../../shared/components/input-box/input-box';
import { Button } from '../../shared/components/button/button';
import { FormationMarkService } from '../../core/services/formation-mark.service';
import { FormationMark } from '../../core/models/formation.models';

@Component({
  selector: 'app-formation-mark-form',
  imports: [Modal, InputBox, ReactiveFormsModule, Button],
  templateUrl: './formation-mark-form.html',
})
export class FormationMarkForm implements OnChanges, OnInit {
  @Input() isOpen = false;
  @Input() editItem: FormationMark | null = null;
  @Output() saved = new EventEmitter<void>();
  @Output() closed = new EventEmitter<void>();

  private service = inject(FormationMarkService);
  private fb = inject(NonNullableFormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  error = '';

  form = this.fb.group({
    defenders: this.fb.control(4, { validators: [Validators.required, Validators.min(0), Validators.max(10)] }),
    midfielders: this.fb.control(3, { validators: [Validators.required, Validators.min(0), Validators.max(10)] }),
    attackers: this.fb.control(3, { validators: [Validators.required, Validators.min(0), Validators.max(10)] }),
  });

  get isEditMode(): boolean {
    return this.editItem !== null;
  }

  ngOnInit(): void {
    this.isOpen = true;
    const slug = this.route.snapshot.paramMap.get('slug');
    if (!slug) return;
    this.service.getBySlug(slug).subscribe({
      next: (mark) => {
        this.editItem = mark;
        this.form.patchValue({
          defenders: mark.defenders,
          midfielders: mark.midfielders,
          attackers: mark.attackers,
        });
      },
      error: () => {
        this.error = 'Formation mark was not found.';
      },
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!changes['editItem'] && !changes['isOpen']) return;
    if (this.editItem) {
      this.form.patchValue({
        defenders: this.editItem.defenders,
        midfielders: this.editItem.midfielders,
        attackers: this.editItem.attackers,
      });
    } else {
      this.form.reset({ defenders: 4, midfielders: 3, attackers: 3 });
    }
  }

  onClose(): void {
    this.closed.emit();
    this.router.navigate(['/formation-mark-list']);
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
        this.router.navigate(['/formation-mark-list']);
      },
      error: (err) => {
        this.error = err?.error?.message || err?.error?.title || 'Could not save the formation mark.';
      },
    });
  }
}
