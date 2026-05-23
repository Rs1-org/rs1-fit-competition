import { CityListDto } from './city';

export type ClubAge = 'U7' | 'U9' | 'U11' | 'U13' | 'U16' | 'U19' | 'U21' | 'Senior';

export interface StadiumOption {
  id: string;
  name: string;
  slug?: string;
}

export interface ClubResponseDto {
  id: string;
  name: string;
  slug?: string;
  clubAge: ClubAge;
  foundingDate?: string;
  cityId?: string;
  cityName?: string;
  stadiumId?: string;
  stadiumName?: string;
}

export interface CreateClubDto {
  name: string;
  foundingDate: string;
  cityId: string;
  clubAge: ClubAge;
  stadiumId: string;
}

export type UpdateClubDto = CreateClubDto;

export interface ClubFormLookups {
  cities: CityListDto[];
  stadiums: StadiumOption[];
}
