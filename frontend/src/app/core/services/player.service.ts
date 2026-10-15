import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { trackUpload } from '../helpers/upload-progress.helper';
import { Player } from '../models/person.models';
import { PagedResult } from '../models/paged-result';

export interface PlayerListFilter {
  name?: string;
  positionId?: string;
  subPositionId?: string;
  ageMin?: number | null;
  ageMax?: number | null;
}

export type PlayerWriteDto = Omit<Player, 'id' | 'picture'> & { picture?: string | null };

@Injectable({ providedIn: 'root' })
export class PlayerService {
  private readonly apiUrl = `${environment.apiUrl}/api/player`;
  private http = inject(HttpClient);
  private readonly listRefresh$ = new BehaviorSubject<void>(undefined);

  readonly listRefreshChanges$ = this.listRefresh$.asObservable();

  notifyListRefresh(): void {
    this.listRefresh$.next();
  }

  getAll(page = 1, pageSize = 10, filter: PlayerListFilter = {}): Observable<PagedResult<Player>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (filter.name) params = params.set('name', filter.name);
    if (filter.positionId) params = params.set('positionId', filter.positionId);
    if (filter.subPositionId) params = params.set('subPositionId', filter.subPositionId);
    if (filter.ageMin != null) params = params.set('ageMin', filter.ageMin);
    if (filter.ageMax != null) params = params.set('ageMax', filter.ageMax);
    return this.http.get<PagedResult<Player>>(this.apiUrl, { params });
  }

  getById(id: string): Observable<Player> {
    return this.http.get<Player>(`${this.apiUrl}/${id}`);
  }

  create(data: PlayerWriteDto, onProgress?: (percent: number) => void): Observable<Player> {
    if (!onProgress) return this.http.post<Player>(this.apiUrl, data);
    return trackUpload(
      this.http.post<Player>(this.apiUrl, data, { reportProgress: true, observe: 'events' }),
      onProgress,
    );
  }

  update(id: string, data: PlayerWriteDto, onProgress?: (percent: number) => void): Observable<Player> {
    if (!onProgress) return this.http.put<Player>(`${this.apiUrl}/${id}`, data);
    return trackUpload(
      this.http.put<Player>(`${this.apiUrl}/${id}`, data, { reportProgress: true, observe: 'events' }),
      onProgress,
    );
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
