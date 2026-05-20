import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { fileToBase64 } from '../../core/helpers/file.helper';
import { emptyPersonFields, Referee } from '../../core/models/person.models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-manage-referees',
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-referees.html',
  styleUrl: './manage-referees.css',
})
export class ManageReferees {
  referee: Referee = {
    ...emptyPersonFields(),
    careerStart: null,
  };

  constructor(private router: Router) {}

  get isRefereeInvalid(): boolean {
    return !this.referee.firstName.trim() || !this.referee.lastName.trim();
  }

  async onPictureSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      this.referee.picture = null;
      return;
    }
    this.referee.picture = await fileToBase64(file);
  }

  async saveReferee(): Promise<void> {
    const payload = {
      firstName: this.referee.firstName.trim(),
      lastName: this.referee.lastName.trim(),
      dateOfBirth: this.referee.dateOfBirth || '00:00:00',
      picture: this.referee.picture,
      careerStart: this.referee.careerStart || null,
    };

    const response = await fetch(`${environment.apiUrl}/api/referee`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });

    if (response.ok) {
      this.resetForm();
      this.router.navigate(['/referee-list']);
    }
  }

  cancel(): void {
    this.resetForm();
  }

  private resetForm(): void {
    this.referee = {
      ...emptyPersonFields(),
      careerStart: null,
    };
  }
}
