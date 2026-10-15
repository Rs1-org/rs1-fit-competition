export interface CountryName{
    name:string;
}
export interface CountryListDto{
    id:string;
    publicId:string;
    name:string;
    slug:string;
    pictureUrl:string;
}
export interface CreateCountryDto{
    name:string;
    picture:File;
}
export interface CountryListFilter{
    name?:string;
    slug?:string;
    hasPicture?:boolean;
    createdFrom?:string;
    createdTo?:string;
}
