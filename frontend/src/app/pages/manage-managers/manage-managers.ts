import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { fileToBase64 } from '../../core/helpers/file.helper';
import { ManagerHistoryForm } from '../../core/models/history.models';
import { SelectionOption, SelectionService } from '../../core/services/selection.service';
import { pictureToDataUrl } from '../../core/models/person.models';
import { HistoryService } from '../../core/services/history.service';
import { ManagerService, ManagerWriteDto } from '../../core/services/manager.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { fadeSwap } from '../../core/animations/motion';
import { UploadProgress } from '../../shared/components/upload-progress/upload-progress';

function pastDate(control: AbstractControl): ValidationErrors | null {
  if (!control.value) return null;
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return new Date(control.value) < today ? null : { pastDate: true };
}

@Component({
  selector: 'app-manage-managers',
  imports: [CommonModule, ReactiveFormsModule, UploadProgress],
  templateUrl: './manage-managers.html',
  styleUrl: './manage-managers.css',
  animations: [fadeSwap],
})
export class ManageManagers implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private historyService = inject(HistoryService);
  private managerService = inject(ManagerService);
  private selectionService = inject(SelectionService);
  private confirmDialog = inject(ConfirmDialogService);
  private cdr = inject(ChangeDetectorRef);
  private fb = inject(FormBuilder);

  form = this.fb.group({
    firstName: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(100)]),
    lastName: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(100)]),
    dateOfBirth: this.fb.nonNullable.control('', [Validators.required, pastDate]),
    careerStart: this.fb.control<string | null>(null),
    history: this.fb.group({
      teamSelectionId: this.fb.nonNullable.control(''),
      notes: this.fb.control<string | null>(null),
      wins: this.fb.control<number | null>(null),
      loses: this.fb.control<number | null>(null),
      draw: this.fb.control<number | null>(null),
      startMandatDate: this.fb.control<string | null>(null),
      endMandatDate: this.fb.control<string | null>(null),
    }),
  });

  selections: SelectionOption[] = [];
  activeTab: 'info' | 'history' = 'info';
  editingId: string | null = null;
  isEditMode = false;
  isLoading = false;
  picture: string | null = null;
  pictureChanged = false;
  uploadProgress: number | null = null;
  private historyIds: Pick<ManagerHistoryForm, 'selectionId' | 'mandatId'> = {};

  ngOnInit(): void {
    this.selectionService.getAll(1, 100).subscribe((page) => {
      this.selections = page.items;
      this.cdr.detectChanges();
    });
    this.route.paramMap.subscribe((params) => {
      const id = params.get('id');
      if (id) {
        this.editingId = id;
        this.isEditMode = true;
        this.loadManager(id);
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

  loadManager(id: string): void {
    this.isLoading = true;
    this.managerService.getById(id).subscribe({
      next: (data) => {
        this.form.patchValue({
          firstName: data.firstName ?? '',
          lastName: data.lastName ?? '',
          dateOfBirth: data.dateOfBirth ?? '',
          careerStart: data.careerStart ?? null,
        });
        this.picture = data.picture ?? null;
        this.pictureChanged = false;
        void this.loadHistory(id);
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  private async loadHistory(id: string): Promise<void> {
    const history = await this.historyService.loadManagerHistory(id);
    this.historyIds = { selectionId: history.selectionId, mandatId: history.mandatId };
    this.form.controls.history.patchValue({
      teamSelectionId: history.teamSelectionId ?? '',
      notes: history.selection.notes,
      wins: history.selection.wins,
      loses: history.selection.loses,
      draw: history.selection.draw,
      startMandatDate: history.mandat.startMandatDate,
      endMandatDate: history.mandat.endMandatDate,
    });
    this.isLoading = false;
    this.cdr.detectChanges();
  }

  async onPictureSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.picture = await fileToBase64(file);
    this.pictureChanged = true;
  }

  async saveManager(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { firstName, lastName } = this.form.getRawValue();
    if (this.isEditMode) {
      const confirmed = await this.confirmDialog.confirm({
        message: `Save changes for manager ${firstName} ${lastName}?`,
      });
      if (!confirmed) return;
    }

    const payload = this.buildPayload();
    const onProgress = this.pictureChanged ? (percent: number) => this.setUploadProgress(percent) : undefined;
    if (onProgress) {
      this.activeTab = 'info';
      this.setUploadProgress(0);
    }
    const request$ = this.isEditMode
      ? this.managerService.update(this.editingId!, payload, onProgress)
      : this.managerService.create(payload, onProgress);

    request$.subscribe({
      next: async (saved) => {
        const managerId = this.isEditMode ? this.editingId! : saved.id!;
        try {
          await this.historyService.saveManagerHistory(managerId, this.historyPayload());
        } catch (err) {
          console.error('Could not save manager history:', err);
        }
        this.managerService.notifyListRefresh();
        this.router.navigate(['/manager-list']);
      },
      error: (err) => {
        this.setUploadProgress(null);
        console.error('Could not save the manager:', err);
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/manager-list']);
  }

  private resetForm(): void {
    this.form.reset();
    this.historyIds = {};
    this.activeTab = 'info';
    this.picture = null;
    this.pictureChanged = false;
  }

  private setUploadProgress(percent: number | null): void {
    this.uploadProgress = percent;
    this.cdr.detectChanges();
  }

  private buildPayload(): ManagerWriteDto {
    const value = this.form.getRawValue();
    const payload: ManagerWriteDto = {
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

  private historyPayload(): ManagerHistoryForm {
    const history = this.form.controls.history.getRawValue();
    return {
      selectionId: this.historyIds.selectionId,
      mandatId: this.historyIds.mandatId,
      teamSelectionId: history.teamSelectionId,
      selection: {
        notes: history.notes,
        wins: history.wins,
        loses: history.loses,
        draw: history.draw,
      },
      mandat: {
        startMandatDate: history.startMandatDate,
        endMandatDate: history.endMandatDate,
      },
    };
  }
}
