import { SortParams } from './import-export';

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
  homeSelectionAbbreviation?: string | null;
  awaySelectionName: string | null;
  awaySelectionAbbreviation?: string | null;
  roundPublicId: string | null;
  roundName: string | null;
  seasonName: string | null;
  competitionName: string | null;
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
  homeSelectionAbbreviation?: string | null;
  homeSelectionSlug: string | null;
  awaySelectionPublicId: string | null;
  awaySelectionName: string | null;
  awaySelectionAbbreviation?: string | null;
  awaySelectionSlug: string | null;
  roundPublicId: string | null;
  roundName: string | null;
  roundSlug: string | null;
  seasonPublicId: string | null;
  seasonName: string | null;
  competitionPublicId: string | null;
  competitionName: string | null;
}

/** Quick tabs of the match list; a phase can cover several statuses. */
export type MatchPhase = 'live' | 'results' | 'upcoming';

/** Mirrors the backend GetAllMatchesQuery filters. */
export interface MatchListFilter extends SortParams {
  phase?: MatchPhase;
  statusPublicId?: string;
  stadiumPublicId?: string;
  roundPublicId?: string;
  isPostponed?: boolean;
  dateFrom?: string;
  dateTo?: string;
  selectionPublicId?: string;
  competitionPublicId?: string;
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
