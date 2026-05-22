export interface PlayerSelectionHistory {
  id?: string;
  goals: number | null;
  matchPlayed: number | null;
  assists: number | null;
  shirtNumber: number | null;
  playerId?: string;
}

export interface TransferValueHistory {
  id?: string;
  date: string | null;
  value: number | null;
  playerId?: string;
}

export interface SelectionManagerHistory {
  id?: string;
  notes: string | null;
  wins: number | null;
  loses: number | null;
  draw: number | null;
  managerId?: string;
}

export interface MandatDate {
  id?: string;
  startMandatDate: string | null;
  endMandatDate: string | null;
  playerSelectionHistoryId?: string | null;
  selectionManagerHistoryId?: string | null;
}

export interface PlayerHistoryForm {
  selection: PlayerSelectionHistory;
  transfer: TransferValueHistory;
  mandat: MandatDate;
  selectionId?: string;
  transferId?: string;
  mandatId?: string;
}

export interface ManagerHistoryForm {
  selection: SelectionManagerHistory;
  mandat: MandatDate;
  selectionId?: string;
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
    !!mandat.endMandatDate
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
    !!mandat.endMandatDate
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
