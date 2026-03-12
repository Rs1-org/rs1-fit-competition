export interface CountryName{
    name:string;
}
export interface CountryListDto{
    name:string;
    slug:string;
    population:number;
    isCapital:boolean;
}
export interface CreateCountryDto{
    name:string;
    picture:File;
}