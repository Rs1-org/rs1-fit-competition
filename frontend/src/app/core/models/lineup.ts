export interface LineupPlayer {
  publicId: string;
  playerPublicId: string;
  playerName: string;
  selectionPublicId: string;
  positionPublicId: string | null;
  positionName: string | null;
  subPositionPublicId: string | null;
  subPositionName: string | null;
  isStarter: boolean;
  shirtNumber: number | null;
}

export interface MatchLineup {
  matchPublicId: string;
  refereePublicId: string | null;
  refereeName: string | null;
  homeFormationPublicId: string | null;
  homeFormationCode: string | null;
  awayFormationPublicId: string | null;
  awayFormationCode: string | null;
  home: LineupPlayer[];
  away: LineupPlayer[];
}

export interface SaveLineupPlayer {
  playerPublicId: string;
  selectionPublicId: string;
  positionPublicId: string | null;
  subPositionPublicId: string | null;
  isStarter: boolean;
  shirtNumber: number | null;
}

export function emptyLineup(matchPublicId = ''): MatchLineup {
  return {
    matchPublicId,
    refereePublicId: null,
    refereeName: null,
    homeFormationPublicId: null,
    homeFormationCode: null,
    awayFormationPublicId: null,
    awayFormationCode: null,
    home: [],
    away: [],
  };
}
