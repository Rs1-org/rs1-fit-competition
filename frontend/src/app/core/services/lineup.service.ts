import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MatchLineup, SaveLineupPlayer } from '../models/lineup';

@Injectable({ providedIn: 'root' })
export class LineupService {
  private readonly apiUrl = `${environment.apiUrl}/api/match`;
  private http = inject(HttpClient);

  get(publicId: string): Observable<MatchLineup> {
    return this.http.get<MatchLineup>(`${this.apiUrl}/${encodeURIComponent(publicId)}/lineup`);
  }

  save(publicId: string, body: {
    homeFormationPublicId: string | null;
    awayFormationPublicId: string | null;
    players: SaveLineupPlayer[];
  }): Observable<MatchLineup> {
    return this.http.put<MatchLineup>(`${this.apiUrl}/${encodeURIComponent(publicId)}/lineup`, body);
  }

  assignReferee(publicId: string, refereePublicId: string | null): Observable<MatchLineup> {
    return this.http.put<MatchLineup>(`${this.apiUrl}/${encodeURIComponent(publicId)}/referee`, { refereePublicId });
  }
}
