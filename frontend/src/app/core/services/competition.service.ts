import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/paged-result';
import { CompetitionDetailsDto, CompetitionListDto, SaveCompetitionDto } from '../models/competition';

@Injectable({ providedIn: 'root' })
export class CompetitionService {
  private readonly apiUrl = `${environment.apiUrl}/api/competition`;
  private http = inject(HttpClient);

  getAll(page = 1, pageSize = 10, search?: string, countryPublicId?: string): Observable<PagedResult<CompetitionListDto>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    if (countryPublicId) params = params.set('countryPublicId', countryPublicId);
    return this.http.get<PagedResult<CompetitionListDto>>(`${this.apiUrl}/getAll`, { params });
  }

  getBySlug(slug: string): Observable<CompetitionDetailsDto> {
    return this.http.get<CompetitionDetailsDto>(`${this.apiUrl}/get/${slug}`);
  }

  create(body: SaveCompetitionDto): Observable<CompetitionDetailsDto> {
    return this.http.post<CompetitionDetailsDto>(`${this.apiUrl}/create`, body);
  }

  update(slug: string, body: SaveCompetitionDto): Observable<CompetitionDetailsDto> {
    return this.http.put<CompetitionDetailsDto>(`${this.apiUrl}/update/${slug}`, body);
  }

  delete(publicId: string): Observable<string> {
    return this.http.delete<string>(`${this.apiUrl}/delete/${publicId}`);
  }
}
