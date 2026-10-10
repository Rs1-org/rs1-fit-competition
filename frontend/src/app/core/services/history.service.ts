import { Injectable } from '@angular/core';
import {
  emptyManagerHistoryForm,
  emptyPlayerHistoryForm,
  hasManagerHistoryData,
  hasPlayerHistoryData,
  ManagerHistoryForm,
  MandatDate,
  PlayerAppearance,
  PlayerHistoryForm,
  PlayerSelectionHistory,
  SelectionManagerHistory,
  TransferValueHistory,
} from '../models/history.models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class HistoryService {
  private readonly baseUrl = environment.apiUrl;

  async loadPlayerHistory(playerId: string): Promise<PlayerHistoryForm> {
    const [selections, transfers, mandats] = await Promise.all([
      this.fetchAll<PlayerSelectionHistory>('/api/player-selection-history'),
      this.fetchAll<TransferValueHistory>('/api/transfer-value-history'),
      this.fetchAll<MandatDate>('/api/mandat-date'),
    ]);

    const form = emptyPlayerHistoryForm();
    const selection = selections.find((item) => item.playerId === playerId);
    const transfer = transfers.find((item) => item.playerId === playerId);

    if (selection) {
      form.selectionId = selection.id;
      form.teamSelectionId = selection.selectionId ?? '';
      form.selection = {
        goals: selection.goals ?? null,
        matchPlayed: selection.matchPlayed ?? null,
        assists: selection.assists ?? null,
        shirtNumber: selection.shirtNumber ?? null,
      };
      const mandat = mandats.find(
        (item) => item.playerSelectionHistoryId === selection.id,
      );
      if (mandat) {
        form.mandatId = mandat.id;
        form.mandat = {
          startMandatDate: mandat.startMandatDate ?? null,
          endMandatDate: mandat.endMandatDate ?? null,
        };
      }
    }

    if (transfer) {
      form.transferId = transfer.id;
      form.transfer = {
        date: transfer.date ?? null,
        value: transfer.value ?? null,
      };
    }

    return form;
  }

  async loadManagerHistory(managerId: string): Promise<ManagerHistoryForm> {
    const [selections, mandats] = await Promise.all([
      this.fetchAll<SelectionManagerHistory>('/api/selection-manager-history'),
      this.fetchAll<MandatDate>('/api/mandat-date'),
    ]);

    const form = emptyManagerHistoryForm();
    const selection = selections.find((item) => item.managerId === managerId);

    if (selection) {
      form.selectionId = selection.id;
      form.teamSelectionId = selection.selectionId ?? '';
      form.selection = {
        notes: selection.notes ?? null,
        wins: selection.wins ?? null,
        loses: selection.loses ?? null,
        draw: selection.draw ?? null,
      };
      const mandat = mandats.find(
        (item) => item.selectionManagerHistoryId === selection.id,
      );
      if (mandat) {
        form.mandatId = mandat.id;
        form.mandat = {
          startMandatDate: mandat.startMandatDate ?? null,
          endMandatDate: mandat.endMandatDate ?? null,
        };
      }
    }

    return form;
  }

  async savePlayerHistory(
    playerId: string,
    form: PlayerHistoryForm,
  ): Promise<void> {
    if (!hasPlayerHistoryData(form)) return;

    let selectionHistoryId = form.selectionId;

    const hasSelectionInput =
      !!form.teamSelectionId ||
      form.selection.goals != null ||
      form.selection.matchPlayed != null ||
      form.selection.assists != null ||
      form.selection.shirtNumber != null;

    const hasMandatInput =
      !!form.mandat.startMandatDate || !!form.mandat.endMandatDate;

    if (hasSelectionInput || (hasMandatInput && !selectionHistoryId)) {
      const payload = {
        goals: form.selection.goals,
        matchPlayed: form.selection.matchPlayed,
        assists: form.selection.assists,
        shirtNumber: form.selection.shirtNumber,
        playerId,
        selectionId: form.teamSelectionId || null,
      };
      selectionHistoryId = await this.upsert(
        '/api/player-selection-history',
        form.selectionId,
        payload,
      );
    }

    const hasTransferInput =
      !!form.transfer.date || form.transfer.value != null;

    if (hasTransferInput) {
      await this.upsert('/api/transfer-value-history', form.transferId, {
        date: form.transfer.date || null,
        value: form.transfer.value,
        playerId,
      });
    }

    if (hasMandatInput && selectionHistoryId) {
      await this.upsert('/api/mandat-date', form.mandatId, {
        startMandatDate: form.mandat.startMandatDate || null,
        endMandatDate: form.mandat.endMandatDate || null,
        playerSelectionHistoryId: selectionHistoryId,
        selectionManagerHistoryId: null,
      });
    }
  }

  async saveManagerHistory(
    managerId: string,
    form: ManagerHistoryForm,
  ): Promise<void> {
    if (!hasManagerHistoryData(form)) return;

    let selectionHistoryId = form.selectionId;

    const hasSelectionInput =
      !!form.teamSelectionId ||
      !!form.selection.notes?.trim() ||
      form.selection.wins != null ||
      form.selection.loses != null ||
      form.selection.draw != null;

    const hasMandatInput =
      !!form.mandat.startMandatDate || !!form.mandat.endMandatDate;

    if (hasSelectionInput || (hasMandatInput && !selectionHistoryId)) {
      selectionHistoryId = await this.upsert(
        '/api/selection-manager-history',
        form.selectionId,
        {
          notes: form.selection.notes?.trim() || null,
          wins: form.selection.wins,
          loses: form.selection.loses,
          draw: form.selection.draw,
          managerId,
          selectionId: form.teamSelectionId || null,
        },
      );
    }

    if (hasMandatInput && selectionHistoryId) {
      await this.upsert('/api/mandat-date', form.mandatId, {
        startMandatDate: form.mandat.startMandatDate || null,
        endMandatDate: form.mandat.endMandatDate || null,
        playerSelectionHistoryId: null,
        selectionManagerHistoryId: selectionHistoryId,
      });
    }
  }

  async loadAppearances(playerId: string): Promise<PlayerAppearance[]> {
    const response = await fetch(`${this.baseUrl}/api/player/${playerId}/appearances`);
    if (!response.ok) return [];
    return response.json();
  }

  private async fetchAll<T>(path: string): Promise<T[]> {
    const response = await fetch(`${this.baseUrl}${path}`);
    if (!response.ok) return [];
    return response.json();
  }

  private async upsert(
    path: string,
    id: string | undefined,
    payload: Record<string, unknown>,
  ): Promise<string> {
    const url = id
      ? `${this.baseUrl}${path}/${id}`
      : `${this.baseUrl}${path}`;
    const response = await fetch(url, {
      method: id ? 'PUT' : 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });

    if (!response.ok) {
      throw new Error(`Failed to save ${path}`);
    }

    const data = await response.json();
    return data.id ?? id ?? '';
  }
}
