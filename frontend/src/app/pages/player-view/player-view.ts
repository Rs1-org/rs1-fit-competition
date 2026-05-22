import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import {
  hasPlayerHistoryData,
  playerHistoryViewLabels,
  PlayerHistoryForm,
} from '../../core/models/history.models';
import { pictureToDataUrl } from '../../core/models/person.models';
import { HistoryService } from '../../core/services/history.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-player-view',
  imports: [CommonModule, RouterModule],
  templateUrl: './player-view.html',
  styleUrl: './player-view.css',
})
export class PlayerView implements OnInit {
  selectedPlayer: Record<string, unknown> | null = null;
  pictureUrl: string | null = null;
  history: PlayerHistoryForm | null = null;
  historyRows: { label: string; value: unknown }[] = [];
  activeTab: 'info' | 'history' = 'info';
  showHistoryTab = false;

  private readonly labels = playerHistoryViewLabels();

  constructor(
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
    private router: Router,
    private historyService: HistoryService,
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    void this.loadPlayer(id);
  }

  get displayProperties(): { key: string; value: unknown }[] {
    if (!this.selectedPlayer) return [];
    const hiddenFields = ['id', 'deletedAt', 'id_key', 'picture'];
    return Object.keys(this.selectedPlayer)
      .filter((key) => !hiddenFields.includes(key))
      .map((key) => ({
        key: key.charAt(0).toUpperCase() + key.slice(1),
        value: this.selectedPlayer![key],
      }));
  }

  goBack(): void {
    this.router.navigate(['/player-list']);
  }

  private async loadPlayer(id: string): Promise<void> {
    try {
      const response = await fetch(`${environment.apiUrl}/api/player/${id}`);
      if (!response.ok) return;

      const data = await response.json();
      this.selectedPlayer = {
        firstName: data.firstName,
        lastName: data.lastName,
        dateOfBirth: data.dateOfBirth,
        height: data.height,
        shirtNumber: data.shirtNumber,
        transferValue: data.transferValue,
      };
      this.pictureUrl = pictureToDataUrl(data.picture);

      this.history = await this.historyService.loadPlayerHistory(id);
      this.showHistoryTab = hasPlayerHistoryData(this.history);
      this.historyRows = this.buildHistoryRows(this.history);
      this.cdr.detectChanges();
    } catch (err) {
      console.error('Greška pri dohvaćanju:', err);
    }
  }

  private buildHistoryRows(
    form: PlayerHistoryForm,
  ): { label: string; value: unknown }[] {
    const rows: { label: string; value: unknown }[] = [];
    const { selection, transfer, mandat } = form;

    if (selection.goals != null) {
      rows.push({ label: this.labels['goals'], value: selection.goals });
    }
    if (selection.matchPlayed != null) {
      rows.push({
        label: this.labels['matchPlayed'],
        value: selection.matchPlayed,
      });
    }
    if (selection.assists != null) {
      rows.push({ label: this.labels['assists'], value: selection.assists });
    }
    if (selection.shirtNumber != null) {
      rows.push({
        label: this.labels['shirtNumber'],
        value: selection.shirtNumber,
      });
    }
    if (transfer.date) {
      rows.push({
        label: this.labels['transferDate'],
        value: transfer.date,
      });
    }
    if (transfer.value != null) {
      rows.push({
        label: this.labels['transferValue'],
        value: transfer.value,
      });
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
