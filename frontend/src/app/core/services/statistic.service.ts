import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { HeadToHead, PlayerStats, Statistic, StatisticChartFilter, StatisticCharts, StatisticFilter, StatisticInput, TeamForm } from '../models/statistic';
import { PagedResult } from '../models/paged-result';
import { toFilterParams, toPagedParams } from '../helpers/paging.helper';

@Injectable({ providedIn: 'root' })
export class StatisticService {
  private readonly http = inject(HttpClient);
  private readonly api = `${environment.apiUrl}/api/statistic`;
  getPage(filter: StatisticFilter, page = 1) { return this.http.get<PagedResult<Statistic>>(this.api, { params: toPagedParams(page, 10, filter) }); }
  getCharts(filter: StatisticChartFilter) { return this.http.get<StatisticCharts>(`${this.api}/charts`, { params: toFilterParams(filter) }); }
  getTeamForm(selectionPublicId: string, last: number, filter: StatisticChartFilter) { return this.http.get<TeamForm>(`${this.api}/advanced/team-form`, { params: toFilterParams({ selectionPublicId, last, ...filter }) }); }
  getHeadToHead(firstSelectionPublicId: string, secondSelectionPublicId: string, filter: StatisticChartFilter) { return this.http.get<HeadToHead>(`${this.api}/advanced/head-to-head`, { params: toFilterParams({ firstSelectionPublicId, secondSelectionPublicId, ...filter }) }); }
  getPlayerStats(filter: StatisticChartFilter & { selectionPublicId?: string }, page = 1) { return this.http.get<PagedResult<PlayerStats>>(`${this.api}/advanced/players`, { params: toPagedParams(page, 10, filter) }); }
  get(id: string) { return this.http.get<Statistic>(`${this.api}/${encodeURIComponent(id)}`); }
  getForMatch(match: string) { return this.http.get<Statistic>(`${this.api}/match/${encodeURIComponent(match)}`); }
  create(matchPublicId: string, input: StatisticInput) { return this.http.post<Statistic>(this.api, { matchPublicId, ...input }); }
  update(publicId: string, input: StatisticInput) { return this.http.put<Statistic>(`${this.api}/${encodeURIComponent(publicId)}`, input); }
}
