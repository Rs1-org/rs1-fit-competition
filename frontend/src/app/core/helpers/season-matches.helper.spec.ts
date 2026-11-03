import { formByTeam, roundWindow, summariseRounds } from './season-matches.helper';
import { MatchListDto } from '../models/match';
import { RoundListDto } from '../models/round';

function match(partial: Partial<MatchListDto>): MatchListDto {
  return {
    publicId: Math.random().toString(36), slug: 's', date: '2026-10-01T18:00:00Z', homeScore: null, awayScore: null, isPostponed: false,
    statusName: 'Scheduled', stadiumName: null, homeSelectionName: 'A', awaySelectionName: 'B', roundPublicId: 'r1', roundName: null,
    seasonName: '2026/27', competitionName: null, ...partial,
  };
}

function round(number: number): RoundListDto {
  return { publicId: `r${number}`, name: `Round ${number}`, slug: `round-${number}`, number, matchCount: 2 };
}

describe('formByTeam', () => {
  it('keeps the last five finished results per team, oldest first', () => {
    const matches = [
      match({ date: '2026-10-03T18:00:00Z', statusName: 'Finished', homeScore: 0, awayScore: 0 }),
      match({ date: '2026-10-01T18:00:00Z', statusName: 'Finished', homeScore: 2, awayScore: 1 }),
      match({ date: '2026-10-05T18:00:00Z', statusName: 'Live', homeScore: 1, awayScore: 0 }),
      ...[7, 8, 9, 10].map((day) => match({ date: `2026-10-${day}T18:00:00Z`, statusName: 'Finished', homeScore: 0, awayScore: 3 })),
    ];

    const form = formByTeam(matches);

    expect(form['A']).toEqual(['D', 'L', 'L', 'L', 'L']);
    expect(form['B']).toEqual(['D', 'W', 'W', 'W', 'W']);
  });
});

describe('summariseRounds', () => {
  it('labels done, live and the next round, and counts goals', () => {
    const matches = [
      match({ roundPublicId: 'r1', statusName: 'Finished', homeScore: 2, awayScore: 1 }),
      match({ roundPublicId: 'r1', statusName: 'Finished', homeScore: 0, awayScore: 0 }),
      match({ roundPublicId: 'r2', statusName: 'Live', homeScore: 1, awayScore: 0 }),
      match({ roundPublicId: 'r2', statusName: 'Scheduled' }),
      match({ roundPublicId: 'r2', statusName: 'Scheduled', isPostponed: true }),
      match({ roundPublicId: 'r3', statusName: 'Scheduled', date: '2026-11-07T15:00:00Z' }),
      match({ roundPublicId: 'r4', statusName: 'Scheduled' }),
    ];

    const [first, second, third, fourth] = summariseRounds([round(3), round(1), round(4), round(2)], matches);

    expect([first.state, second.state, third.state, fourth.state]).toEqual(['done', 'live', 'next', 'upcoming']);
    expect(first.goals).toBe(3);
    expect(second).toEqual(expect.objectContaining({ live: 1, toPlay: 1, postponed: 1 }));
    expect(third.firstDate).toBe('2026-11-07T15:00:00Z');
  });

  it('does not call an empty round done', () => {
    expect(summariseRounds([round(1)], [])[0].state).toBe('next');
  });
});

describe('roundWindow', () => {
  const rounds = Array.from({ length: 20 }, (_, index) => round(index + 1));

  it('centres on the live round', () => {
    const matches = rounds.map((r, index) => match({ roundPublicId: r.publicId, statusName: index < 11 ? 'Finished' : index === 11 ? 'Live' : 'Scheduled', homeScore: 1, awayScore: 1 }));

    expect(roundWindow(summariseRounds(rounds, matches)).map((s) => s.round.number)).toEqual([9, 10, 11, 12, 13, 14, 15]);
  });

  it('shows the last rounds of a finished season', () => {
    const matches = rounds.map((r) => match({ roundPublicId: r.publicId, statusName: 'Finished', homeScore: 1, awayScore: 0 }));

    expect(roundWindow(summariseRounds(rounds, matches)).at(-1)?.round.number).toBe(20);
  });
});
