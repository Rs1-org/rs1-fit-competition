export interface CityName{
    name:string;
}

export interface CityListDto{
    id:string;
    name:string;
    population:number;
    isCapital:boolean;
    countryName:string;
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
