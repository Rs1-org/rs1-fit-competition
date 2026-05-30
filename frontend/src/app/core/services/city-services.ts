import { inject, Injectable } from "@angular/core";
import { environment } from '../../../environments/environment';
import { Observable } from "rxjs";
import { CityDetailsDto, CityListDto, CityName } from "../models/city";
import { HttpClient, HttpParams } from "@angular/common/http";
import { PagedResult } from "../models/paged-result";

@Injectable({
    providedIn:'root'
})

export class CityService{
    private apiUrl=`${environment.apiUrl}/api/city/`;

    private http=inject(HttpClient);

    create(city:any):Observable<CityName>{
        return this.http.post<CityName>(`${this.apiUrl}createCity`, city);
    }

    update(slug:string, city:any):Observable<CityName>{
        return this.http.put<CityName>(`${this.apiUrl}updateCity/${slug}`, city);
    }

    delete(id:string):Observable<CityName>{
        return this.http.delete<CityName>(`${this.apiUrl}deleteCity/${id}`);
    }

    getAll(page = 1, pageSize = 10, searchTerm?: string): Observable<PagedResult<CityListDto>> {
        let params = new HttpParams()
            .set('page', page)
            .set('pageSize', pageSize);
        if (searchTerm) params = params.set('searchTerm', searchTerm);
        return this.http.get<PagedResult<CityListDto>>(`${this.apiUrl}getCities`, { params });
    }

    getBySlug(slug: string): Observable<CityDetailsDto> {
        return this.http.get<CityDetailsDto>(`${this.apiUrl}getCity/${slug}`);
    }
}