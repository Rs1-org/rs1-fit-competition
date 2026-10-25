import { FormControl, FormGroup } from '@angular/forms';
import { futureDateValidator, scoresValidator, teamsDifferValidator, toLocalInputValue } from './match-form.validators';

const scores = (homeScore: number | null, awayScore: number | null, halfHomeScore: number | null, halfAwayScore: number | null) =>
  new FormGroup({
    homeScore: new FormControl(homeScore),
    awayScore: new FormControl(awayScore),
    halfHomeScore: new FormControl(halfHomeScore),
    halfAwayScore: new FormControl(halfAwayScore),
  });

describe('match form validators (mirror the backend rules)', () => {
  it('rejects the same home and away team, allows empty teams', () => {
    const group = (home: string, away: string) =>
      new FormGroup({ homeSelectionPublicId: new FormControl(home), awaySelectionPublicId: new FormControl(away) });
    expect(teamsDifferValidator(group('a', 'a'))).toEqual({ sameTeams: true });
    expect(teamsDifferValidator(group('a', 'b'))).toBeNull();
    expect(teamsDifferValidator(group('', ''))).toBeNull();
  });

  it('requires score pairs and keeps half-time within full time', () => {
    expect(scoresValidator(scores(2, 1, 1, 0))).toBeNull();
    expect(scoresValidator(scores(null, null, null, null))).toBeNull();
    expect(scoresValidator(scores(2, null, null, null))).toEqual({ scorePair: true });
    expect(scoresValidator(scores(2, 1, 1, null))).toEqual({ halfScorePair: true });
    expect(scoresValidator(scores(1, 1, 2, 0))).toEqual({ halfAboveFull: true });
    // 0 is a real score, not "missing".
    expect(scoresValidator(scores(0, 0, 0, 0))).toBeNull();
  });

  it('only accepts a future kick-off for a new match', () => {
    expect(futureDateValidator(new FormControl('2000-01-01T12:00'))).toEqual({ pastDate: true });
    expect(futureDateValidator(new FormControl('2999-01-01T12:00'))).toBeNull();
    expect(futureDateValidator(new FormControl(''))).toBeNull();
  });

  it('converts an API instant to a datetime-local value and back', () => {
    const local = toLocalInputValue('2026-10-30T16:05:00Z');
    expect(local).toMatch(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/);
    expect(new Date(local).toISOString()).toBe('2026-10-30T16:05:00.000Z');
    expect(toLocalInputValue(null)).toBe('');
  });
});
