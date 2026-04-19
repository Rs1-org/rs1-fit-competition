import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { HttpClient } from "@angular/common/http";
import { CountryListDto, CountryName } from "../models/country";
import { environment } from "../../../environments/environment";
@Injectable({
    providedIn:'root'
})
export class CountryService{
    private http=inject(HttpClient);
    private apiUrl=`${environment.apiUrl}/api/country`;

    create(form:FormData):Observable<CountryName>{
        return this.http.post<CountryName>(`${this.apiUrl}/create`, form);
    }
    
    getAll():Observable<CountryListDto[]>{
        return this.http.get<CountryListDto[]>(`${this.apiUrl}/getAll`);
    }
}