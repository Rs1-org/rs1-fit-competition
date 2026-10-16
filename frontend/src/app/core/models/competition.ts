export interface CompetitionListDto {
  publicId: string;
  name: string;
  slug: string;
  shortName: string | null;
  countryName: string | null;
  seasonCount: number;
}

export interface CompetitionDetailsDto {
  publicId: string;
  name: string;
  slug: string;
  shortName: string | null;
  description: string | null;
  countryPublicId: string | null;
  countryName: string | null;
}

export interface SaveCompetitionDto {
  name: string;
  shortName: string | null;
  description: string | null;
  countryPublicId: string | null;
}
