export interface SeasonListDto {
  id: string;
  name: string;
  slug: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
  competitionName: string;
  roundCount: number;
}

export interface SeasonDetailsDto {
  id: string;
  name: string;
  slug: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
  competitionId: string;
  competitionName: string;
  competitionSlug: string;
}

export interface SaveSeasonDto {
  competitionId?: string;
  name: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
}
