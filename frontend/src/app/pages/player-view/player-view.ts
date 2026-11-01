import { CommonModule, formatNumber } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import {
  hasPlayerHistoryData,
  PlayerAppearance,
  PlayerHistoryForm,
  TransferValueHistory,
} from '../../core/models/history.models';
import { pictureToDataUrl } from '../../core/models/person.models';
import { HistoryService } from '../../core/services/history.service';
import { PlayerService } from '../../core/services/player.service';
import { fadeSwap } from '../../core/animations/motion';
import { AppDatePipe, AppNumberPipe, REGIONAL_LOCALE } from '../../shared/pipes/regional-format.pipes';
import { Breadcrumb } from '../../shared/components/breadcrumb/breadcrumb';
import { StatTile } from '../../shared/components/stat-tile/stat-tile';
import { Chart, ChartSeries } from '../../shared/components/chart/chart';
import { Skeleton } from '../../shared/components/skeleton/skeleton';
import { EmptyState } from '../../shared/components/empty-state/empty-state';
import { ErrorBanner } from '../../shared/components/error-banner/error-banner';
import { ImageZoom } from '../../shared/components/image-zoom/image-zoom';
import { apiErrorMessage } from '../../core/utils/api-error';

@Component({
  selector: 'app-player-view',
  imports: [CommonModule, RouterModule, AppDatePipe, AppNumberPipe, Breadcrumb, StatTile, Chart, Skeleton, EmptyState, ErrorBanner, ImageZoom],
  templateUrl: './player-view.html',
  styleUrls: ['./player-view.css', './person-profile.css'],
  animations: [fadeSwap],
})
export class PlayerView implements OnInit {
  private route = inject(ActivatedRoute);
  private cdr = inject(ChangeDetectorRef);
  private router = inject(Router);
  private historyService = inject(HistoryService);
  private playerService = inject(PlayerService);

  selectedPlayer: Record<string, unknown> | null = null;
  pictureUrl: string | null = null;
  history: PlayerHistoryForm | null = null;
  historyRows: { label: string; value: unknown }[] = [];
  appearances: PlayerAppearance[] = [];
  activeTab: 'info' | 'history' = 'info';
  showHistoryTab = false;
  loading = true;
  error = '';
  zoom = false;
  transfers: TransferValueHistory[] = [];
  private playerId = '';

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loading = false;
      this.error = 'Player could not be loaded.';
      return;
    }
    this.playerId = id;
    this.loadPlayer(id);
  }

  retry(): void {
    if (this.playerId) this.loadPlayer(this.playerId);
  }

  money(value: unknown): string {
    const amount = Number(value);
    if (value == null || value === '' || Number.isNaN(amount)) return '—';
    return amount >= 1_000_000
      ? `€ ${formatNumber(amount / 1_000_000, REGIONAL_LOCALE, '1.1-1')} M`
      : `€ ${formatNumber(amount, REGIONAL_LOCALE, '1.0-0')}`;
  }

  heightLabel(value: unknown): string {
    const amount = Number(value);
    if (value == null || value === '' || Number.isNaN(amount)) return '—';
    return amount >= 3
      ? `${formatNumber(amount, REGIONAL_LOCALE, '1.0-0')} cm`
      : `${formatNumber(amount, REGIONAL_LOCALE, '1.2-2')} m`;
  }

  career(): { team: string; from: string | null; to: string | null; apps: number }[] {
    const groups = new Map<string, PlayerAppearance[]>();
    for (const appearance of this.appearances) {
      const team = appearance.selectionName || 'Team not recorded';
      groups.set(team, [...(groups.get(team) ?? []), appearance]);
    }
    return [...groups.entries()].map(([team, rows]) => {
      const dates = rows.map((row) => row.matchDate).filter(Boolean).sort();
      return { team, from: dates[0] ?? null, to: dates.at(-1) ?? null, apps: rows.length };
    });
  }

  age(value: unknown): number | null {
    const date = new Date(String(value ?? ''));
    if (Number.isNaN(date.getTime())) return null;
    const today = new Date();
    let years = today.getFullYear() - date.getFullYear();
    if (today.getMonth() < date.getMonth() || (today.getMonth() === date.getMonth() && today.getDate() < date.getDate())) years--;
    return years;
  }

  chartLabels(): string[] {
    return this.chartPoints().map((point) => point.date ?? '');
  }

  chartSeries(): ChartSeries[] {
    return [{ label: 'Value', data: this.chartPoints().map((point) => point.value ?? 0), color: '--ac', type: 'line' }];
  }

  private chartPoints(): TransferValueHistory[] {
    return this.transfers
      .filter((point) => point.date && point.value != null)
      .sort((left, right) => String(left.date).localeCompare(String(right.date)));
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

  private loadPlayer(id: string): void {
    this.loading = true;
    this.error = '';
    this.playerService.getById(id).subscribe({
      next: async (data) => {
        this.selectedPlayer = {
          firstName: data.firstName,
          lastName: data.lastName,
          dateOfBirth: data.dateOfBirth,
          height: data.height,
          shirtNumber: data.shirtNumber,
          transferValue: data.transferValue,
          positionName: data.positionName,
          subPositionName: data.subPositionName,
        };
        this.pictureUrl = pictureToDataUrl(data.picture);
        const [history, appearances, transfers] = await Promise.all([
          this.historyService.loadPlayerHistory(id),
          this.historyService.loadAppearances(id),
          this.historyService.loadTransfers(id),
        ]);
        this.history = history;
        this.appearances = appearances;
        this.transfers = transfers;
        this.showHistoryTab = hasPlayerHistoryData(history) || appearances.length > 0;
        this.historyRows = this.buildHistoryRows(history);
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.loading = false;
        this.error = apiErrorMessage(err, 'Player could not be loaded.');
        this.cdr.detectChanges();
      },
    });
  }

  private buildHistoryRows(
    form: PlayerHistoryForm,
  ): { label: string; value: unknown }[] {
    const rows: { label: string; value: unknown }[] = [];
    const { selection, transfer, mandat } = form;

    if (selection.goals != null) {
      rows.push({ label: 'Goals', value: selection.goals });
    }
    if (selection.matchPlayed != null) {
      rows.push({
        label: 'Appearances',
        value: selection.matchPlayed,
      });
    }
    if (selection.assists != null) {
      rows.push({ label: 'Assists', value: selection.assists });
    }
    if (selection.shirtNumber != null) {
      rows.push({
        label: 'Shirt number',
        value: selection.shirtNumber,
      });
    }
    if (transfer.date) {
      rows.push({
        label: 'Transfer date',
        value: transfer.date,
      });
    }
    if (transfer.value != null) {
      rows.push({
        label: 'Transfer value',
        value: transfer.value,
      });
    }
    if (mandat.startMandatDate) {
      rows.push({
        label: 'From',
        value: mandat.startMandatDate,
      });
    }
    if (mandat.endMandatDate) {
      rows.push({
        label: 'To',
        value: mandat.endMandatDate,
      });
    }

    return rows;
  }
}
