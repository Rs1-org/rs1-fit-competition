import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/paged-result';
import { MatchDetailsDto, MatchListDto, MatchListFilter, SaveMatchDto } from '../models/match';
import { toPagedParams } from '../helpers/paging.helper';

@Injectable({ providedIn: 'root' })
export class MatchService {
  private readonly apiUrl = `${environment.apiUrl}/api/match`;
  private http = inject(HttpClient);

  getAll(page = 1, pageSize = 10, filter: MatchListFilter = {}): Observable<PagedResult<MatchListDto>> {
    return this.http.get<PagedResult<MatchListDto>>(`${this.apiUrl}/getAll`, {
      params: toPagedParams(page, pageSize, filter),
    });
  }

  getBySlug(slug: string): Observable<MatchDetailsDto> {
    return this.http.get<MatchDetailsDto>(`${this.apiUrl}/get/${encodeURIComponent(slug)}`);
  }

  /** The backend generates the slug from the teams and date. */
  create(body: SaveMatchDto): Observable<MatchDetailsDto> {
    return this.http.post<MatchDetailsDto>(`${this.apiUrl}/create`, body);
  }

  update(slug: string, body: SaveMatchDto): Observable<MatchDetailsDto> {
    return this.http.put<MatchDetailsDto>(`${this.apiUrl}/update/${encodeURIComponent(slug)}`, body);
  }

  delete(publicId: string): Observable<string> {
    return this.http.delete<string>(`${this.apiUrl}/delete/${encodeURIComponent(publicId)}`);
  }
}
