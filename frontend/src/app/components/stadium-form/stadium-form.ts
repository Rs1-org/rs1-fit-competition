import { Component, inject, Input, OnInit, Output, EventEmitter, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputBox } from '../../shared/components/input-box/input-box';
import { Modal } from '../../shared/components/modal/modal';
import { CityService } from '../../core/services/city-services';
import { StadiumService } from '../../core/services/stadium.service';
import { UserStateService } from '../../core/services/user-state.service';
import { ToastService } from '../../core/services/toast.service';
import { Button } from '../../shared/components/button/button';
import { Icon } from '../../shared/components/icon/icon';
import { Dropdown } from '../../shared/components/dropdown/dropdown';
import { Map } from '../../shared/components/map/map';
import { ModalState } from '../../core/helpers/modal-state.helper';
import { CityListDto } from '../../core/models/city';
import { StadiumDetailsDto } from '../../core/models/stadium';
import { UploadProgress } from '../../shared/components/upload-progress/upload-progress';
import { showServerErrors } from '../../core/utils/api-error';

/** Where the map opens and the pin starts for a new stadium (Sarajevo). */
const DEFAULT_POINT = { latitude: 43.8563, longitude: 18.4131 };

@Component({
  selector: 'app-stadium-form',
  standalone: true,
  imports: [InputBox, Modal, ReactiveFormsModule, Button, Icon, Dropdown, Map, UploadProgress],
  templateUrl: './stadium-form.html',
  styleUrl: './stadium-form.css',
})
export class StadiumForm implements OnInit {
  private fb = inject(NonNullableFormBuilder);
  private cityService = inject(CityService);
  private stadiumService = inject(StadiumService);
  private userState = inject(UserStateService);
  private toast = inject(ToastService);

  modalState = new ModalState();
  editingSlug: string | null = null;
  cities = signal<CityListDto[]>([]);
  private selectedPicture: File | null = null;
  uploadProgress = signal<number | null>(null);
  saving = signal(false);
  formError = signal('');

  @Input() showTrigger = true;
  @Output() saved = new EventEmitter<StadiumDetailsDto>();

  form = this.fb.group({
    name: this.fb.control('', { validators: [Validators.required, Validators.maxLength(150)] }),
    capacity: this.fb.control(0, { validators: [Validators.required, Validators.min(1)] }),
    dateOfConstruction: this.fb.control('', { validators: Validators.required }),
    cityPublicId: this.fb.control('', { validators: Validators.required }),
    latitude: this.fb.control(DEFAULT_POINT.latitude, { validators: [Validators.required, Validators.min(-90), Validators.max(90)] }),
    longitude: this.fb.control(DEFAULT_POINT.longitude, { validators: [Validators.required, Validators.min(-180), Validators.max(180)] }),
    geoDescription: this.fb.control('', { validators: [Validators.required, Validators.maxLength(500)] })
  });

  ngOnInit(): void {
    this.loadCities();
  }

  loadCities(): void {
    this.cityService.getAllForDropdown().subscribe({
      next: (cities) => this.cities.set(cities),
      error: () => this.formError.set('Cities could not be loaded. Close the form and try again.'),
    });
  }

  openModal(): void {
    if (!this.userState.canManage()) return;
    this.editingSlug = null;
    this.selectedPicture = null;
    this.formError.set('');
    this.form.reset();
    this.modalState.open();
  }

  openForEdit(stadium: StadiumDetailsDto): void {
    if (!this.userState.canManage()) return;
    this.editingSlug = stadium.slug ?? null;
    this.selectedPicture = null;
    this.formError.set('');
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
    this.uploadProgress.set(null);
    this.formError.set('');
    this.modalState.close(this.form);
  }

  onFileChange(event: Event): void {
    this.selectedPicture = (event.target as HTMLInputElement).files?.[0] ?? null;
  }

  onLocationSelected(event: { lat: number, lng: number }): void {
    this.form.patchValue({ latitude: round(event.lat), longitude: round(event.lng) });
    this.form.controls.latitude.markAsDirty();
    this.form.controls.longitude.markAsDirty();
  }

  /** The pin follows the latitude and longitude fields while both are valid. */
  pinLat(): number | null {
    return this.form.controls.latitude.valid && this.form.controls.longitude.valid ? this.form.controls.latitude.value : null;
  }

  pinLng(): number | null {
    return this.pinLat() === null ? null : this.form.controls.longitude.value;
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
    if (this.saving()) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const fd = this.buildFormData();
    const onProgress = this.selectedPicture
      ? (percent: number) => this.uploadProgress.set(percent)
      : undefined;
    if (onProgress) this.uploadProgress.set(0);
    const editing = !!this.editingSlug;
    this.saving.set(true);
    this.formError.set('');

    const done = (stadium: StadiumDetailsDto) => {
      this.saving.set(false);
      this.toast.success(editing ? 'Stadium updated' : 'Stadium added');
      this.closeModal();
      this.saved.emit(stadium);
    };
    const fail = (err: unknown) => {
      this.saving.set(false);
      this.uploadProgress.set(null);
      const message = showServerErrors(this.form, err, 'The stadium could not be saved.');
      this.formError.set(message);
      if (message) this.toast.error(message);
    };

    if (this.editingSlug) {
      this.stadiumService.update(this.editingSlug, fd, onProgress).subscribe({ next: done, error: fail });
    } else {
      this.stadiumService.create(fd, onProgress).subscribe({ next: done, error: fail });
    }
  }
}

/** Six decimals are about 10 cm, plenty for a stadium. */
function round(value: number): number {
  return Math.round(value * 1e6) / 1e6;
}
