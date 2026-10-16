import { SortParams } from './import-export';

export interface StadiumListDto {
    publicId: string;
    name: string;
    capacity: number;
    dateOfConstruction: string;
    slug: string;
    pictureUrl: string;
    cityName: string;
    countryName: string;
}

export interface StadiumDetailsDto {
    publicId: string;
    name: string;
    capacity: number;
    dateOfConstruction: string;
    slug: string;
    pictureUrl: string;
    cityPublicId: string;
    cityName: string;
    latitude: number;
    longitude: number;
    geoDescription: string;
    cityLatitude: number | null;
    cityLongitude: number | null;
}

export interface StadiumListFilter extends SortParams {
    name?: string;
    countryPublicId?: string;
    cityPublicId?: string;
    minCapacity?: number;
    maxCapacity?: number;
}
