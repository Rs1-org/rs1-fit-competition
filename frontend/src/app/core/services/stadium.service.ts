import { inject, Injectable } from "@angular/core";
<<<<<<< HEAD
import { environment } from "src/enviroments/enviroment";
import { HttpClient, HttpParams } from "@angular/common/http";
=======
import { environment } from '../../../environments/environment';
import { HttpClient } from "@angular/common/http";
>>>>>>> main
import { Observable } from "rxjs";
import { StadiumDetailsDto, StadiumListDto } from "../models/stadium";
import { PagedResult } from "../models/paged-result";

@Injectable({
    providedIn: 'root'
})
export class StadiumService {
    private apiUrl = `${environment.apiUrl}/api/Stadium`;
    private http = inject(HttpClient);

    getAll(page = 1, pageSize = 10): Observable<PagedResult<StadiumListDto>> {
        const params = new HttpParams()
            .set('page', page)
            .set('pageSize', pageSize);
        return this.http.get<PagedResult<StadiumListDto>>(`${this.apiUrl}/getStadiums`, { params });
    }

    getBySlug(slug: string): Observable<StadiumDetailsDto> {
        return this.http.get<StadiumDetailsDto>(`${this.apiUrl}/getStadium/${slug}`);
    }

    create(formData: FormData): Observable<StadiumDetailsDto> {
        return this.http.post<StadiumDetailsDto>(`${this.apiUrl}/createStadium`, formData);
    }

    update(slug: string, formData: FormData): Observable<StadiumDetailsDto> {
        return this.http.put<StadiumDetailsDto>(`${this.apiUrl}/updateStadium/${slug}`, formData);
    }

    delete(id: string): Observable<any> {
        return this.http.delete(`${this.apiUrl}/deleteStadium/${id}`);
    }
}
