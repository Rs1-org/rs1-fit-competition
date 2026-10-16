import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { HttpClient } from "@angular/common/http";
import { environment } from '../../../environments/environment';
import { trackUpload } from "../helpers/upload-progress.helper";
import { fetchAllPages, MAX_PAGE_SIZE, toPagedParams } from "../helpers/paging.helper";
import { CountryListDto, CountryListFilter, CountryName } from "../models/country";
import { PagedResult } from "../models/paged-result";

@Injectable({
    providedIn:'root'
})
export class CountryService{
    private http=inject(HttpClient);
    private apiUrl=`${environment.apiUrl}/api/country`;

    create(form:FormData, onProgress?: (percent: number) => void):Observable<CountryName>{
        if (!onProgress) return this.http.post<CountryName>(`${this.apiUrl}/create`, form);
        return trackUpload(
            this.http.post<CountryName>(`${this.apiUrl}/create`, form, { reportProgress: true, observe: 'events' }),
            onProgress,
        );
    }

    update(slug:string, form:FormData, onProgress?: (percent: number) => void):Observable<CountryName>{
        if (!onProgress) return this.http.put<CountryName>(`${this.apiUrl}/update/${slug}`, form);
        return trackUpload(
            this.http.put<CountryName>(`${this.apiUrl}/update/${slug}`, form, { reportProgress: true, observe: 'events' }),
            onProgress,
        );
    }

    delete(publicId: string):Observable<CountryName>{
        return this.http.delete<CountryName>(`${this.apiUrl}/delete/${publicId}`);
    }

    getAll(page = 1, pageSize = 10, filter: CountryListFilter = {}):Observable<PagedResult<CountryListDto>>{
        const params = toPagedParams(page, pageSize, filter);
        return this.http.get<PagedResult<CountryListDto>>(`${this.apiUrl}/getAll`, { params });
    }

    getAllForDropdown():Observable<CountryListDto[]>{
        return fetchAllPages((page) => this.getAll(page, MAX_PAGE_SIZE));
    }
}