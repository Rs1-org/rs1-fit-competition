import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { fileToBase64 } from '../../core/helpers/file.helper';
import { PlayerAppearance, PlayerHistoryForm } from '../../core/models/history.models';
import { SelectionOption, SelectionService } from '../../core/services/selection.service';
import { pictureToDataUrl } from '../../core/models/person.models';
import { HistoryService } from '../../core/services/history.service';
import { PlayerService, PlayerWriteDto } from '../../core/services/player.service';
import { PositionService } from '../../core/services/position.service';
import { SubPositionService } from '../../core/services/subposition.service';
import { Position, SubPosition } from '../../core/models/formation.models';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { fadeSwap } from '../../core/animations/motion';
import { UploadProgress } from '../../shared/components/upload-progress/upload-progress';
import { AppDatePipe } from '../../shared/pipes/regional-format.pipes';

function pastDate(control: AbstractControl): ValidationErrors | null {
  if (!control.value) return null;
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return new Date(control.value) < today ? null : { pastDate: true };
}

function positionWhenSubPosition(group: AbstractControl): ValidationErrors | null {
  const positionId = group.get('positionId')?.value;
  const subPositionId = group.get('subPositionId')?.value;
  return subPositionId && !positionId ? { positionRequired: true } : null;
}

@Component({
  selector: 'app-manage-players',
  imports: [CommonModule, ReactiveFormsModule, UploadProgress, AppDatePipe],
  templateUrl: './manage-players.html',
  styleUrl: './manage-players.css',
  animations: [fadeSwap],
})
export class ManagePlayers implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private historyService = inject(HistoryService);
  private playerService = inject(PlayerService);
  private positionService = inject(PositionService);
  private subPositionService = inject(SubPositionService);
  private selectionService = inject(SelectionService);
  private confirmDialog = inject(ConfirmDialogService);
  private cdr = inject(ChangeDetectorRef);
  private fb = inject(FormBuilder);

  form = this.fb.group({
    firstName: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(100)]),
    lastName: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(100)]),
    dateOfBirth: this.fb.nonNullable.control('', [Validators.required, pastDate]),
    height: this.fb.control<number | null>(null, [Validators.required, Validators.min(0.01), Validators.max(300)]),
    shirtNumber: this.fb.control<number | null>(null, [Validators.required, Validators.min(1), Validators.max(99)]),
    transferValue: this.fb.control<number | null>(0, [Validators.required, Validators.min(0)]),
    positionId: this.fb.nonNullable.control(''),
    subPositionId: this.fb.nonNullable.control(''),
    history: this.fb.group({
      teamSelectionId: this.fb.nonNullable.control(''),
      goals: this.fb.control<number | null>(null),
      matchPlayed: this.fb.control<number | null>(null),
      assists: this.fb.control<number | null>(null),
      shirtNumber: this.fb.control<number | null>(null),
      transferDate: this.fb.control<string | null>(null),
      transferValue: this.fb.control<number | null>(null),
      startMandatDate: this.fb.control<string | null>(null),
      endMandatDate: this.fb.control<string | null>(null),
    }),
  }, { validators: positionWhenSubPosition });

  activeTab: 'info' | 'history' = 'info';
  editingId: string | null = null;
  isEditMode = false;
  isLoading = false;
  picture: string | null = null;
  pictureChanged = false;
  uploadProgress: number | null = null;
  positions: Position[] = [];
  subPositions: SubPosition[] = [];
  selections: SelectionOption[] = [];
  appearances: PlayerAppearance[] = [];
  private historyIds: Pick<PlayerHistoryForm, 'selectionId' | 'transferId' | 'mandatId'> = {};

  constructor() {
    this.form.controls.positionId.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => {
      const current = this.form.controls.subPositionId.value;
      if (current && !this.filteredSubPositions.some((item) => item.id === current)) {
        this.form.controls.subPositionId.setValue('');
      }
    });
  }

  ngOnInit(): void {
    this.positionService.getAll(1, 100).subscribe((page) => {
      this.positions = page.items;
      this.cdr.detectChanges();
    });
    this.subPositionService.getAll(1, 100).subscribe((page) => {
      this.subPositions = page.items;
      this.cdr.detectChanges();
    });
    this.selectionService.getAll(1, 100).subscribe((page) => {
      this.selections = page.items;
      this.cdr.detectChanges();
    });
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

  get picturePreview(): string | null {
    return pictureToDataUrl(this.picture);
  }

  get filteredSubPositions(): SubPosition[] {
    const positionId = this.form.controls.positionId.value;
    if (!positionId) return [];
    return this.subPositions.filter((item) => item.positionId === positionId);
  }

  fieldError(name: 'firstName' | 'lastName' | 'dateOfBirth' | 'height' | 'shirtNumber' | 'transferValue' | 'subPositionId'): string | null {
    if (name === 'subPositionId' && this.form.hasError('positionRequired') && this.form.controls.subPositionId.touched) {
      return 'Position is required when a subposition is set.';
    }
    const control = this.form.controls[name];
    if (!control.touched || control.valid) return null;
    if (control.hasError('required')) return 'This field is required.';
    if (control.hasError('maxlength')) return 'Maximum length is 100 characters.';
    if (control.hasError('pastDate')) return 'Date of birth must be in the past.';
    if (control.hasError('min') || control.hasError('max')) return 'Value is out of range.';
    return 'Invalid value.';
  }

  loadPlayer(id: string): void {
    this.isLoading = true;
    this.playerService.getById(id).subscribe({
      next: (data) => {
        this.form.patchValue({
          firstName: data.firstName ?? '',
          lastName: data.lastName ?? '',
          dateOfBirth: data.dateOfBirth ?? '',
          height: data.height,
          shirtNumber: data.shirtNumber,
          transferValue: data.transferValue,
          positionId: data.positionId ?? '',
          subPositionId: data.subPositionId ?? '',
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
    const history = await this.historyService.loadPlayerHistory(id);
    this.historyIds = {
      selectionId: history.selectionId,
      transferId: history.transferId,
      mandatId: history.mandatId,
    };
    this.form.controls.history.patchValue({
      teamSelectionId: history.teamSelectionId ?? '',
      goals: history.selection.goals,
      matchPlayed: history.selection.matchPlayed,
      assists: history.selection.assists,
      shirtNumber: history.selection.shirtNumber,
      transferDate: history.transfer.date,
      transferValue: history.transfer.value,
      startMandatDate: history.mandat.startMandatDate,
      endMandatDate: history.mandat.endMandatDate,
    });
    this.appearances = await this.historyService.loadAppearances(id);
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

  async savePlayer(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { firstName, lastName } = this.form.getRawValue();
    if (this.isEditMode) {
      const confirmed = await this.confirmDialog.confirm({
        message: `Save changes for player ${firstName} ${lastName}?`,
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
      ? this.playerService.update(this.editingId!, payload, onProgress)
      : this.playerService.create(payload, onProgress);

    request$.subscribe({
      next: async (saved) => {
        const playerId = this.isEditMode ? this.editingId! : saved.id!;
        try {
          await this.historyService.savePlayerHistory(playerId, this.historyPayload());
        } catch (err) {
          console.error('Could not save player history:', err);
        }
        this.playerService.notifyListRefresh();
        this.router.navigate(['/player-list']);
      },
      error: (err) => {
        this.setUploadProgress(null);
        console.error('Could not save the player:', err);
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/player-list']);
  }

  private resetForm(): void {
    this.form.reset({ transferValue: 0, positionId: '', subPositionId: '' });
    this.historyIds = {};
    this.appearances = [];
    this.activeTab = 'info';
    this.picture = null;
    this.pictureChanged = false;
  }

  private setUploadProgress(percent: number | null): void {
    this.uploadProgress = percent;
    this.cdr.detectChanges();
  }

  private buildPayload(): PlayerWriteDto {
    const value = this.form.getRawValue();
    const payload: PlayerWriteDto = {
      firstName: value.firstName.trim(),
      lastName: value.lastName.trim(),
      dateOfBirth: value.dateOfBirth,
      picture: this.picture,
      height: Number(value.height),
      shirtNumber: Number(value.shirtNumber),
      transferValue: Number(value.transferValue),
      positionId: value.positionId || null,
      subPositionId: value.subPositionId || null,
    };

    if (this.isEditMode && !this.pictureChanged) {
      const { picture: _picture, ...rest } = payload;
      return rest;
    }

    return payload;
  }

  private historyPayload(): PlayerHistoryForm {
    const history = this.form.controls.history.getRawValue();
    return {
      selectionId: this.historyIds.selectionId,
      transferId: this.historyIds.transferId,
      mandatId: this.historyIds.mandatId,
      teamSelectionId: history.teamSelectionId,
      selection: {
        goals: history.goals,
        matchPlayed: history.matchPlayed,
        assists: history.assists,
        shirtNumber: history.shirtNumber,
      },
      transfer: {
        date: history.transferDate,
        value: history.transferValue,
      },
      mandat: {
        startMandatDate: history.startMandatDate,
        endMandatDate: history.endMandatDate,
      },
    };
  }
}
