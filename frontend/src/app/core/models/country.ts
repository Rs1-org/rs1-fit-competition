import { SortParams } from './import-export';

export interface CountryName{
    name:string;
}
export interface CountryListDto{
    publicId: string;
    name:string;
    slug:string;
    pictureUrl:string;
    createdAt: string;
}
export interface CreateCountryDto{
    name:string;
    picture:File;
}
export interface CountryListFilter extends SortParams {
    name?:string;
    slug?:string;
    hasPicture?:boolean;
    createdFrom?:string;
    createdTo?:string;
}
