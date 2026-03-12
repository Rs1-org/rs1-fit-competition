import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { CountryName } from "../models/country";
import { HttpClient } from "@angular/common/http";
import { CreateCountryDto } from "../../models/Country";

@Injectable({
    providedIn:'root'
})
export class CountryService{
    private apiUrl="http://localhost:5244/api/country/";
    private http=inject(HttpClient);

    create(form:FormData):Observable<CountryName>{
        return this.http.post<CountryName>(this.apiUrl+`create`, form);
    }
}