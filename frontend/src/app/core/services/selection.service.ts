import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/paged-result';

export interface SelectionOption {
  id: string;
  name: string;
  abbreviation: string;
  pictureUrl: string | null;
  followersCount: number;
  slug: string;
  countryId: string;
  countryName: string | null;
}

@Injectable({ providedIn: 'root' })
export class SelectionService {
  private readonly apiUrl = `${environment.apiUrl}/api/selection`;
  private http = inject(HttpClient);

  getAll(page = 1, pageSize = 10, search?: string): Observable<PagedResult<SelectionOption>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    return this.http.get<PagedResult<SelectionOption>>(this.apiUrl, { params });
  }

  getBySlug(slug: string): Observable<SelectionOption> {
    return this.http.get<SelectionOption>(`${this.apiUrl}/${slug}`);
  }

  create(body: { name: string; abbreviation: string; followersCount: number; countryId: string; pictureUrl?: string | null }): Observable<SelectionOption> {
    return this.http.post<SelectionOption>(this.apiUrl, body);
  }

  update(slug: string, body: { name: string; abbreviation: string; followersCount: number; countryId: string; pictureUrl?: string | null }): Observable<SelectionOption> {
    return this.http.put<SelectionOption>(`${this.apiUrl}/${slug}`, body);
  }

  delete(id: string): Observable<string> {
    return this.http.delete<string>(`${this.apiUrl}/${id}`);
  }
}
