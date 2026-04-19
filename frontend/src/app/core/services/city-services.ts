import { inject, Injectable } from "@angular/core";
import { environment } from "../../../environments/environment";
import { Observable } from "rxjs";
import { CityName } from "../models/city";
import { HttpClient } from "@angular/common/http";

@Injectable({
    providedIn:'root'
})

export class CityService{
    private apiUrl=`${environment.apiUrl}/api/City/`;

    private http=inject(HttpClient); 

    create(city:any):Observable<CityName>{
        return this.http.post<CityName>(`${this.apiUrl}createCity`,city);
    }
}