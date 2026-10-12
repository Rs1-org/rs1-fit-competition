export const CLUB_AGES = ['U7', 'U9', 'U11', 'U13', 'U16', 'U19', 'U21', 'Senior'];

export interface ClubInput {
  name: string;
  foundingDate: string;
  clubAge: number;
  cityId: string;
  stadiumId: string;
}

export interface Club extends ClubInput {
  id: string;
  slug: string;
  cityName: string;
  stadiumName: string;
}

export interface ClubFilter {
  name?: string;
  cityId?: string;
  stadiumId?: string;
  foundingYear?: number;
  clubAge?: number;
}
