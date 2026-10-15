import { AppCurrencyPipe, AppDatePipe, AppDateTimePipe, AppNumberPipe } from './regional-format.pipes';

describe('regional format pipes', () => {
  it('formats a date-only value as dd.MM.yyyy', () => {
    expect(new AppDatePipe().transform('2026-10-11')).toBe('11.10.2026');
  });

  it('formats a local date and time as dd.MM.yyyy HH:mm', () => {
    expect(new AppDateTimePipe().transform(new Date(2026, 9, 11, 18, 0))).toBe('11.10.2026 18:00');
  });

  it('formats numbers with a comma decimal separator', () => {
    expect(new AppNumberPipe().transform(1234.56)).toBe('1.234,56');
  });

  it('appends the convertible mark', () => {
    expect(new AppCurrencyPipe().transform(1234.56)).toBe('1.234,56 KM');
  });
});
