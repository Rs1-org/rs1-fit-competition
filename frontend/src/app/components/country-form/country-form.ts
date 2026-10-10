import { Component, EventEmitter, inject, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Modal } from '../../shared/components/modal/modal';
import { InputBox } from '../../shared/components/input-box/input-box';
import { Button } from '../../shared/components/button/button';
import { CountryService } from '../../core/services/country-service';
import { CountryListDto } from '../../core/models/country';
import { UploadProgress } from '../../shared/components/upload-progress/upload-progress';

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
  private fb = inject(NonNullableFormBuilder);

  uploadProgress: number | null = null;

  form = this.fb.group({
    name: this.fb.control('', { validators: [Validators.required, Validators.maxLength(100)] }),
    picture: this.fb.control<File | null>(null)
  });

  get isEditMode(): boolean {
    return this.editCountry !== null;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['editCountry']) {
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
    this.uploadProgress = null;
    this.form.reset();
    this.closed.emit();
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const formData = new FormData();
    formData.append('name', this.form.controls.name.value);

    const picture = this.form.controls.picture.value;
    if (picture) formData.append('picture', picture);
    const onProgress = picture ? (percent: number) => { this.uploadProgress = percent; } : undefined;
    if (onProgress) this.uploadProgress = 0;

    const done = () => {
      this.uploadProgress = null;
      this.form.reset();
      this.saved.emit();
    };
    const fail = (err: unknown) => {
      this.uploadProgress = null;
      console.error(err);
    };

    if (this.isEditMode) {
      this.countryService.update(this.editCountry!.slug, formData, onProgress).subscribe({ next: done, error: fail });
    } else {
      this.countryService.create(formData, onProgress).subscribe({ next: done, error: fail });
    }
  }
}
