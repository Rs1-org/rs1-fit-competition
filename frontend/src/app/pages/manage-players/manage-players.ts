import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { fileToBase64 } from '../../core/helpers/file.helper';
import {
  emptyPlayerHistoryForm,
  PlayerHistoryForm,
} from '../../core/models/history.models';
import { emptyPersonFields, pictureToDataUrl, Player } from '../../core/models/person.models';
import { HistoryService } from '../../core/services/history.service';
import { PlayerService, PlayerWriteDto } from '../../core/services/player.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';

@Component({
  selector: 'app-manage-players',
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-players.html',
  styleUrl: './manage-players.css',
})
export class ManagePlayers implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private historyService = inject(HistoryService);
  private playerService = inject(PlayerService);
  private confirmDialog = inject(ConfirmDialogService);
  private cdr = inject(ChangeDetectorRef);

  player: Player = {
    ...emptyPersonFields(),
    height: 0,
    shirtNumber: 0,
    transferValue: 0,
  };

  history: PlayerHistoryForm = emptyPlayerHistoryForm();
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
        this.loadPlayer(id);
        return;
      }

      this.editingId = null;
      this.isEditMode = false;
      this.resetForm();
      this.cdr.detectChanges();
    });
  }

  private resetForm(): void {
    this.player = {
      ...emptyPersonFields(),
      height: 0,
      shirtNumber: 0,
      transferValue: 0,
    };
    this.history = emptyPlayerHistoryForm();
    this.activeTab = 'info';
    this.pictureChanged = false;
  }

  get isPlayerInvalid(): boolean {
    return (
      !this.player.firstName.trim() ||
      !this.player.lastName.trim() ||
      !this.player.dateOfBirth ||
      !this.player.height ||
      this.player.height <= 0 ||
      !this.player.shirtNumber ||
      this.player.shirtNumber <= 0 ||
      !this.player.transferValue ||
      this.player.transferValue <= 0
    );
  }

  get picturePreview(): string | null {
    return pictureToDataUrl(this.player.picture);
  }

  loadPlayer(id: string): void {
    this.isLoading = true;
    this.playerService.getById(id).subscribe({
      next: (data) => {
        this.player = {
          id: data.id,
          firstName: data.firstName ?? '',
          lastName: data.lastName ?? '',
          dateOfBirth: data.dateOfBirth ?? '',
          picture: data.picture ?? null,
          height: data.height,
          shirtNumber: data.shirtNumber,
          transferValue: data.transferValue,
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
    this.history = await this.historyService.loadPlayerHistory(id);
    this.isLoading = false;
    this.cdr.detectChanges();
  }

  async onPictureSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.player.picture = await fileToBase64(file);
    this.pictureChanged = true;
  }

  async savePlayer(): Promise<void> {
    if (this.isPlayerInvalid) return;

    if (this.isEditMode) {
      const confirmed = await this.confirmDialog.confirm({
        message: `Jeste li sigurni da želite spremiti promjene za igrača ${this.player.firstName} ${this.player.lastName}?`,
      });
      if (!confirmed) return;
    }

    const payload = this.buildPayload();
    const request$ = this.isEditMode
      ? this.playerService.update(this.editingId!, payload)
      : this.playerService.create(payload);

    request$.subscribe({
      next: async (saved) => {
        const playerId = this.isEditMode ? this.editingId! : saved.id!;
        try {
          await this.historyService.savePlayerHistory(playerId, this.history);
        } catch (err) {
          console.error('Greška pri spremanju historije igrača:', err);
        }
        this.playerService.notifyListRefresh();
        this.router.navigate(['/player-list']);
      },
      error: (err) => console.error('Greška pri spremanju igrača:', err),
    });
  }

  cancel(): void {
    this.router.navigate(['/player-list']);
  }

  private buildPayload(): PlayerWriteDto {
    const payload: PlayerWriteDto = {
      firstName: this.player.firstName.trim(),
      lastName: this.player.lastName.trim(),
      dateOfBirth: this.player.dateOfBirth,
      picture: this.player.picture,
      height: this.player.height,
      shirtNumber: this.player.shirtNumber,
      transferValue: this.player.transferValue,
    };

    if (this.isEditMode && !this.pictureChanged) {
      const { picture: _picture, ...rest } = payload;
      return rest;
    }

    return payload;
  }
}
