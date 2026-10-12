export interface CompetitionListDto {
  id: string;
  name: string;
  slug: string;
  shortName: string | null;
  countryName: string | null;
  seasonCount: number;
}

export interface CompetitionDetailsDto {
  id: string;
  name: string;
  slug: string;
  shortName: string | null;
  description: string | null;
  countryId: string | null;
  countryName: string | null;
}

export interface SaveCompetitionDto {
  name: string;
  shortName: string | null;
  description: string | null;
  countryId: string | null;
}
