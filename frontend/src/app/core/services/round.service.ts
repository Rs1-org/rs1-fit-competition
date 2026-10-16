import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/paged-result';
import { RoundDetailsDto, RoundListDto, RoundMatchDto, SaveRoundDto } from '../models/round';

@Injectable({ providedIn: 'root' })
export class RoundService {
  private readonly apiUrl = `${environment.apiUrl}/api/round`;
  private readonly matchApiUrl = `${environment.apiUrl}/api/match`;
  private http = inject(HttpClient);

  getAll(page = 1, pageSize = 10, seasonPublicId?: string, search?: string): Observable<PagedResult<RoundListDto>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (seasonPublicId) params = params.set('seasonPublicId', seasonPublicId);
    if (search) params = params.set('search', search);
    return this.http.get<PagedResult<RoundListDto>>(`${this.apiUrl}/getAll`, { params });
  }

  getBySlug(slug: string): Observable<RoundDetailsDto> {
    return this.http.get<RoundDetailsDto>(`${this.apiUrl}/get/${slug}`);
  }

  getMatches(roundPublicId: string, page = 1, pageSize = 10): Observable<PagedResult<RoundMatchDto>> {
    const params = new HttpParams().set('roundPublicId', roundPublicId).set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<RoundMatchDto>>(`${this.matchApiUrl}/getAll`, { params });
  }

  create(body: SaveRoundDto): Observable<RoundDetailsDto> {
    return this.http.post<RoundDetailsDto>(`${this.apiUrl}/create`, body);
  }

  update(slug: string, body: SaveRoundDto): Observable<RoundDetailsDto> {
    return this.http.put<RoundDetailsDto>(`${this.apiUrl}/update/${slug}`, body);
  }

  delete(publicId: string): Observable<string> {
    return this.http.delete<string>(`${this.apiUrl}/delete/${publicId}`);
  }
}
