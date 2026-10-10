import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { trackUpload } from '../helpers/upload-progress.helper';
import { Player } from '../models/person.models';

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

  getAll(): Observable<Player[]> {
    return this.http.get<Player[]>(this.apiUrl);
  }

  getById(id: string): Observable<Player> {
    return this.http.get<Player>(`${this.apiUrl}/${id}`);
  }

  search(term: string): Observable<Player[]> {
    const params = new HttpParams().set('term', term);
    return this.http.get<Player[]>(`${this.apiUrl}/search`, { params });
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
