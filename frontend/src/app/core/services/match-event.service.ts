import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { EventContext, EventFilter, EventInput, EventLookups, MatchEvent } from '../models/match-event';
import { PagedResult } from '../models/paged-result';
import { toPagedParams } from '../helpers/paging.helper';

@Injectable({ providedIn: 'root' })
export class MatchEventService {
  private readonly http = inject(HttpClient);
  private readonly api = `${environment.apiUrl}/api/events`;
  getPage(filter: EventFilter, page = 1) { return this.http.get<PagedResult<MatchEvent>>(this.api, { params: toPagedParams(page, 20, filter) }); }
  get(id: string) { return this.http.get<MatchEvent>(`${this.api}/${encodeURIComponent(id)}`); }
  lookups() { return this.http.get<EventLookups>(`${this.api}/lookups`); }
  context(match: string) { return this.http.get<EventContext>(`${this.api}/match/${encodeURIComponent(match)}/context`); }
  create(input: EventInput) { return this.http.post<{ publicId: string }>(this.api, input); }
  update(id: string, input: EventInput) { return this.http.put<{ publicId: string }>(`${this.api}/${encodeURIComponent(id)}`, input); }
  delete(id: string) { return this.http.delete<{ publicId: string }>(`${this.api}/${encodeURIComponent(id)}`); }
}
