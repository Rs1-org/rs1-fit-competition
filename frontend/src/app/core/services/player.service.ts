import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Player } from '../models/person.models';

export type PlayerWriteDto = Omit<Player, 'id' | 'picture'> & { picture?: string | null };

@Injectable({ providedIn: 'root' })
export class PlayerService {
  private readonly apiUrl = `${environment.apiUrl}/api/player`;
  private http = inject(HttpClient);

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

  create(data: PlayerWriteDto): Observable<Player> {
    return this.http.post<Player>(this.apiUrl, data);
  }

  update(id: string, data: PlayerWriteDto): Observable<Player> {
    return this.http.put<Player>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
