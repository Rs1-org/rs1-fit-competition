import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { toPagedParams } from '../helpers/paging.helper';
import { Club, ClubFilter, ClubInput } from '../models/club';
import { PagedResult } from '../models/paged-result';

@Injectable({ providedIn: 'root' })
export class ClubService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/api/club`;

  getAll(page = 1, pageSize = 10, filter: ClubFilter = {}) {
    return this.http.get<PagedResult<Club>>(this.url, { params: toPagedParams(page, pageSize, filter) });
  }
  getBySlug(slug: string) { return this.http.get<Club>(`${this.url}/${encodeURIComponent(slug)}`); }
  create(input: ClubInput) { return this.http.post<Club>(this.url, input); }
  update(slug: string, input: ClubInput) { return this.http.put<Club>(`${this.url}/${encodeURIComponent(slug)}`, input); }
  delete(id: string) { return this.http.delete<string>(`${this.url}/${id}`); }
}
