import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { fileToBase64 } from '../../core/helpers/file.helper';
import { pictureToDataUrl } from '../../core/models/person.models';
import { RefereeService, RefereeWriteDto } from '../../core/services/referee.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { UploadProgress } from '../../shared/components/upload-progress/upload-progress';

function pastDate(control: AbstractControl): ValidationErrors | null {
  if (!control.value) return null;
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return new Date(control.value) < today ? null : { pastDate: true };
}

@Component({
  selector: 'app-manage-referees',
  imports: [CommonModule, ReactiveFormsModule, UploadProgress],
  templateUrl: './manage-referees.html',
  styleUrl: './manage-referees.css',
})
export class ManageReferees implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private refereeService = inject(RefereeService);
  private confirmDialog = inject(ConfirmDialogService);
  private cdr = inject(ChangeDetectorRef);
  private fb = inject(FormBuilder);

  form = this.fb.group({
    firstName: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(100)]),
    lastName: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(100)]),
    dateOfBirth: this.fb.nonNullable.control('', [Validators.required, pastDate]),
    careerStart: this.fb.control<string | null>(null),
  });

  editingId: string | null = null;
  isEditMode = false;
  picture: string | null = null;
  pictureChanged = false;
  uploadProgress: number | null = null;

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const id = params.get('id');
      if (id) {
        this.editingId = id;
        this.isEditMode = true;
        this.loadReferee(id);
        return;
      }

      this.editingId = null;
      this.isEditMode = false;
      this.resetForm();
      this.cdr.detectChanges();
    });
  }

  get picturePreview(): string | null {
    return pictureToDataUrl(this.picture);
  }

  fieldError(name: 'firstName' | 'lastName' | 'dateOfBirth'): string | null {
    const control = this.form.controls[name];
    if (!control.touched || control.valid) return null;
    if (control.hasError('required')) return 'This field is required.';
    if (control.hasError('maxlength')) return 'Maximum length is 100 characters.';
    if (control.hasError('pastDate')) return 'Date of birth must be in the past.';
    return 'Invalid value.';
  }

  loadReferee(id: string): void {
    this.refereeService.getById(id).subscribe({
      next: (data) => {
        this.form.patchValue({
          firstName: data.firstName ?? '',
          lastName: data.lastName ?? '',
          dateOfBirth: data.dateOfBirth ?? '',
          careerStart: data.careerStart ?? null,
        });
        this.picture = data.picture ?? null;
        this.pictureChanged = false;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Could not load the referee:', err),
    });
  }

  async onPictureSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.picture = await fileToBase64(file);
    this.pictureChanged = true;
  }

  async saveReferee(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { firstName, lastName } = this.form.getRawValue();
    if (this.isEditMode) {
      const confirmed = await this.confirmDialog.confirm({
        message: `Save changes for referee ${firstName} ${lastName}?`,
      });
      if (!confirmed) return;
    }

    const payload = this.buildPayload();
    const onProgress = this.pictureChanged ? (percent: number) => this.setUploadProgress(percent) : undefined;
    if (onProgress) this.setUploadProgress(0);
    const request$ = this.isEditMode
      ? this.refereeService.update(this.editingId!, payload, onProgress)
      : this.refereeService.create(payload, onProgress);

    request$.subscribe({
      next: () => {
        this.refereeService.notifyListRefresh();
        this.router.navigate(['/referee-list']);
      },
      error: (err) => {
        this.setUploadProgress(null);
        console.error('Could not save the referee:', err);
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/referee-list']);
  }

  private resetForm(): void {
    this.form.reset({ firstName: '', lastName: '', dateOfBirth: '', careerStart: null });
    this.picture = null;
    this.pictureChanged = false;
  }

  private setUploadProgress(percent: number | null): void {
    this.uploadProgress = percent;
    this.cdr.detectChanges();
  }

  private buildPayload(): RefereeWriteDto {
    const value = this.form.getRawValue();
    const payload: RefereeWriteDto = {
      firstName: value.firstName.trim(),
      lastName: value.lastName.trim(),
      dateOfBirth: value.dateOfBirth,
      picture: this.picture,
      careerStart: value.careerStart || null,
    };

    if (this.isEditMode && !this.pictureChanged) {
      const { picture: _picture, ...rest } = payload;
      return rest;
    }

    return payload;
  }
}
