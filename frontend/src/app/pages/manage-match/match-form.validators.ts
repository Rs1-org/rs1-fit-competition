import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

const isSet = (value: unknown) => value !== null && value !== undefined && value !== '';

/** Mirrors the backend: home and away selection must be different. */
export const teamsDifferValidator: ValidatorFn = (group: AbstractControl): ValidationErrors | null => {
  const { homeSelectionPublicId: home, awaySelectionPublicId: away } = group.value;
  return home && away && home === away ? { sameTeams: true } : null;
};

/**
 * Mirrors UpdateMatchCommandValidator: full-time and half-time scores are set in pairs,
 * and a half-time score cannot exceed the full-time score.
 */
export const scoresValidator: ValidatorFn = (group: AbstractControl): ValidationErrors | null => {
  const { homeScore, awayScore, halfHomeScore, halfAwayScore } = group.value;
  const errors: ValidationErrors = {};
  if (isSet(homeScore) !== isSet(awayScore)) errors['scorePair'] = true;
  if (isSet(halfHomeScore) !== isSet(halfAwayScore)) errors['halfScorePair'] = true;
  if (
    (isSet(halfHomeScore) && isSet(homeScore) && halfHomeScore > homeScore) ||
    (isSet(halfAwayScore) && isSet(awayScore) && halfAwayScore > awayScore)
  ) {
    errors['halfAboveFull'] = true;
  }
  return Object.keys(errors).length ? errors : null;
};

/** Mirrors CreateMatchCommandValidator: a new match is scheduled in the future. */
export const futureDateValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  if (!control.value) return null;
  return new Date(control.value).getTime() > Date.now() ? null : { pastDate: true };
};

/** "2026-10-30T16:00:00Z" → "2026-10-30T18:00" in the browser's time zone (datetime-local value). */
export function toLocalInputValue(iso: string | null): string {
  if (!iso) return '';
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return '';
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}
