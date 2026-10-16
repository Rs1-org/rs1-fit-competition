export interface SeasonListDto {
  publicId: string;
  name: string;
  slug: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
  competitionName: string;
  roundCount: number;
}

export interface SeasonDetailsDto {
  publicId: string;
  name: string;
  slug: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
  competitionPublicId: string;
  competitionName: string;
  competitionSlug: string;
}

export interface SaveSeasonDto {
  competitionPublicId?: string;
  name: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
}
