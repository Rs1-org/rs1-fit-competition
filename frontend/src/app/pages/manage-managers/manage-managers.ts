import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { fileToBase64 } from '../../core/helpers/file.helper';
import { emptyPersonFields, Manager } from '../../core/models/person.models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-manage-managers',
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-managers.html',
  styleUrl: './manage-managers.css',
})
export class ManageManagers {
  manager: Manager = {
    ...emptyPersonFields(),
    careerStart: null,
  };

  constructor(private router: Router) {}

  get isManagerInvalid(): boolean {
    return !this.manager.firstName.trim() || !this.manager.lastName.trim();
  }

  async onPictureSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      this.manager.picture = null;
      return;
    }
    this.manager.picture = await fileToBase64(file);
  }

  async saveManager(): Promise<void> {
    const payload = {
      firstName: this.manager.firstName.trim(),
      lastName: this.manager.lastName.trim(),
      dateOfBirth: this.manager.dateOfBirth || '00:00:00',
      picture: this.manager.picture,
      careerStart: this.manager.careerStart || null,
    };

    const response = await fetch(`${environment.apiUrl}/api/manager`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });

    if (response.ok) {
      this.resetForm();
      this.router.navigate(['/manager-list']);
    }
  }

  cancel(): void {
    this.resetForm();
  }

  private resetForm(): void {
    this.manager = {
      ...emptyPersonFields(),
      careerStart: null,
    };
  }
}
