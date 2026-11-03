import { MatchListDto } from '../models/match';
import { RoundListDto } from '../models/round';
import { FormResult, matchPhase } from '../../shared/match/match-display';

export type RoundState = 'done' | 'live' | 'next' | 'upcoming';

export interface RoundSummary {
  round: RoundListDto;
  state: RoundState;
  matches: number;
  goals: number;
  live: number;
  toPlay: number;
  postponed: number;
  firstDate: string | null;
  lastDate: string | null;
}

function isFinished(match: MatchListDto): boolean {
  return matchPhase(match.statusName, match.isPostponed) === 'finished' && match.homeScore !== null && match.awayScore !== null;
}

function isLive(match: MatchListDto): boolean {
  const phase = matchPhase(match.statusName, match.isPostponed);
  return phase === 'live' || phase === 'half-time';
}

/**
 * Last results per team name, oldest first (as the form chips read them).
 * Keyed by name because the match list does not carry team publicIds.
 */
export function formByTeam(matches: MatchListDto[], last = 5): Record<string, FormResult[]> {
  const byTeam: Record<string, FormResult[]> = {};
  const played = matches.filter(isFinished).sort((a, b) => (a.date ?? '').localeCompare(b.date ?? ''));
  const add = (team: string | null, result: FormResult) => {
    if (team) byTeam[team] = [...(byTeam[team] ?? []), result].slice(-last);
  };
  for (const match of played) {
    const home = match.homeScore!;
    const away = match.awayScore!;
    add(match.homeSelectionName, home > away ? 'W' : home < away ? 'L' : 'D');
    add(match.awaySelectionName, away > home ? 'W' : away < home ? 'L' : 'D');
  }
  return byTeam;
}

/**
 * One summary per round, in round-number order. A round is done when it has matches and none is left to play
 * (cancelled ones do not count), live while any of its matches is on, and the first other round is next.
 */
export function summariseRounds(rounds: RoundListDto[], matches: MatchListDto[]): RoundSummary[] {
  const summaries = [...rounds].sort((a, b) => a.number - b.number).map((round): RoundSummary => {
    const own = matches.filter((match) => match.roundPublicId === round.publicId);
    const dates = own.map((match) => match.date).filter((date): date is string => !!date).sort();
    const live = own.filter(isLive).length;
    const postponed = own.filter((match) => match.isPostponed).length;
    const cancelled = own.filter((match) => matchPhase(match.statusName, match.isPostponed) === 'cancelled').length;
    const finished = own.filter(isFinished).length;
    const toPlay = own.length - finished - live - postponed - cancelled;
    const done = own.length > 0 && live === 0 && toPlay === 0 && postponed === 0;
    return {
      round,
      state: live ? 'live' : done ? 'done' : 'upcoming',
      matches: own.length,
      goals: own.filter(isFinished).reduce((sum, match) => sum + match.homeScore! + match.awayScore!, 0),
      live,
      toPlay,
      postponed,
      firstDate: dates[0] ?? null,
      lastDate: dates.at(-1) ?? null,
    };
  });
  const next = summaries.find((summary) => summary.state === 'upcoming');
  if (next) next.state = 'next';
  return summaries;
}

/** A few rounds around the current one, so a long season fits in the side card. */
export function roundWindow(summaries: RoundSummary[], size = 7): RoundSummary[] {
  if (summaries.length <= size) return summaries;
  const live = summaries.findIndex((s) => s.state === 'live');
  const next = summaries.findIndex((s) => s.state === 'next');
  // Finished seasons show their last rounds.
  const anchor = live >= 0 ? live : next >= 0 ? next : summaries.length - 1;
  const start = Math.min(Math.max(0, anchor - 3), summaries.length - size);
  return summaries.slice(start, start + size);
}
