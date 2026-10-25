export interface MatchListDto {
  publicId: string;
  slug: string;
  date: string | null;
  homeScore: number | null;
  awayScore: number | null;
  isPostponed: boolean;
  statusName: string | null;
  stadiumName: string | null;
  homeSelectionName: string | null;
  awaySelectionName: string | null;
  roundPublicId: string | null;
  roundName: string | null;
}

export interface MatchDetailsDto {
  publicId: string;
  slug: string;
  date: string | null;
  followersCount: number;
  homeScore: number | null;
  awayScore: number | null;
  halfHomeScore: number | null;
  halfAwayScore: number | null;
  isPostponed: boolean;
  statusPublicId: string;
  statusName: string | null;
  stadiumPublicId: string;
  stadiumName: string | null;
  stadiumSlug: string | null;
  homeSelectionPublicId: string | null;
  homeSelectionName: string | null;
  homeSelectionSlug: string | null;
  awaySelectionPublicId: string | null;
  awaySelectionName: string | null;
  awaySelectionSlug: string | null;
  roundPublicId: string | null;
  roundName: string | null;
  roundSlug: string | null;
  seasonPublicId: string | null;
  competitionPublicId: string | null;
}

/** Mirrors the backend GetAllMatchesQuery filters. */
export interface MatchListFilter {
  statusPublicId?: string;
  stadiumPublicId?: string;
  roundPublicId?: string;
  isPostponed?: boolean;
  dateFrom?: string;
  dateTo?: string;
}

export interface SaveMatchDto {
  date: string;
  followersCount: number;
  isPostponed: boolean;
  statusPublicId: string;
  stadiumPublicId: string;
  homeSelectionPublicId: string | null;
  awaySelectionPublicId: string | null;
  roundPublicId: string | null;
  homeScore?: number | null;
  awayScore?: number | null;
  halfHomeScore?: number | null;
  halfAwayScore?: number | null;
}
