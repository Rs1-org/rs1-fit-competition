import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Statistic, StatisticFilter, StatisticInput } from '../models/statistic';
import { PagedResult } from '../models/paged-result';
import { toPagedParams } from '../helpers/paging.helper';

@Injectable({ providedIn: 'root' })
export class StatisticService {
  private readonly http = inject(HttpClient);
  private readonly api = `${environment.apiUrl}/api/statistic`;
  getPage(filter: StatisticFilter, page = 1) { return this.http.get<PagedResult<Statistic>>(this.api, { params: toPagedParams(page, 10, filter) }); }
  get(id: string) { return this.http.get<Statistic>(`${this.api}/${encodeURIComponent(id)}`); }
  getForMatch(match: string) { return this.http.get<Statistic>(`${this.api}/match/${encodeURIComponent(match)}`); }
  create(matchPublicId: string, input: StatisticInput) { return this.http.post<Statistic>(this.api, { matchPublicId, ...input }); }
  update(publicId: string, input: StatisticInput) { return this.http.put<Statistic>(`${this.api}/${encodeURIComponent(publicId)}`, input); }
}
