import { AbstractControl, ValidationErrors } from '@angular/forms';

function dateValue(value: string): Date | null {
  if (!/^\d{4}-\d{2}-\d{2}$/.test(value)) return null;
  const date = new Date(`${value}T00:00:00`);
  const [year, month, day] = value.split('-').map(Number);
  return year > 0 && date.getFullYear() === year && date.getMonth() + 1 === month && date.getDate() === day ? date : null;
}

export function userDateRangeValidator(group: AbstractControl): ValidationErrors | null {
  const from = group.get('registeredFrom')?.value as string;
  const to = group.get('registeredTo')?.value as string;
  if ((from && !dateValue(from)) || (to && !dateValue(to))) return { dateRange: true };
  return from && to && from > to ? { dateRange: true } : null;
}

// Local calendar-day boundaries match the local dates displayed by the regional pipes.
export function userDateBoundary(value: string, end = false): string | undefined {
  const date = dateValue(value);
  if (!date) return undefined;
  if (end) date.setHours(23, 59, 59, 999);
  return date.toISOString();
}
