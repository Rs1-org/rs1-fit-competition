import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/paged-result';
import { SubPosition } from '../models/formation.models';

@Injectable({ providedIn: 'root' })
export class SubPositionService {
  private readonly apiUrl = `${environment.apiUrl}/api/subposition`;
  private http = inject(HttpClient);

  getAll(page = 1, pageSize = 10, search?: string, positionId?: string): Observable<PagedResult<SubPosition>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    if (positionId) params = params.set('positionId', positionId);
    return this.http.get<PagedResult<SubPosition>>(this.apiUrl, { params });
  }

  create(body: { name: string; positionId: string }): Observable<SubPosition> {
    return this.http.post<SubPosition>(this.apiUrl, body);
  }

  update(slug: string, body: { name: string; positionId: string }): Observable<SubPosition> {
    return this.http.put<SubPosition>(`${this.apiUrl}/${slug}`, body);
  }

  delete(id: string): Observable<string> {
    return this.http.delete<string>(`${this.apiUrl}/${id}`);
  }
}
