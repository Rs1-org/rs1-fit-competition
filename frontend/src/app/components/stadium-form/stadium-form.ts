import { Component, inject, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputBox } from '../../shared/components/input-box/input-box';
import { Modal } from '../../shared/components/modal/modal';
import { CityService } from '../../core/services/city-services';
import { StadiumService } from '../../core/services/stadium.service';
import { UserStateService } from '../../core/services/user-state.service';
import { Button } from '../../shared/components/button/button';
import { Dropdown } from '../../shared/components/dropdown/dropdown';
import { Map } from '../../shared/components/map/map';
import { ModalState } from '../../core/helpers/modal-state.helper';
import { StadiumDetailsDto } from '../../core/models/stadium';
import { UploadProgress } from '../../shared/components/upload-progress/upload-progress';

@Component({
  selector: 'app-stadium-form',
  standalone: true,
  imports: [InputBox, Modal, ReactiveFormsModule, Button, Dropdown, Map, UploadProgress],
  templateUrl: './stadium-form.html',
  styleUrl: './stadium-form.css',
})
export class StadiumForm implements OnInit {
  private fb = inject(NonNullableFormBuilder);
  private cityService = inject(CityService);
  private stadiumService = inject(StadiumService);
  private userState = inject(UserStateService);

  modalState = new ModalState();
  editingSlug: string | null = null;
  cities: any[] = [];
  private selectedPicture: File | null = null;
  uploadProgress: number | null = null;

  @Input() showTrigger = true;
  @Output() saved = new EventEmitter<void>();

  form = this.fb.group({
    name: this.fb.control('', { validators: [Validators.required, Validators.maxLength(150)] }),
    capacity: this.fb.control(0, { validators: [Validators.required, Validators.min(1)] }),
    dateOfConstruction: this.fb.control('', { validators: Validators.required }),
    cityPublicId: this.fb.control('', { validators: Validators.required }),
    latitude: this.fb.control(43.8563, { validators: [Validators.required, Validators.min(-90), Validators.max(90)] }),
    longitude: this.fb.control(18.4131, { validators: [Validators.required, Validators.min(-180), Validators.max(180)] }),
    geoDescription: this.fb.control('', { validators: [Validators.required, Validators.maxLength(500)] })
  });

  ngOnInit(): void {
    this.loadCities();
  }

  loadCities(): void {
    this.cityService.getAllForDropdown().subscribe({
      next: (cities) => { this.cities = cities; },
      error: (err) => console.error('Error loading cities: ', err)
    });
  }

  openModal(): void {
    if (!this.userState.canManage()) return;
    this.editingSlug = null;
    this.selectedPicture = null;
    this.modalState.open();
  }

  openForEdit(stadium: StadiumDetailsDto): void {
    if (!this.userState.canManage()) return;
    this.editingSlug = stadium.slug ?? null;
    this.selectedPicture = null;
    this.form.patchValue({
      name: stadium.name ?? '',
      capacity: stadium.capacity,
      dateOfConstruction: new Date(stadium.dateOfConstruction).toISOString().split('T')[0],
      cityPublicId: stadium.cityPublicId as unknown as string,
      latitude: Number(stadium.latitude),
      longitude: Number(stadium.longitude),
      geoDescription: stadium.geoDescription ?? ''
    });
    this.modalState.open();
  }

  closeModal(): void {
    this.editingSlug = null;
    this.selectedPicture = null;
    this.uploadProgress = null;
    this.modalState.close(this.form);
  }

  onFileChange(event: Event): void {
    this.selectedPicture = (event.target as HTMLInputElement).files?.[0] ?? null;
  }

  onLocationSelected(event: { lat: number, lng: number }): void {
    this.form.patchValue({ latitude: event.lat, longitude: event.lng });
  }

  private buildFormData(): FormData {
    const value = this.form.getRawValue();
    const fd = new FormData();
    fd.append('name', value.name);
    fd.append('capacity', String(value.capacity));
    fd.append('dateOfConstruction', value.dateOfConstruction);
    fd.append('cityPublicId', value.cityPublicId);
    fd.append('latitude', String(value.latitude));
    fd.append('longitude', String(value.longitude));
    fd.append('geoDescription', value.geoDescription);
    if (this.selectedPicture) fd.append('picture', this.selectedPicture);
    return fd;
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const fd = this.buildFormData();
    const onProgress = this.selectedPicture
      ? (percent: number) => { this.uploadProgress = percent; }
      : undefined;
    if (onProgress) this.uploadProgress = 0;

    const done = () => {
      this.uploadProgress = null;
      this.closeModal();
      this.saved.emit();
    };
    const fail = (err: unknown) => {
      this.uploadProgress = null;
      console.error(err);
    };

    if (this.editingSlug) {
      this.stadiumService.update(this.editingSlug, fd, onProgress).subscribe({ next: done, error: fail });
    } else {
      this.stadiumService.create(fd, onProgress).subscribe({ next: done, error: fail });
    }
  }
}
