import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { fileToBase64 } from '../../core/helpers/file.helper';
import { emptyPersonFields, pictureToDataUrl, Referee } from '../../core/models/person.models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-manage-referees',
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-referees.html',
  styleUrl: './manage-referees.css',
})
export class ManageReferees implements OnInit {
  referee: Referee = {
    ...emptyPersonFields(),
    careerStart: null,
  };

  editingId: string | null = null;
  isEditMode = false;
  pictureChanged = false;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const id = params.get('id');
      if (id) {
        this.editingId = id;
        this.isEditMode = true;
        void this.loadReferee(id);
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

  async loadReferee(id: string): Promise<void> {
    const response = await fetch(`${environment.apiUrl}/api/referee/${id}`);
    if (!response.ok) return;

    const data = await response.json();
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
  }

  async onPictureSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.referee.picture = await fileToBase64(file);
    this.pictureChanged = true;
  }

  async saveReferee(): Promise<void> {
    const payload: Record<string, unknown> = {
      firstName: this.referee.firstName.trim(),
      lastName: this.referee.lastName.trim(),
      dateOfBirth: this.referee.dateOfBirth,
      careerStart: this.referee.careerStart || null,
    };

    if (!this.isEditMode || this.pictureChanged) {
      payload['picture'] = this.referee.picture;
    }

    const url = this.isEditMode
      ? `${environment.apiUrl}/api/referee/${this.editingId}`
      : `${environment.apiUrl}/api/referee`;

    const response = await fetch(url, {
      method: this.isEditMode ? 'PUT' : 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });

    if (response.ok) {
      this.router.navigate(['/referee-list']);
    }
  }

  cancel(): void {
    this.router.navigate(['/referee-list']);
  }
}
