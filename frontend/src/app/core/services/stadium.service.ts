import { inject, Injectable } from "@angular/core";
import { environment } from '../../../environments/environment';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { StadiumDetailsDto, StadiumListDto } from "../models/stadium";

@Injectable({
    providedIn: 'root'
})
export class StadiumService {
    private apiUrl = `${environment.apiUrl}/api/Stadium`;
    private http = inject(HttpClient);

    getAll(): Observable<StadiumListDto[]> {
        return this.http.get<StadiumListDto[]>(`${this.apiUrl}/getStadiums`);
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
