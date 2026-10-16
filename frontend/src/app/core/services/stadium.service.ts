import { inject, Injectable } from "@angular/core";
import { environment } from '../../../environments/environment';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { trackUpload } from "../helpers/upload-progress.helper";
import { StadiumDetailsDto, StadiumListDto, StadiumListFilter } from "../models/stadium";
import { PagedResult } from "../models/paged-result";
import { toFilterParams, toPagedParams } from "../helpers/paging.helper";
import { ExportFormat, ImportResult } from "../models/import-export";

@Injectable({
    providedIn: 'root'
})
export class StadiumService {
    private apiUrl = `${environment.apiUrl}/api/Stadium`;
    private http = inject(HttpClient);

    getAll(page = 1, pageSize = 10, filter: StadiumListFilter = {}): Observable<PagedResult<StadiumListDto>> {
        const params = toPagedParams(page, pageSize, filter);
        return this.http.get<PagedResult<StadiumListDto>>(`${this.apiUrl}/getStadiums`, { params });
    }

    getBySlug(slug: string): Observable<StadiumDetailsDto> {
        return this.http.get<StadiumDetailsDto>(`${this.apiUrl}/getStadium/${slug}`);
    }

    create(formData: FormData, onProgress?: (percent: number) => void): Observable<StadiumDetailsDto> {
        if (!onProgress) return this.http.post<StadiumDetailsDto>(`${this.apiUrl}/createStadium`, formData);
        return trackUpload(
            this.http.post<StadiumDetailsDto>(`${this.apiUrl}/createStadium`, formData, { reportProgress: true, observe: 'events' }),
            onProgress,
        );
    }

    update(slug: string, formData: FormData, onProgress?: (percent: number) => void): Observable<StadiumDetailsDto> {
        if (!onProgress) return this.http.put<StadiumDetailsDto>(`${this.apiUrl}/updateStadium/${slug}`, formData);
        return trackUpload(
            this.http.put<StadiumDetailsDto>(`${this.apiUrl}/updateStadium/${slug}`, formData, { reportProgress: true, observe: 'events' }),
            onProgress,
        );
    }

    delete(publicId: string): Observable<any> {
        return this.http.delete(`${this.apiUrl}/deleteStadium/${publicId}`);
    }

    /** Current filters + sort, without paging (RS1 item 86). */
    export(filter: StadiumListFilter, format: ExportFormat): Observable<Blob> {
        const params = toFilterParams({ ...filter, format });
        return this.http.get(`${this.apiUrl}/export`, { params, responseType: 'blob' });
    }

    /** All-or-nothing import; the result lists row errors when nothing was saved. */
    import(file: File): Observable<ImportResult> {
        const form = new FormData();
        form.append('file', file);
        return this.http.post<ImportResult>(`${this.apiUrl}/import`, form);
    }
}
