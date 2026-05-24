import { inject, Injectable } from "@angular/core";
import { environment } from '../../../environments/environment';
import { Observable } from "rxjs";
import { CityDetailsDto, CityListDto, CityName } from "../models/city";
import { HttpClient } from "@angular/common/http";

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

    getAll(): Observable<CityListDto[]> {
        return this.http.get<CityListDto[]>(`${this.apiUrl}getCities`);
    }

    getBySlug(slug: string): Observable<CityDetailsDto> {
        return this.http.get<CityDetailsDto>(`${this.apiUrl}getCity/${slug}`);
    }
}