export type MatchPhase = 'scheduled' | 'live' | 'half-time' | 'finished' | 'postponed' | 'cancelled';
export type FormResult = 'W' | 'D' | 'L';

const CREST_COLOURS = ['#0f5f52', '#1d4e89', '#8e1833', '#8f5f00', '#4c3d78', '#0e7490', '#9a3412', '#166534'];

export function teamCode(name: string | null | undefined, abbreviation?: string | null): string {
  const explicit = abbreviation?.replace(/[^A-Za-z]/g, '').slice(0, 3).toUpperCase();
  if (explicit) return explicit;
  const words = (name ?? '')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^A-Za-z ]/g, '')
    .trim()
    .split(/\s+/)
    .filter((word) => word && !['FK', 'NK', 'HSK', 'RK'].includes(word.toUpperCase()));
  const source = (words.at(-1) ?? 'TBD').toUpperCase();
  return source.slice(0, 3);
}

/** Crest code for a competition: its short name, else the initials ("Premijer liga BiH" → "PLB"). */
export function competitionCode(name: string | null | undefined, shortName?: string | null): string {
  const explicit = shortName?.replace(/[^A-Za-z0-9]/g, '').slice(0, 3).toUpperCase();
  if (explicit) return explicit;
  const initials = (name ?? '')
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .split(/\s+/)
    .map((word) => word.replace(/[^A-Za-z0-9]/g, '').charAt(0))
    .join('');
  return (initials || 'CMP').slice(0, 3).toUpperCase();
}

export function crestColour(code: string): string {
  let hash = 0;
  for (const char of code) hash = (hash * 31 + char.charCodeAt(0)) >>> 0;
  return CREST_COLOURS[hash % CREST_COLOURS.length];
}

export function matchPhase(statusName: string | null | undefined, isPostponed = false): MatchPhase {
  if (isPostponed) return 'postponed';
  const value = (statusName ?? '').toLowerCase().replace(/[\s-]+/g, '');
  if (value.includes('cancel')) return 'cancelled';
  if (value.includes('postpone')) return 'postponed';
  if (value.includes('half')) return 'half-time';
  if (value.includes('live')) return 'live';
  if (value.includes('finish') || value === 'ft') return 'finished';
  return 'scheduled';
}

export function statusShort(phase: MatchPhase, minute?: string | null): string {
  if (phase === 'live') return minute ? minuteLabel(minute) : 'LIVE';
  if (phase === 'half-time') return 'HT';
  if (phase === 'finished') return 'FT';
  if (phase === 'postponed') return 'PPD';
  if (phase === 'cancelled') return 'CANC.';
  return 'SCHED.';
}

/** Short line under the score for a match that is not waiting for kick-off. */
export function statusNote(phase: MatchPhase): string {
  if (phase === 'finished') return 'Full time';
  if (phase === 'postponed') return 'Postponed, new date to be confirmed';
  if (phase === 'cancelled') return 'Cancelled';
  return '';
}

export function kickoffCountdown(date: string | null | undefined, now: number): string {
  if (!date) return '';
  const minutes = Math.ceil((new Date(date).getTime() - now) / 60_000);
  if (minutes <= 0) return 'Kick-off soon';
  const days = Math.floor(minutes / 1440);
  const hours = Math.floor((minutes % 1440) / 60);
  if (days) return `Kick-off in ${days} d ${hours} h`;
  return hours ? `Kick-off in ${hours} h ${minutes % 60} min` : `Kick-off in ${minutes} min`;
}

type GoalEvent = {
  eventType: { code: string };
  selection: { publicId: string };
  player: { name: string } | null;
  minute: number;
  addedMinute: number;
};

/** One line per scorer of a team. An own goal counts for the opponent of the player's team. */
export function goalscorers(
  events: GoalEvent[],
  teamId: string | null | undefined,
  opponentId: string | null | undefined,
): { name: string; minutes: string[] }[] {
  const lines = new Map<string, string[]>();
  const inOrder = [...events].sort((a, b) => a.minute - b.minute || a.addedMinute - b.addedMinute);
  for (const event of inOrder) {
    const { code } = event.eventType;
    const scored = (code === 'goal' || code === 'penalty') && event.selection.publicId === teamId;
    const ownGoal = code === 'own-goal' && event.selection.publicId === opponentId;
    if (!scored && !ownGoal) continue;
    const name = event.player?.name ?? 'Unknown';
    const suffix = code === 'penalty' ? ' (P)' : ownGoal ? ' (OG)' : '';
    lines.set(name, [...(lines.get(name) ?? []), minuteLabel(event.minute, event.addedMinute) + suffix]);
  }
  return [...lines].map(([name, minutes]) => ({ name, minutes }));
}

/** The furthest minute recorded so far, shown next to the live badge. */
export function latestMinute(events: { minute: number; addedMinute: number }[]): string | null {
  if (!events.length) return null;
  const last = events.reduce((a, b) => (b.minute > a.minute || (b.minute === a.minute && b.addedMinute > a.addedMinute) ? b : a));
  return String(last.minute) + (last.addedMinute ? `+${last.addedMinute}` : '');
}

export function statusLabel(phase: MatchPhase): string {
  if (phase === 'half-time') return 'Half-time';
  if (phase === 'finished') return 'Full time';
  return phase.charAt(0).toUpperCase() + phase.slice(1);
}

export function minuteLabel(minute: string | number | null | undefined, added?: number | null): string {
  if (minute === null || minute === undefined || minute === '') return '';
  const text = String(minute).replace(/'$/, '');
  const extra = added ? `+${added}` : '';
  return `${text}${extra}'`;
}

export function scoreText(home: number | null | undefined, away: number | null | undefined): string {
  if (home === null || home === undefined || away === null || away === undefined) return 'vs';
  return `${home} – ${away}`;
}
