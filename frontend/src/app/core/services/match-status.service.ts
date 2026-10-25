import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MatchStatusListDto } from '../models/match-status';
import { PagedResult } from '../models/paged-result';

@Injectable({ providedIn: 'root' })
export class MatchStatusService {
  private readonly apiUrl = `${environment.apiUrl}/api/matchstatus`;
  private http = inject(HttpClient);

  getAll(page = 1, pageSize = 20): Observable<PagedResult<MatchStatusListDto>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<MatchStatusListDto>>(this.apiUrl, { params });
  }
}
