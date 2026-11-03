import { Component, EventEmitter, inject, Input, OnChanges, Output, signal, SimpleChanges } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Modal } from '../../shared/components/modal/modal';
import { InputBox } from '../../shared/components/input-box/input-box';
import { Button } from '../../shared/components/button/button';
import { CountryService } from '../../core/services/country-service';
import { ToastService } from '../../core/services/toast.service';
import { CountryListDto } from '../../core/models/country';
import { UploadProgress } from '../../shared/components/upload-progress/upload-progress';
import { showServerErrors } from '../../core/utils/api-error';

@Component({
  selector: 'app-country-form',
  imports: [Modal, InputBox, ReactiveFormsModule, Button, UploadProgress],
  templateUrl: './country-form.html',
  styleUrl: './country-form.css',
})
export class CountryForm implements OnChanges {
  @Input() isOpen = false;
  @Input() editCountry: CountryListDto | null = null;
  @Output() saved = new EventEmitter<void>();
  @Output() closed = new EventEmitter<void>();

  private countryService = inject(CountryService);
  private toast = inject(ToastService);
  private fb = inject(NonNullableFormBuilder);

  uploadProgress = signal<number | null>(null);
  saving = signal(false);
  formError = signal('');

  form = this.fb.group({
    name: this.fb.control('', { validators: [Validators.required, Validators.maxLength(100)] }),
    picture: this.fb.control<File | null>(null)
  });

  get isEditMode(): boolean {
    return this.editCountry !== null;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['editCountry']) {
      this.formError.set('');
      if (this.editCountry) {
        this.form.patchValue({ name: this.editCountry.name });
      } else {
        this.form.reset();
      }
    }
  }

  onFileChange(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0] ?? null;
    this.form.controls.picture.setValue(file);
  }

  onClose(): void {
    this.uploadProgress.set(null);
    this.formError.set('');
    this.form.reset();
    this.closed.emit();
  }

  onSubmit(): void {
    if (this.saving()) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const formData = new FormData();
    formData.append('name', this.form.controls.name.value);

    const picture = this.form.controls.picture.value;
    if (picture) formData.append('picture', picture);
    const onProgress = picture ? (percent: number) => this.uploadProgress.set(percent) : undefined;
    if (onProgress) this.uploadProgress.set(0);
    const editing = this.isEditMode;
    this.saving.set(true);
    this.formError.set('');

    const done = () => {
      this.saving.set(false);
      this.uploadProgress.set(null);
      this.toast.success(editing ? 'Country updated' : 'Country added');
      this.form.reset();
      this.saved.emit();
    };
    const fail = (err: unknown) => {
      this.saving.set(false);
      this.uploadProgress.set(null);
      const message = showServerErrors(this.form, err, 'The country could not be saved.');
      this.formError.set(message);
      if (message) this.toast.error(message);
    };

    if (this.isEditMode) {
      this.countryService.update(this.editCountry!.slug, formData, onProgress).subscribe({ next: done, error: fail });
    } else {
      this.countryService.create(formData, onProgress).subscribe({ next: done, error: fail });
    }
  }
}
