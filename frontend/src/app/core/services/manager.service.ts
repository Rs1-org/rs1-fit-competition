import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Manager } from '../models/person.models';

export type ManagerWriteDto = Omit<Manager, 'id' | 'picture'> & { picture?: string | null };

@Injectable({ providedIn: 'root' })
export class ManagerService {
  private readonly apiUrl = `${environment.apiUrl}/api/manager`;
  private http = inject(HttpClient);
  private readonly listRefresh$ = new BehaviorSubject<void>(undefined);

  readonly listRefreshChanges$ = this.listRefresh$.asObservable();

  notifyListRefresh(): void {
    this.listRefresh$.next();
  }

  getAll(): Observable<Manager[]> {
    return this.http.get<Manager[]>(this.apiUrl);
  }

  getById(id: string): Observable<Manager> {
    return this.http.get<Manager>(`${this.apiUrl}/${id}`);
  }

  search(term: string): Observable<Manager[]> {
    const params = new HttpParams().set('term', term);
    return this.http.get<Manager[]>(`${this.apiUrl}/search`, { params });
  }

  create(data: ManagerWriteDto): Observable<Manager> {
    return this.http.post<Manager>(this.apiUrl, data);
  }

  update(id: string, data: ManagerWriteDto): Observable<Manager> {
    return this.http.put<Manager>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
