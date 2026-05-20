import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { fileToBase64 } from '../../core/helpers/file.helper';
import { emptyPersonFields, Manager } from '../../core/models/person.models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-manage-managers',
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-managers.html',
  styleUrl: './manage-managers.css',
})
export class ManageManagers implements OnInit {
  manager: Manager = {
    ...emptyPersonFields(),
    careerStart: null,
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
      void this.loadManager(id);
    }
  }

  get isManagerInvalid(): boolean {
    return (
      !this.manager.firstName.trim() ||
      !this.manager.lastName.trim() ||
      !this.manager.dateOfBirth
    );
  }

  async loadManager(id: string): Promise<void> {
    const response = await fetch(`${environment.apiUrl}/api/manager/${id}`);
    if (!response.ok) return;

    const data = await response.json();
    this.manager = {
      id: data.id,
      firstName: data.firstName ?? '',
      lastName: data.lastName ?? '',
      dateOfBirth: data.dateOfBirth ?? '',
      picture: data.picture ?? null,
      careerStart: data.careerStart ?? null,
    };
    this.pictureChanged = false;
  }

  async onPictureSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.manager.picture = await fileToBase64(file);
    this.pictureChanged = true;
  }

  async saveManager(): Promise<void> {
    const payload: Record<string, unknown> = {
      firstName: this.manager.firstName.trim(),
      lastName: this.manager.lastName.trim(),
      dateOfBirth: this.manager.dateOfBirth,
      careerStart: this.manager.careerStart || null,
    };

    if (!this.isEditMode || this.pictureChanged) {
      payload['picture'] = this.manager.picture;
    }

    const url = this.isEditMode
      ? `${environment.apiUrl}/api/manager/${this.editingId}`
      : `${environment.apiUrl}/api/manager`;

    const response = await fetch(url, {
      method: this.isEditMode ? 'PUT' : 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });

    if (response.ok) {
      this.router.navigate(['/manager-list']);
    }
  }

  cancel(): void {
    this.router.navigate(['/manager-list']);
  }
}
