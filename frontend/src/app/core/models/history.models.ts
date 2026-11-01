export interface PlayerSelectionHistory {
  publicId?: string;
  goals: number | null;
  matchPlayed: number | null;
  assists: number | null;
  shirtNumber: number | null;
  playerPublicId?: string;
  selectionPublicId?: string | null;
}

export interface TransferValueHistory {
  publicId?: string;
  date: string | null;
  value: number | null;
  playerPublicId?: string;
}

export interface SelectionManagerHistory {
  publicId?: string;
  notes: string | null;
  wins: number | null;
  loses: number | null;
  draw: number | null;
  managerPublicId?: string;
  selectionPublicId?: string | null;
}

export interface MandatDate {
  publicId?: string;
  startMandatDate: string | null;
  endMandatDate: string | null;
  playerSelectionHistoryPublicId?: string | null;
  selectionManagerHistoryPublicId?: string | null;
}

export interface PlayerAppearance {
  matchPublicId: string;
  matchSlug: string | null;
  matchDate: string;
  stadiumName: string | null;
  selectionPublicId: string;
  selectionName: string;
  side: string;
  shirtNumber: number | null;
}

export interface PlayerHistoryForm {
  selection: PlayerSelectionHistory;
  transfer: TransferValueHistory;
  mandat: MandatDate;
  selectionId?: string;
  teamSelectionId: string;
  transferId?: string;
  mandatId?: string;
}

export interface ManagerHistoryForm {
  selection: SelectionManagerHistory;
  mandat: MandatDate;
  selectionId?: string;
  teamSelectionId: string;
  mandatId?: string;
}

export function emptyPlayerHistoryForm(): PlayerHistoryForm {
  return {
    selection: {
      goals: null,
      matchPlayed: null,
      assists: null,
      shirtNumber: null,
    },
    transfer: {
      date: null,
      value: null,
    },
    mandat: {
      startMandatDate: null,
      endMandatDate: null,
    },
    teamSelectionId: '',
  };
}

export function emptyManagerHistoryForm(): ManagerHistoryForm {
  return {
    selection: {
      notes: null,
      wins: null,
      loses: null,
      draw: null,
    },
    mandat: {
      startMandatDate: null,
      endMandatDate: null,
    },
    teamSelectionId: '',
  };
}

export function hasPlayerHistoryData(form: PlayerHistoryForm): boolean {
  const { selection, transfer, mandat } = form;
  return (
    selection.goals != null ||
    selection.matchPlayed != null ||
    selection.assists != null ||
    selection.shirtNumber != null ||
    !!transfer.date ||
    transfer.value != null ||
    !!mandat.startMandatDate ||
    !!mandat.endMandatDate ||
    !!form.teamSelectionId
  );
}

export function hasManagerHistoryData(form: ManagerHistoryForm): boolean {
  const { selection, mandat } = form;
  return (
    !!selection.notes?.trim() ||
    selection.wins != null ||
    selection.loses != null ||
    selection.draw != null ||
    !!mandat.startMandatDate ||
    !!mandat.endMandatDate ||
    !!form.teamSelectionId
  );
}

export function playerHistoryViewLabels(): Record<string, string> {
  return {
    goals: 'Golovi',
    matchPlayed: 'Odigrane utakmice',
    assists: 'Asistencije',
    shirtNumber: 'Broj dresa',
    transferDate: 'Datum transfera',
    transferValue: 'Vrijednost transfera',
    startMandatDate: 'Početak mandata',
    endMandatDate: 'Kraj mandata',
  };
}

export function managerHistoryViewLabels(): Record<string, string> {
  return {
    notes: 'Bilješke',
    wins: 'Pobjede',
    loses: 'Porazi',
    draw: 'Neriješeno',
    startMandatDate: 'Početak mandata',
    endMandatDate: 'Kraj mandata',
  };
}
