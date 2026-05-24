export interface CountryName{
    name:string;
}
export interface CountryListDto{
    id:string;
    name:string;
    slug:string;
    pictureUrl:string;
}
export interface CreateCountryDto{
    name:string;
    picture:File;
}