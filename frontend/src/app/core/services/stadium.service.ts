import { inject, Injectable } from "@angular/core";
import { environment } from "src/enviroments/enviroment";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { StadiumDetailsDto, StadiumListDto, CreateStadiumDto, UpdateStadiumDto } from "../models/stadium";

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

    create(stadium: CreateStadiumDto): Observable<StadiumDetailsDto> {
        return this.http.post<StadiumDetailsDto>(`${this.apiUrl}/createStadium`, stadium);
    }

    update(slug: string, stadium: UpdateStadiumDto): Observable<StadiumDetailsDto> {
        return this.http.put<StadiumDetailsDto>(`${this.apiUrl}/updateStadium/${slug}`, stadium);
    }

    delete(id: string): Observable<any> {
        return this.http.delete(`${this.apiUrl}/deleteStadium/${id}`);
    }
}
