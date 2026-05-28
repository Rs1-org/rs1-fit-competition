import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Referee } from '../models/person.models';

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

  getAll(): Observable<Referee[]> {
    return this.http.get<Referee[]>(this.apiUrl);
  }

  getById(id: string): Observable<Referee> {
    return this.http.get<Referee>(`${this.apiUrl}/${id}`);
  }

  search(term: string): Observable<Referee[]> {
    const params = new HttpParams().set('term', term);
    return this.http.get<Referee[]>(`${this.apiUrl}/search`, { params });
  }

  create(data: RefereeWriteDto): Observable<Referee> {
    return this.http.post<Referee>(this.apiUrl, data);
  }

  update(id: string, data: RefereeWriteDto): Observable<Referee> {
    return this.http.put<Referee>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
