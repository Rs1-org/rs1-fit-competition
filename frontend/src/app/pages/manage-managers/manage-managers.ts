import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { fileToBase64 } from '../../core/helpers/file.helper';
import {
  emptyManagerHistoryForm,
  ManagerHistoryForm,
} from '../../core/models/history.models';
import { emptyPersonFields, pictureToDataUrl, Manager } from '../../core/models/person.models';
import { HistoryService } from '../../core/services/history.service';
import { ManagerService, ManagerWriteDto } from '../../core/services/manager.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';

@Component({
  selector: 'app-manage-managers',
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-managers.html',
  styleUrl: './manage-managers.css',
})
export class ManageManagers implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private historyService = inject(HistoryService);
  private managerService = inject(ManagerService);
  private confirmDialog = inject(ConfirmDialogService);
  private cdr = inject(ChangeDetectorRef);

  manager: Manager = {
    ...emptyPersonFields(),
    careerStart: null,
  };

  history: ManagerHistoryForm = emptyManagerHistoryForm();
  activeTab: 'info' | 'history' = 'info';
  editingId: string | null = null;
  isEditMode = false;
  isLoading = false;
  pictureChanged = false;

  ngOnInit(): void {
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

  private resetForm(): void {
    this.manager = {
      ...emptyPersonFields(),
      careerStart: null,
    };
    this.history = emptyManagerHistoryForm();
    this.activeTab = 'info';
    this.pictureChanged = false;
  }

  get isManagerInvalid(): boolean {
    return (
      !this.manager.firstName.trim() ||
      !this.manager.lastName.trim() ||
      !this.manager.dateOfBirth
    );
  }

  get picturePreview(): string | null {
    return pictureToDataUrl(this.manager.picture);
  }

  loadManager(id: string): void {
    this.isLoading = true;
    this.managerService.getById(id).subscribe({
      next: (data) => {
        this.manager = {
          id: data.id,
          firstName: data.firstName ?? '',
          lastName: data.lastName ?? '',
          dateOfBirth: data.dateOfBirth ?? '',
          picture: data.picture ?? null,
          careerStart: data.careerStart ?? null,
        };
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
    this.history = await this.historyService.loadManagerHistory(id);
    this.isLoading = false;
    this.cdr.detectChanges();
  }

  async onPictureSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.manager.picture = await fileToBase64(file);
    this.pictureChanged = true;
  }

  async saveManager(): Promise<void> {
    if (this.isManagerInvalid) return;

    if (this.isEditMode) {
      const confirmed = await this.confirmDialog.confirm({
        message: `Jeste li sigurni da želite spremiti promjene za menadžera ${this.manager.firstName} ${this.manager.lastName}?`,
      });
      if (!confirmed) return;
    }

    const payload = this.buildPayload();
    const request$ = this.isEditMode
      ? this.managerService.update(this.editingId!, payload)
      : this.managerService.create(payload);

    request$.subscribe({
      next: async (saved) => {
        const managerId = this.isEditMode ? this.editingId! : saved.id!;
        try {
          await this.historyService.saveManagerHistory(managerId, this.history);
        } catch (err) {
          console.error('Greška pri spremanju historije menadžera:', err);
        }
        this.managerService.notifyListRefresh();
        this.router.navigate(['/manager-list']);
      },
      error: (err) => console.error('Greška pri spremanju menadžera:', err),
    });
  }

  cancel(): void {
    this.router.navigate(['/manager-list']);
  }

  private buildPayload(): ManagerWriteDto {
    const payload: ManagerWriteDto = {
      firstName: this.manager.firstName.trim(),
      lastName: this.manager.lastName.trim(),
      dateOfBirth: this.manager.dateOfBirth,
      picture: this.manager.picture,
      careerStart: this.manager.careerStart || null,
    };

    if (this.isEditMode && !this.pictureChanged) {
      const { picture: _picture, ...rest } = payload;
      return rest;
    }

    return payload;
  }
}
