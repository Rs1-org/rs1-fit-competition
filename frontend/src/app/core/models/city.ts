export interface CityName{
    name:string;
}

export interface CityListDto{
    publicId: string;
    name:string;
    slug:string;
    population:number;
    isCapital:boolean;
    countryName:string;
}

export interface CityDetailsDto{
    name:string;
    slug:string;
    population:number;
    isCapital:boolean;
    latitude:number;
    longitude:number;
    description:string;
    countryName:string;
    countryPublicId:string;
}

export interface CreateCityDto{
    name:string;
    population:number;
    isCapital:boolean;
    countryPublicId:string;
    latitude:number;
    longitude:number;
    description:string;
}

export interface CityListFilter{
    name?:string;
    countryPublicId?:string;
    isCapital?:boolean;
    minPopulation?:number;
    maxPopulation?:number;
}
