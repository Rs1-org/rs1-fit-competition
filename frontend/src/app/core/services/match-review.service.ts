import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { MatchReviewPage, ReviewInput, ReviewMatch, ReviewMatchDetails } from '../models/match-review';
import { PagedResult } from '../models/paged-result';
import { toPagedParams } from '../helpers/paging.helper';

@Injectable({ providedIn: 'root' })
export class MatchReviewService {
  private readonly http = inject(HttpClient);
  private readonly api = environment.apiUrl;
  getMatches(page = 1) { return this.http.get<PagedResult<ReviewMatch>>(`${this.api}/api/Match/getAll`, { params: toPagedParams(page, 10) }); }
  getMatch(id: string) { return this.http.get<ReviewMatchDetails>(`${this.api}/api/Match/getByPublicId/${encodeURIComponent(id)}`); }
  getReviews(id: string, page = 1) {
    return this.http.get<MatchReviewPage>(`${this.api}/api/reviews/match/${encodeURIComponent(id)}`, { params: toPagedParams(page, 10) });
  }
  create(id: string, input: ReviewInput) { return this.http.post<{ publicId: string }>(`${this.api}/api/reviews/match/${encodeURIComponent(id)}`, input); }
  update(id: string, input: ReviewInput) { return this.http.put<{ publicId: string }>(`${this.api}/api/reviews/${encodeURIComponent(id)}`, input); }
  delete(id: string) { return this.http.delete<{ publicId: string }>(`${this.api}/api/reviews/${encodeURIComponent(id)}`); }
  react(id: string, value: -1 | 1) { return this.http.put<{ value: number | null }>(`${this.api}/api/reviews/${encodeURIComponent(id)}/reaction`, { value }); }
}
