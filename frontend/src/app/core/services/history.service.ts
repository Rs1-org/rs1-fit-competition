import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import {
  emptyManagerHistoryForm,
  emptyPlayerHistoryForm,
  hasManagerHistoryData,
  hasPlayerHistoryData,
  ManagerHistoryForm,
  PlayerAppearance,
  PlayerHistoryForm,
  PlayerSelectionHistory,
  SelectionManagerHistory,
  TransferValueHistory,
} from '../models/history.models';
import { PagedResult } from '../models/paged-result';
import { environment } from '../../../environments/environment';

interface MandateDate {
  id?: string;
  startDate: string | null;
  endDate: string | null;
  playerSelectionHistoryId?: string | null;
  selectionManagerHistoryId?: string | null;
}

@Injectable({ providedIn: 'root' })
export class HistoryService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  async loadPlayerHistory(playerId: string): Promise<PlayerHistoryForm> {
    const [selections, transfers] = await Promise.all([
      this.list<PlayerSelectionHistory>('/api/player-selection-history', { playerId }),
      this.list<TransferValueHistory>('/api/transfer-value-history', { playerId }),
    ]);

    const form = emptyPlayerHistoryForm();
    const selection = selections[0];
    const transfer = transfers[0];

    if (selection) {
      form.selectionId = selection.id;
      form.teamSelectionId = selection.selectionId ?? '';
      form.selection = {
        goals: selection.goals ?? null,
        matchPlayed: selection.matchPlayed ?? null,
        assists: selection.assists ?? null,
        shirtNumber: selection.shirtNumber ?? null,
      };
      const mandates = await this.list<MandateDate>('/api/mandate-date', {
        playerSelectionHistoryId: selection.id,
      });
      const mandate = mandates[0];
      if (mandate) {
        form.mandatId = mandate.id;
        form.mandat = {
          startMandatDate: mandate.startDate ?? null,
          endMandatDate: mandate.endDate ?? null,
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
    const selections = await this.list<SelectionManagerHistory>('/api/selection-manager-history', { managerId });
    const form = emptyManagerHistoryForm();
    const selection = selections[0];

    if (selection) {
      form.selectionId = selection.id;
      form.teamSelectionId = selection.selectionId ?? '';
      form.selection = {
        notes: selection.notes ?? null,
        wins: selection.wins ?? null,
        loses: selection.loses ?? null,
        draw: selection.draw ?? null,
      };
      const mandates = await this.list<MandateDate>('/api/mandate-date', {
        selectionManagerHistoryId: selection.id,
      });
      const mandate = mandates[0];
      if (mandate) {
        form.mandatId = mandate.id;
        form.mandat = {
          startMandatDate: mandate.startDate ?? null,
          endMandatDate: mandate.endDate ?? null,
        };
      }
    }

    return form;
  }

  async savePlayerHistory(playerId: string, form: PlayerHistoryForm): Promise<void> {
    if (!hasPlayerHistoryData(form)) return;

    let selectionHistoryId = form.selectionId;
    const hasSelectionInput =
      !!form.teamSelectionId ||
      form.selection.goals != null ||
      form.selection.matchPlayed != null ||
      form.selection.assists != null ||
      form.selection.shirtNumber != null;
    const hasMandateInput = !!form.mandat.startMandatDate || !!form.mandat.endMandatDate;

    if (hasSelectionInput || (hasMandateInput && !selectionHistoryId)) {
      selectionHistoryId = await this.upsert<PlayerSelectionHistory>('/api/player-selection-history', form.selectionId, {
        goals: form.selection.goals,
        matchPlayed: form.selection.matchPlayed,
        assists: form.selection.assists,
        shirtNumber: form.selection.shirtNumber,
        playerId,
        selectionId: form.teamSelectionId || null,
      });
    }

    if (!!form.transfer.date || form.transfer.value != null) {
      await this.upsert('/api/transfer-value-history', form.transferId, {
        date: form.transfer.date || null,
        value: form.transfer.value,
        playerId,
      });
    }

    if (hasMandateInput && selectionHistoryId) {
      await this.upsert('/api/mandate-date', form.mandatId, {
        startDate: form.mandat.startMandatDate || null,
        endDate: form.mandat.endMandatDate || null,
        playerSelectionHistoryId: selectionHistoryId,
        selectionManagerHistoryId: null,
      });
    }
  }

  async saveManagerHistory(managerId: string, form: ManagerHistoryForm): Promise<void> {
    if (!hasManagerHistoryData(form)) return;

    let selectionHistoryId = form.selectionId;
    const hasSelectionInput =
      !!form.teamSelectionId ||
      !!form.selection.notes?.trim() ||
      form.selection.wins != null ||
      form.selection.loses != null ||
      form.selection.draw != null;
    const hasMandateInput = !!form.mandat.startMandatDate || !!form.mandat.endMandatDate;

    if (hasSelectionInput || (hasMandateInput && !selectionHistoryId)) {
      selectionHistoryId = await this.upsert<SelectionManagerHistory>('/api/selection-manager-history', form.selectionId, {
        notes: form.selection.notes?.trim() || null,
        wins: form.selection.wins,
        loses: form.selection.loses,
        draw: form.selection.draw,
        managerId,
        selectionId: form.teamSelectionId || null,
      });
    }

    if (hasMandateInput && selectionHistoryId) {
      await this.upsert('/api/mandate-date', form.mandatId, {
        startDate: form.mandat.startMandatDate || null,
        endDate: form.mandat.endMandatDate || null,
        playerSelectionHistoryId: null,
        selectionManagerHistoryId: selectionHistoryId,
      });
    }
  }

  async loadAppearances(playerId: string): Promise<PlayerAppearance[]> {
    try {
      return await firstValueFrom(
        this.http.get<PlayerAppearance[]>(`${this.baseUrl}/api/player/${playerId}/appearances`),
      );
    } catch {
      return [];
    }
  }

  private async list<T extends { id?: string }>(path: string, filter: Record<string, string | undefined>): Promise<T[]> {
    let params = new HttpParams().set('page', '1').set('pageSize', '20');
    for (const [key, value] of Object.entries(filter)) {
      if (value) params = params.set(key, value);
    }
    const page = await firstValueFrom(
      this.http.get<PagedResult<T>>(`${this.baseUrl}${path}`, { params }),
    );
    return page.items ?? [];
  }

  private async upsert<T extends { id?: string }>(
    path: string,
    id: string | undefined,
    payload: Record<string, unknown>,
  ): Promise<string> {
    const url = id ? `${this.baseUrl}${path}/${id}` : `${this.baseUrl}${path}`;
    const request = id
      ? this.http.put<T>(url, payload)
      : this.http.post<T>(url, payload);
    const data = await firstValueFrom(request);
    return data.id ?? id ?? '';
  }
}
