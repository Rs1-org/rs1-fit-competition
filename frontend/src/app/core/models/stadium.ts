export interface StadiumListDto {
    id: string;
    name: string;
    capacity: number;
    slug: string;
    pictureUrl: string;
    cityName: string;
    countryName: string;
}

export interface StadiumDetailsDto {
    id: string;
    name: string;
    capacity: number;
    dateOfConstruction: string;
    slug: string;
    pictureUrl: string;
    cityId: string;
    cityName: string;
    latitude: number;
    longitude: number;
    geoDescription: string;
}

export interface CreateStadiumDto {
    name: string;
    capacity: number;
    dateOfConstruction: string;
    pictureUrl: string;
    cityId: string;
    latitude: number;
    longitude: number;
    geoDescription: string;
}

export interface UpdateStadiumDto {
    name: string;
    capacity: number;
    dateOfConstruction: string;
    pictureUrl: string;
    cityId: string;
    latitude: number;
    longitude: number;
    geoDescription: string;
}
