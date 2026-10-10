export interface FormationMark {
  id: string;
  defenders: number;
  midfielders: number;
  attackers: number;
  slug: string;
}

export interface Formation {
  id: string;
  code: string;
  slug: string;
  formationMarkId: string;
  defenders: number;
  midfielders: number;
  attackers: number;
}

export interface Position {
  id: string;
  name: string;
  slug: string;
}

export interface SubPosition {
  id: string;
  name: string;
  slug: string;
  positionId: string;
  positionName?: string | null;
}
