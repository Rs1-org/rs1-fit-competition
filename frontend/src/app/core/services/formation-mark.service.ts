import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/paged-result';
import { FormationMark } from '../models/formation.models';

@Injectable({ providedIn: 'root' })
export class FormationMarkService {
  private readonly apiUrl = `${environment.apiUrl}/api/formation-mark`;
  private http = inject(HttpClient);

  getAll(page = 1, pageSize = 10, search?: string): Observable<PagedResult<FormationMark>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    return this.http.get<PagedResult<FormationMark>>(this.apiUrl, { params });
  }

  getBySlug(slug: string): Observable<FormationMark> {
    return this.http.get<FormationMark>(`${this.apiUrl}/${slug}`);
  }

  create(body: Pick<FormationMark, 'defenders' | 'midfielders' | 'attackers'>): Observable<FormationMark> {
    return this.http.post<FormationMark>(this.apiUrl, body);
  }

  update(slug: string, body: Pick<FormationMark, 'defenders' | 'midfielders' | 'attackers'>): Observable<FormationMark> {
    return this.http.put<FormationMark>(`${this.apiUrl}/${slug}`, body);
  }

  delete(id: string): Observable<string> {
    return this.http.delete<string>(`${this.apiUrl}/${id}`);
  }
}
