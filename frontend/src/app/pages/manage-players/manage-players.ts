import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { fileToBase64 } from '../../core/helpers/file.helper';
import { emptyPersonFields, Player } from '../../core/models/person.models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-manage-players',
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-players.html',
  styleUrl: './manage-players.css',
})
export class ManagePlayers {
  player: Player = {
    ...emptyPersonFields(),
    height: 0,
    shirtNumber: 0,
    transferValue: 0,
  };

  constructor(private router: Router) {}

  get isPlayerInvalid(): boolean {
    return (
      !this.player.firstName.trim() ||
      !this.player.lastName.trim() ||
      !this.player.height ||
      this.player.height <= 0 ||
      !this.player.shirtNumber ||
      this.player.shirtNumber <= 0 ||
      !this.player.transferValue ||
      this.player.transferValue <= 0
    );
  }

  async onPictureSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      this.player.picture = null;
      return;
    }
    this.player.picture = await fileToBase64(file);
  }

  async savePlayer(): Promise<void> {
    const payload = {
      firstName: this.player.firstName.trim(),
      lastName: this.player.lastName.trim(),
      dateOfBirth: this.player.dateOfBirth || '00:00:00',
      picture: this.player.picture,
      height: this.player.height,
      shirtNumber: this.player.shirtNumber,
      transferValue: this.player.transferValue,
    };

    const response = await fetch(`${environment.apiUrl}/api/player`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });

    if (response.ok) {
      this.resetForm();
      this.router.navigate(['/player-list']);
    }
  }

  cancel(): void {
    this.resetForm();
  }

  private resetForm(): void {
    this.player = {
      ...emptyPersonFields(),
      height: 0,
      shirtNumber: 0,
      transferValue: 0,
    };
  }
}
