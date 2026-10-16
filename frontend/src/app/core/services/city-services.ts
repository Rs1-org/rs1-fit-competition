import { inject, Injectable } from "@angular/core";
import { environment } from '../../../environments/environment';
import { Observable } from "rxjs";
import { CityDetailsDto, CityListDto, CityListFilter, CityName } from "../models/city";
import { HttpClient } from "@angular/common/http";
import { fetchAllPages, MAX_PAGE_SIZE, toFilterParams, toPagedParams } from "../helpers/paging.helper";
import { ExportFormat, ImportResult } from "../models/import-export";
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

    delete(publicId: string):Observable<CityName>{
        return this.http.delete<CityName>(`${this.apiUrl}deleteCity/${publicId}`);
    }

    getAll(page = 1, pageSize = 10, filter: CityListFilter = {}): Observable<PagedResult<CityListDto>> {
        const params = toPagedParams(page, pageSize, filter);
        return this.http.get<PagedResult<CityListDto>>(`${this.apiUrl}getCities`, { params });
    }

    getAllForDropdown(): Observable<CityListDto[]> {
        return fetchAllPages((page) => this.getAll(page, MAX_PAGE_SIZE));
    }

    getBySlug(slug: string): Observable<CityDetailsDto> {
        return this.http.get<CityDetailsDto>(`${this.apiUrl}getCity/${slug}`);
    }

    /** Current filters + sort, without paging (RS1 item 86). */
    export(filter: CityListFilter, format: ExportFormat): Observable<Blob> {
        const params = toFilterParams({ ...filter, format });
        return this.http.get(`${this.apiUrl}export`, { params, responseType: 'blob' });
    }

    /** All-or-nothing import; the result lists row errors when nothing was saved. */
    import(file: File): Observable<ImportResult> {
        const form = new FormData();
        form.append('file', file);
        return this.http.post<ImportResult>(`${this.apiUrl}import`, form);
    }
}