import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom, Observable } from 'rxjs';
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
  publicId?: string;
  startDate: string | null;
  endDate: string | null;
  playerSelectionHistoryPublicId?: string | null;
  selectionManagerHistoryPublicId?: string | null;
}

@Injectable({ providedIn: 'root' })
export class HistoryService {
  private http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  playerSelections(selectionId: string): Observable<PagedResult<PlayerSelectionHistory>> {
    const params = new HttpParams().set('page', '1').set('pageSize', '100').set('selectionPublicId', selectionId);
    return this.http.get<PagedResult<PlayerSelectionHistory>>(`${this.baseUrl}/api/player-selection-history`, { params });
  }

  managerSelections(selectionId: string): Observable<PagedResult<SelectionManagerHistory>> {
    const params = new HttpParams().set('page', '1').set('pageSize', '100').set('selectionPublicId', selectionId);
    return this.http.get<PagedResult<SelectionManagerHistory>>(`${this.baseUrl}/api/selection-manager-history`, { params });
  }

  async loadPlayerHistory(playerId: string): Promise<PlayerHistoryForm> {
    const [selections, transfers] = await Promise.all([
      this.list<PlayerSelectionHistory>('/api/player-selection-history', { playerPublicId: playerId }),
      this.list<TransferValueHistory>('/api/transfer-value-history', { playerPublicId: playerId }),
    ]);

    const form = emptyPlayerHistoryForm();
    const selection = selections[0];
    const transfer = transfers[0];

    if (selection) {
      form.selectionId = selection.publicId;
      form.teamSelectionId = selection.selectionPublicId ?? '';
      form.selection = {
        goals: selection.goals ?? null,
        matchPlayed: selection.matchPlayed ?? null,
        assists: selection.assists ?? null,
        shirtNumber: selection.shirtNumber ?? null,
      };
      const mandates = await this.list<MandateDate>('/api/mandate-date', {
        playerSelectionHistoryPublicId: selection.publicId,
      });
      const mandate = mandates[0];
      if (mandate) {
        form.mandatId = mandate.publicId;
        form.mandat = {
          startMandatDate: mandate.startDate ?? null,
          endMandatDate: mandate.endDate ?? null,
        };
      }
    }

    if (transfer) {
      form.transferId = transfer.publicId;
      form.transfer = {
        date: transfer.date ?? null,
        value: transfer.value ?? null,
      };
    }

    return form;
  }

  async loadManagerHistory(managerId: string): Promise<ManagerHistoryForm> {
    const selections = await this.list<SelectionManagerHistory>('/api/selection-manager-history', { managerPublicId: managerId });
    const form = emptyManagerHistoryForm();
    const selection = selections[0];

    if (selection) {
      form.selectionId = selection.publicId;
      form.teamSelectionId = selection.selectionPublicId ?? '';
      form.selection = {
        notes: selection.notes ?? null,
        wins: selection.wins ?? null,
        loses: selection.loses ?? null,
        draw: selection.draw ?? null,
      };
      const mandates = await this.list<MandateDate>('/api/mandate-date', {
        selectionManagerHistoryPublicId: selection.publicId,
      });
      const mandate = mandates[0];
      if (mandate) {
        form.mandatId = mandate.publicId;
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
        playerPublicId: playerId,
        selectionPublicId: form.teamSelectionId || null,
      });
    }

    if (!!form.transfer.date || form.transfer.value != null) {
      await this.upsert('/api/transfer-value-history', form.transferId, {
        date: form.transfer.date || null,
        value: form.transfer.value,
        playerPublicId: playerId,
      });
    }

    if (hasMandateInput && selectionHistoryId) {
      await this.upsert('/api/mandate-date', form.mandatId, {
        startDate: form.mandat.startMandatDate || null,
        endDate: form.mandat.endMandatDate || null,
        playerSelectionHistoryPublicId: selectionHistoryId,
        selectionManagerHistoryPublicId: null,
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
        managerPublicId: managerId,
        selectionPublicId: form.teamSelectionId || null,
      });
    }

    if (hasMandateInput && selectionHistoryId) {
      await this.upsert('/api/mandate-date', form.mandatId, {
        startDate: form.mandat.startMandatDate || null,
        endDate: form.mandat.endMandatDate || null,
        playerSelectionHistoryPublicId: null,
        selectionManagerHistoryPublicId: selectionHistoryId,
      });
    }
  }

  loadTransfers(playerId: string): Promise<TransferValueHistory[]> {
    return this.list<TransferValueHistory>('/api/transfer-value-history', { playerPublicId: playerId });
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

  private async list<T extends { publicId?: string }>(path: string, filter: Record<string, string | undefined>): Promise<T[]> {
    let params = new HttpParams().set('page', '1').set('pageSize', '20');
    for (const [key, value] of Object.entries(filter)) {
      if (value) params = params.set(key, value);
    }
    const page = await firstValueFrom(
      this.http.get<PagedResult<T>>(`${this.baseUrl}${path}`, { params }),
    );
    return page.items ?? [];
  }

  private async upsert<T extends { publicId?: string }>(
    path: string,
    id: string | undefined,
    payload: Record<string, unknown>,
  ): Promise<string> {
    const url = id ? `${this.baseUrl}${path}/${id}` : `${this.baseUrl}${path}`;
    const request = id
      ? this.http.put<T>(url, payload)
      : this.http.post<T>(url, payload);
    const data = await firstValueFrom(request);
    return data.publicId ?? id ?? '';
  }
}
