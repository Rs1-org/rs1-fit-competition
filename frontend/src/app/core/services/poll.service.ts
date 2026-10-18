import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Poll, PollInput } from '../models/poll';
import { PagedResult } from '../models/paged-result';
import { toPagedParams } from '../helpers/paging.helper';

@Injectable({ providedIn: 'root' })
export class PollService {
  private readonly http = inject(HttpClient);
  private readonly api = `${environment.apiUrl}/api/polls`;
  getPage(match: string, page = 1) { return this.http.get<PagedResult<Poll>>(`${this.api}/match/${encodeURIComponent(match)}`, { params: toPagedParams(page, 10) }); }
  create(match: string, input: PollInput) { return this.http.post<{ publicId: string }>(`${this.api}/match/${encodeURIComponent(match)}`, input); }
  vote(poll: string, optionPublicId: string) { return this.http.post<{ publicId: string }>(`${this.api}/${encodeURIComponent(poll)}/vote`, { optionPublicId }); }
}
