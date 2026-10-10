import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/paged-result';
import { Formation } from '../models/formation.models';

@Injectable({ providedIn: 'root' })
export class FormationService {
  private readonly apiUrl = `${environment.apiUrl}/api/formation`;
  private http = inject(HttpClient);

  getAll(page = 1, pageSize = 10, search?: string): Observable<PagedResult<Formation>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    return this.http.get<PagedResult<Formation>>(this.apiUrl, { params });
  }

  getBySlug(slug: string): Observable<Formation> {
    return this.http.get<Formation>(`${this.apiUrl}/${slug}`);
  }

  create(body: { code: string; formationMarkId: string }): Observable<Formation> {
    return this.http.post<Formation>(this.apiUrl, body);
  }

  update(slug: string, body: { code: string; formationMarkId: string }): Observable<Formation> {
    return this.http.put<Formation>(`${this.apiUrl}/${slug}`, body);
  }

  delete(id: string): Observable<string> {
    return this.http.delete<string>(`${this.apiUrl}/${id}`);
  }
}
