export interface TeamStatisticValues {
  totalShots: number; shotsOnTarget: number; totalRedCards: number; totalYellowCards: number;
  corners: number; bigChances: number; possession: number;
}
export interface TeamStatistic extends TeamStatisticValues { publicId: string; selectionPublicId: string; selectionName: string; }
export interface Statistic {
  publicId: string; matchPublicId: string; matchSlug: string | null; matchDate: string;
  home: TeamStatistic | null; away: TeamStatistic | null;
}
export interface StatisticInput { home: TeamStatisticValues; away: TeamStatisticValues; }
export interface StatisticFilter {
  matchPublicId?: string; selectionPublicId?: string; dateFrom?: string; dateTo?: string; minTotalShots?: number | null;
}
export const statisticMetrics = [
  { key: 'totalShots', label: 'Shots' }, { key: 'shotsOnTarget', label: 'Shots on target' },
  { key: 'corners', label: 'Corners' }, { key: 'possession', label: 'Possession (%)' },
  { key: 'totalYellowCards', label: 'Yellow cards' }, { key: 'totalRedCards', label: 'Red cards' },
  { key: 'bigChances', label: 'Big chances' },
] as const;
