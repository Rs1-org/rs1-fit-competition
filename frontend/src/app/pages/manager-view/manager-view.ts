import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import {
  hasManagerHistoryData,
  managerHistoryViewLabels,
  ManagerHistoryForm,
} from '../../core/models/history.models';
import { pictureToDataUrl } from '../../core/models/person.models';
import { HistoryService } from '../../core/services/history.service';
import { ManagerService } from '../../core/services/manager.service';

@Component({
  selector: 'app-manager-view',
  imports: [CommonModule, RouterModule],
  templateUrl: './manager-view.html',
  styleUrl: './manager-view.css',
})
export class ManagerView implements OnInit {
  private route = inject(ActivatedRoute);
  private cdr = inject(ChangeDetectorRef);
  private router = inject(Router);
  private historyService = inject(HistoryService);
  private managerService = inject(ManagerService);

  selectedManager: Record<string, unknown> | null = null;
  pictureUrl: string | null = null;
  history: ManagerHistoryForm | null = null;
  historyRows: { label: string; value: unknown }[] = [];
  activeTab: 'info' | 'history' = 'info';
  showHistoryTab = false;

  private readonly labels = managerHistoryViewLabels();

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    this.loadManager(id);
  }

  get displayProperties(): { key: string; value: unknown }[] {
    if (!this.selectedManager) return [];
    const hiddenFields = ['id', 'deletedAt', 'id_key', 'picture'];
    return Object.keys(this.selectedManager)
      .filter((key) => !hiddenFields.includes(key))
      .map((key) => ({
        key: key.charAt(0).toUpperCase() + key.slice(1),
        value: this.selectedManager![key],
      }));
  }

  goBack(): void {
    this.router.navigate(['/manager-list']);
  }

  private loadManager(id: string): void {
    this.managerService.getById(id).subscribe({
      next: async (data) => {
        this.selectedManager = {
          firstName: data.firstName,
          lastName: data.lastName,
          dateOfBirth: data.dateOfBirth,
          careerStart: data.careerStart || '—',
        };
        this.pictureUrl = pictureToDataUrl(data.picture);

        this.history = await this.historyService.loadManagerHistory(id);
        this.showHistoryTab = hasManagerHistoryData(this.history);
        this.historyRows = this.buildHistoryRows(this.history);
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Greška pri dohvaćanju:', err),
    });
  }

  private buildHistoryRows(
    form: ManagerHistoryForm,
  ): { label: string; value: unknown }[] {
    const rows: { label: string; value: unknown }[] = [];
    const { selection, mandat } = form;

    if (selection.notes?.trim()) {
      rows.push({ label: this.labels['notes'], value: selection.notes });
    }
    if (selection.wins != null) {
      rows.push({ label: this.labels['wins'], value: selection.wins });
    }
    if (selection.loses != null) {
      rows.push({ label: this.labels['loses'], value: selection.loses });
    }
    if (selection.draw != null) {
      rows.push({ label: this.labels['draw'], value: selection.draw });
    }
    if (mandat.startMandatDate) {
      rows.push({
        label: this.labels['startMandatDate'],
        value: mandat.startMandatDate,
      });
    }
    if (mandat.endMandatDate) {
      rows.push({
        label: this.labels['endMandatDate'],
        value: mandat.endMandatDate,
      });
    }

    return rows;
  }
}
