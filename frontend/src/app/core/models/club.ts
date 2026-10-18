export const CLUB_AGES = ['U7', 'U9', 'U11', 'U13', 'U16', 'U19', 'U21', 'Senior'] as const;

export interface ClubListDto {
  publicId: string;
  name: string;
  slug: string;
  foundingDate: string | null;
  clubAge: number;
  cityName: string | null;
  countryName: string | null;
  stadiumName: string | null;
}

export interface ClubDetailsDto extends ClubListDto {
  cityPublicId: string | null;
  countryPublicId: string | null;
  stadiumPublicId: string | null;
  stadiumSlug: string | null;
}

export interface SaveClubDto {
  name: string;
  foundingDate: string | null;
  clubAge: number;
  cityPublicId: string | null;
  stadiumPublicId: string | null;
}

export interface ClubListFilter {
  name?: string;
  countryPublicId?: string;
  cityPublicId?: string;
  foundedFrom?: string;
  foundedTo?: string;
  hasStadium?: boolean;
  clubAge?: number;
  sortBy?: 'name' | 'foundingDate' | 'clubAge';
  descending?: boolean;
}
