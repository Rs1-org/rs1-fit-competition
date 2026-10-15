import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { trackUpload } from '../helpers/upload-progress.helper';
import { Referee } from '../models/person.models';
import { PagedResult } from '../models/paged-result';

export interface RefereeListFilter {
  name?: string;
  ageMin?: number | null;
  ageMax?: number | null;
  careerStartFrom?: string;
  careerStartTo?: string;
}

export type RefereeWriteDto = Omit<Referee, 'id' | 'picture'> & { picture?: string | null };

@Injectable({ providedIn: 'root' })
export class RefereeService {
  private readonly apiUrl = `${environment.apiUrl}/api/referee`;
  private http = inject(HttpClient);
  private readonly listRefresh$ = new BehaviorSubject<void>(undefined);

  readonly listRefreshChanges$ = this.listRefresh$.asObservable();

  notifyListRefresh(): void {
    this.listRefresh$.next();
  }

  getAll(page = 1, pageSize = 10, filter: RefereeListFilter = {}): Observable<PagedResult<Referee>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (filter.name) params = params.set('name', filter.name);
    if (filter.ageMin != null) params = params.set('ageMin', filter.ageMin);
    if (filter.ageMax != null) params = params.set('ageMax', filter.ageMax);
    if (filter.careerStartFrom) params = params.set('careerStartFrom', filter.careerStartFrom);
    if (filter.careerStartTo) params = params.set('careerStartTo', filter.careerStartTo);
    return this.http.get<PagedResult<Referee>>(this.apiUrl, { params });
  }

  getById(id: string): Observable<Referee> {
    return this.http.get<Referee>(`${this.apiUrl}/${id}`);
  }

  create(data: RefereeWriteDto, onProgress?: (percent: number) => void): Observable<Referee> {
    if (!onProgress) return this.http.post<Referee>(this.apiUrl, data);
    return trackUpload(
      this.http.post<Referee>(this.apiUrl, data, { reportProgress: true, observe: 'events' }),
      onProgress,
    );
  }

  update(id: string, data: RefereeWriteDto, onProgress?: (percent: number) => void): Observable<Referee> {
    if (!onProgress) return this.http.put<Referee>(`${this.apiUrl}/${id}`, data);
    return trackUpload(
      this.http.put<Referee>(`${this.apiUrl}/${id}`, data, { reportProgress: true, observe: 'events' }),
      onProgress,
    );
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
