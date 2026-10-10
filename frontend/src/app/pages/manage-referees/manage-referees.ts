import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { fileToBase64 } from '../../core/helpers/file.helper';
import { emptyPersonFields, pictureToDataUrl, Referee } from '../../core/models/person.models';
import { RefereeService, RefereeWriteDto } from '../../core/services/referee.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { UploadProgress } from '../../shared/components/upload-progress/upload-progress';

@Component({
  selector: 'app-manage-referees',
  imports: [CommonModule, FormsModule, UploadProgress],
  templateUrl: './manage-referees.html',
  styleUrl: './manage-referees.css',
})
export class ManageReferees implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private refereeService = inject(RefereeService);
  private confirmDialog = inject(ConfirmDialogService);
  private cdr = inject(ChangeDetectorRef);

  referee: Referee = {
    ...emptyPersonFields(),
    careerStart: null,
  };

  editingId: string | null = null;
  isEditMode = false;
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

  private resetForm(): void {
    this.referee = {
      ...emptyPersonFields(),
      careerStart: null,
    };
    this.pictureChanged = false;
  }

  get isRefereeInvalid(): boolean {
    return (
      !this.referee.firstName.trim() ||
      !this.referee.lastName.trim() ||
      !this.referee.dateOfBirth
    );
  }

  get picturePreview(): string | null {
    return pictureToDataUrl(this.referee.picture);
  }

  loadReferee(id: string): void {
    this.refereeService.getById(id).subscribe({
      next: (data) => {
        this.referee = {
          id: data.id,
          firstName: data.firstName ?? '',
          lastName: data.lastName ?? '',
          dateOfBirth: data.dateOfBirth ?? '',
          picture: data.picture ?? null,
          careerStart: data.careerStart ?? null,
        };
        this.pictureChanged = false;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Greška pri učitavanju suca:', err),
    });
  }

  async onPictureSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.referee.picture = await fileToBase64(file);
    this.pictureChanged = true;
  }

  async saveReferee(): Promise<void> {
    if (this.isRefereeInvalid) return;

    if (this.isEditMode) {
      const confirmed = await this.confirmDialog.confirm({
        message: `Jeste li sigurni da želite spremiti promjene za suca ${this.referee.firstName} ${this.referee.lastName}?`,
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
        console.error('Greška pri spremanju suca:', err);
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/referee-list']);
  }

  private setUploadProgress(percent: number | null): void {
    this.uploadProgress = percent;
    this.cdr.detectChanges();
  }

  private buildPayload(): RefereeWriteDto {
    const payload: RefereeWriteDto = {
      firstName: this.referee.firstName.trim(),
      lastName: this.referee.lastName.trim(),
      dateOfBirth: this.referee.dateOfBirth,
      picture: this.referee.picture,
      careerStart: this.referee.careerStart || null,
    };

    if (this.isEditMode && !this.pictureChanged) {
      const { picture: _picture, ...rest } = payload;
      return rest;
    }

    return payload;
  }
}
