import { crestColour, matchPhase, scoreText, statusShort, teamCode } from './match-display';

describe('match display', () => {
  it('uses the selection abbreviation and otherwise the club name', () => {
    expect(teamCode('FK Sarajevo', 'sar')).toBe('SAR');
    expect(teamCode('FK Željezničar')).toBe('ZEL');
  });

  it('keeps one colour for the same code', () => {
    expect(crestColour('SAR')).toBe(crestColour('SAR'));
  });

  it('maps match statuses to a short label that is not colour alone', () => {
    expect(matchPhase('Live')).toBe('live');
    expect(matchPhase('Finished', true)).toBe('postponed');
    expect(statusShort('live', '67')).toBe("67'");
    expect(statusShort('half-time')).toBe('HT');
    expect(statusShort('finished')).toBe('FT');
    expect(scoreText(2, 1)).toBe('2 – 1');
    expect(scoreText(null, null)).toBe('vs');
  });
});
