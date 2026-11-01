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
export interface RoundGoals { roundNumber: number; roundName: string; matches: number; goals: number; }
export interface TeamResults { selectionPublicId: string; selectionName: string; played: number; wins: number; draws: number;
  losses: number; goalsFor: number; goalsAgainst: number; points: number; }
export interface TeamShots { selectionPublicId: string; selectionName: string; matches: number; averageShots: number;
  averageShotsOnTarget: number; averagePossession: number; }
export interface StatisticCharts { goalsPerRound: RoundGoals[]; teamResults: TeamResults[]; teamShots: TeamShots[]; }
export interface StatisticChartFilter { competitionPublicId?: string; seasonPublicId?: string; }
export interface TeamFormMatch { date: string; opponentName: string; goalsFor: number; goalsAgainst: number; result: 'W' | 'D' | 'L'; }
export interface TeamForm {
  selectionPublicId: string; selectionName: string; wins: number; draws: number; losses: number;
  goalsFor: number; goalsAgainst: number; points: number; matches: TeamFormMatch[];
}
export interface HeadToHeadMatch { date: string; homeName: string; homeScore: number; awayName: string; awayScore: number; }
export interface HeadToHead {
  firstName: string; secondName: string; firstWins: number; draws: number; secondWins: number;
  firstGoals: number; secondGoals: number; matches: HeadToHeadMatch[];
}
export interface PlayerStats { playerPublicId: string; playerName: string; selectionName: string; goals: number; yellowCards: number; redCards: number; }
