import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/paged-result';
import { Position } from '../models/formation.models';

@Injectable({ providedIn: 'root' })
export class PositionService {
  private readonly apiUrl = `${environment.apiUrl}/api/position`;
  private http = inject(HttpClient);

  getAll(page = 1, pageSize = 10, search?: string): Observable<PagedResult<Position>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    return this.http.get<PagedResult<Position>>(this.apiUrl, { params });
  }

  create(body: { name: string }): Observable<Position> {
    return this.http.post<Position>(this.apiUrl, body);
  }

  update(slug: string, body: { name: string }): Observable<Position> {
    return this.http.put<Position>(`${this.apiUrl}/${slug}`, body);
  }

  delete(id: string): Observable<string> {
    return this.http.delete<string>(`${this.apiUrl}/${id}`);
  }
}
