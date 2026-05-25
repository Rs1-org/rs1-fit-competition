import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { HttpClient, HttpParams } from "@angular/common/http";
import { environment } from '../../../environments/environment';
import { CountryListDto, CountryName } from "../models/country";
import { PagedResult } from "../models/paged-result";

@Injectable({
    providedIn:'root'
})
export class CountryService{
    private http=inject(HttpClient);
    private apiUrl=`${environment.apiUrl}/api/country`;

    create(form:FormData):Observable<CountryName>{
        return this.http.post<CountryName>(`${this.apiUrl}/create`, form);
    }

    update(slug:string, form:FormData):Observable<CountryName>{
        return this.http.put<CountryName>(`${this.apiUrl}/update/${slug}`, form);
    }

    delete(id:string):Observable<CountryName>{
        return this.http.delete<CountryName>(`${this.apiUrl}/delete/${id}`);
    }

    getAll(page = 1, pageSize = 10):Observable<PagedResult<CountryListDto>>{
        const params = new HttpParams()
            .set('page', page)
            .set('pageSize', pageSize);
        return this.http.get<PagedResult<CountryListDto>>(`${this.apiUrl}/getAll`, { params });
    }
}