import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CityListDto } from '../models/city';
import { ClubResponseDto, CreateClubDto, StadiumOption, UpdateClubDto } from '../models/club';

@Injectable({
  providedIn: 'root'
})
export class ClubService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/Club`;

  create(club: CreateClubDto): Observable<ClubResponseDto> {
    return this.http.post<ClubResponseDto>(`${this.apiUrl}/create`, club);
  }

  update(slug: string, club: UpdateClubDto): Observable<ClubResponseDto> {
    return this.http.put<ClubResponseDto>(`${this.apiUrl}/update/${slug}`, club);
  }

  delete(id: string): Observable<boolean> {
    return this.http.delete<boolean>(`${this.apiUrl}/delete/${id}`);
  }

  getAll(): Observable<ClubResponseDto[]> {
    return this.http.get<ClubResponseDto[]>(`${this.apiUrl}/getAll`);
  }

  get(slug: string): Observable<ClubResponseDto> {
    return this.http.get<ClubResponseDto>(`${this.apiUrl}/get/${slug}`);
  }

  getCities(): Observable<CityListDto[]> {
    return this.http.get<CityListDto[]>(`${environment.apiUrl}/api/City/getCities`);
  }

  getStadiums(): Observable<StadiumOption[]> {
    return this.http.get<StadiumOption[]>(`${environment.apiUrl}/api/Stadium/getStadiums`);
  }
}
