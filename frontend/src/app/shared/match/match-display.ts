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
