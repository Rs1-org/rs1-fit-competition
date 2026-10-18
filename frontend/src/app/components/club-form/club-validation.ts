import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}

export const clubNameValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const name = String(control.value ?? '').trim();
  return name.length >= 2 &&
    /[a-z0-9]/i.test(
      name
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .replace(/[đð]/gi, 'd'),
    )
    ? null
    : { clubName: true };
};

export const foundingDateValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const value = String(control.value ?? '');
  if (!value) return null;
  const parsed = new Date(`${value}T00:00:00Z`);
  return /^\d{4}-\d{2}-\d{2}$/.test(value) &&
    Number(value.slice(0, 4)) >= 1 &&
    !Number.isNaN(parsed.getTime()) &&
    parsed.toISOString().slice(0, 10) === value &&
    value <= todayIso()
    ? null
    : { foundingDate: true };
};

export const clubDateRangeValidator: ValidatorFn = (
  group: AbstractControl,
): ValidationErrors | null => {
  const { foundedFrom, foundedTo } = group.value;
  return foundedFrom && foundedTo && foundedFrom > foundedTo ? { dateRange: true } : null;
};
