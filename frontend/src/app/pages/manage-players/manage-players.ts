import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { fileToBase64 } from '../../core/helpers/file.helper';
import { emptyPersonFields, Player } from '../../core/models/person.models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-manage-players',
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-players.html',
  styleUrl: './manage-players.css',
})
export class ManagePlayers implements OnInit {
  player: Player = {
    ...emptyPersonFields(),
    height: 0,
    shirtNumber: 0,
    transferValue: 0,
  };

  editingId: string | null = null;
  isEditMode = false;
  pictureChanged = false;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.editingId = id;
      this.isEditMode = true;
      void this.loadPlayer(id);
    }
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

  async loadPlayer(id: string): Promise<void> {
    const response = await fetch(`${environment.apiUrl}/api/player/${id}`);
    if (!response.ok) return;

    const data = await response.json();
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
  }

  async onPictureSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.player.picture = await fileToBase64(file);
    this.pictureChanged = true;
  }

  async savePlayer(): Promise<void> {
    const payload: Record<string, unknown> = {
      firstName: this.player.firstName.trim(),
      lastName: this.player.lastName.trim(),
      dateOfBirth: this.player.dateOfBirth,
      height: this.player.height,
      shirtNumber: this.player.shirtNumber,
      transferValue: this.player.transferValue,
    };

    if (!this.isEditMode || this.pictureChanged) {
      payload['picture'] = this.player.picture;
    }

    const url = this.isEditMode
      ? `${environment.apiUrl}/api/player/${this.editingId}`
      : `${environment.apiUrl}/api/player`;

    const response = await fetch(url, {
      method: this.isEditMode ? 'PUT' : 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });

    if (response.ok) {
      this.router.navigate(['/player-list']);
    }
  }

  cancel(): void {
    this.router.navigate(['/player-list']);
  }
}
