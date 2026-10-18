import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ClubDetailsDto, ClubListDto, ClubListFilter, SaveClubDto } from '../models/club';
import { PagedResult } from '../models/paged-result';
import { toPagedParams } from '../helpers/paging.helper';

@Injectable({ providedIn: 'root' })
export class ClubService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/club`;

  getAll(
    page = 1,
    pageSize = 10,
    filter: ClubListFilter = {},
  ): Observable<PagedResult<ClubListDto>> {
    return this.http.get<PagedResult<ClubListDto>>(this.apiUrl, {
      params: toPagedParams(page, pageSize, filter),
    });
  }

  getBySlug(slug: string): Observable<ClubDetailsDto> {
    return this.http.get<ClubDetailsDto>(`${this.apiUrl}/get/${encodeURIComponent(slug)}`);
  }

  create(body: SaveClubDto): Observable<ClubDetailsDto> {
    return this.http.post<ClubDetailsDto>(`${this.apiUrl}/create`, body);
  }

  update(slug: string, body: SaveClubDto): Observable<ClubDetailsDto> {
    return this.http.put<ClubDetailsDto>(`${this.apiUrl}/update/${encodeURIComponent(slug)}`, body);
  }

  delete(publicId: string): Observable<{ publicId: string }> {
    return this.http.delete<{ publicId: string }>(
      `${this.apiUrl}/delete/${encodeURIComponent(publicId)}`,
    );
  }
}
