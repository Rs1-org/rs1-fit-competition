export interface CityName{
    name:string;
}

export interface CityListDto{
    id:string;
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
    countryId:string;
}

export interface CreateCityDto{
    name:string;
    population:number;
    isCapital:boolean;
    countryId:string;
    latitude:number;
    longitude:number;
    description:string;
}
