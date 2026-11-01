export interface FormationMark {
  publicId: string;
  defenders: number;
  midfielders: number;
  attackers: number;
  slug: string;
}

export interface Formation {
  publicId: string;
  code: string;
  slug: string;
  formationMarkPublicId: string;
  defenders: number;
  midfielders: number;
  attackers: number;
}

export interface Position {
  publicId: string;
  name: string;
  slug: string;
}

export interface SubPosition {
  publicId: string;
  name: string;
  slug: string;
  positionPublicId: string;
  positionName?: string | null;
}
