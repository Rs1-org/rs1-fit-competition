export interface RoundListDto {
  id: string;
  name: string;
  slug: string;
  number: number;
  matchCount: number;
}

export interface RoundDetailsDto {
  id: string;
  name: string;
  slug: string;
  number: number;
  seasonId: string;
  seasonName: string;
  seasonSlug: string;
  competitionName: string;
  competitionSlug: string;
}

export interface SaveRoundDto {
  seasonId?: string;
  name: string;
  number: number;
}

/** Subset of the match list item shown on the round page (full match UI lives in COMP-65). */
export interface RoundMatchDto {
  id: string;
  slug: string | null;
  date: string | null;
  homeScore: number | null;
  awayScore: number | null;
  isPostponed: boolean;
  statusName: string | null;
  stadiumName: string | null;
  homeSelectionName: string | null;
  awaySelectionName: string | null;
}
