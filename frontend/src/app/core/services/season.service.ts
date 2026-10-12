import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/paged-result';
import { SaveSeasonDto, SeasonDetailsDto, SeasonListDto } from '../models/season';

@Injectable({ providedIn: 'root' })
export class SeasonService {
  private readonly apiUrl = `${environment.apiUrl}/api/season`;
  private http = inject(HttpClient);

  getAll(page = 1, pageSize = 10, competitionId?: string, search?: string, isCurrent?: boolean): Observable<PagedResult<SeasonListDto>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (competitionId) params = params.set('competitionId', competitionId);
    if (search) params = params.set('search', search);
    if (isCurrent !== undefined) params = params.set('isCurrent', isCurrent);
    return this.http.get<PagedResult<SeasonListDto>>(`${this.apiUrl}/getAll`, { params });
  }

  getBySlug(slug: string): Observable<SeasonDetailsDto> {
    return this.http.get<SeasonDetailsDto>(`${this.apiUrl}/get/${slug}`);
  }

  create(body: SaveSeasonDto): Observable<SeasonDetailsDto> {
    return this.http.post<SeasonDetailsDto>(`${this.apiUrl}/create`, body);
  }

  update(slug: string, body: SaveSeasonDto): Observable<SeasonDetailsDto> {
    return this.http.put<SeasonDetailsDto>(`${this.apiUrl}/update/${slug}`, body);
  }

  delete(id: string): Observable<string> {
    return this.http.delete<string>(`${this.apiUrl}/delete/${id}`);
  }
}
