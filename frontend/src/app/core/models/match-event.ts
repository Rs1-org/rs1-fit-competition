export interface EventLookup { publicId: string; code: string; name: string; sortOrder: number; }
export interface EventLookups { eventTypes: EventLookup[]; matchParts: EventLookup[]; }
export interface EventParticipant { publicId: string; name: string; }
export interface EventSelection extends EventParticipant { players: EventParticipant[]; }
export interface EventContext { matchPublicId: string; homeName: string; awayName: string; selections: EventSelection[]; }
export interface MatchEvent {
  publicId: string; matchPublicId: string; matchName: string;
  eventType: EventLookup; matchPart: EventLookup; selection: EventParticipant;
  player: EventParticipant | null; relatedPlayer: EventParticipant | null;
  minute: number; addedMinute: number; note: string | null;
}
export interface EventInput {
  matchPublicId: string; eventTypePublicId: string; matchPartPublicId: string; selectionPublicId: string;
  playerPublicId: string | null; relatedPlayerPublicId: string | null; minute: number; addedMinute: number; note: string | null;
}
export interface EventFilter {
  matchPublicId?: string; eventTypePublicId?: string; selectionPublicId?: string; playerPublicId?: string;
  minuteFrom?: number | null; minuteTo?: number | null;
}
