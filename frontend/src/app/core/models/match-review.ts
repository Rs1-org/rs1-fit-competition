import { PagedResult } from './paged-result';

export interface Review {
  publicId: string; rating: number; comment: string; authorName: string; createdAt: string;
  isMine: boolean; likeCount: number; dislikeCount: number; myReaction: number | null;
}
export interface MatchReviewPage {
  averageRating: number | null; reviewCount: number; reviews: PagedResult<Review>; myReview: Review | null;
}
export interface ReviewInput { rating: number; comment: string; }
export interface ReviewMatch {
  publicId: string; date: string | null; homeSelectionName: string | null; awaySelectionName: string | null;
  homeScore: number | null; awayScore: number | null; statusName: string | null; stadiumName: string | null;
}
