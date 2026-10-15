import { formatDate, formatNumber } from '@angular/common';
import { Pipe, PipeTransform } from '@angular/core';

export const REGIONAL_LOCALE = 'bs-Latn';

function asDate(value: unknown): Date | null {
  if (value == null || value === '') return null;
  if (value instanceof Date) return Number.isNaN(value.getTime()) ? null : value;
  const text = String(value);
  const dateOnly = /^(\d{4})-(\d{2})-(\d{2})$/.exec(text);
  if (dateOnly) {
    return new Date(Number(dateOnly[1]), Number(dateOnly[2]) - 1, Number(dateOnly[3]));
  }
  const parsed = new Date(text);
  return Number.isNaN(parsed.getTime()) ? null : parsed;
}

function asNumber(value: unknown): number | null {
  if (value == null || value === '') return null;
  const number = typeof value === 'number' ? value : Number(value);
  return Number.isFinite(number) ? number : null;
}

@Pipe({ name: 'appDate' })
export class AppDatePipe implements PipeTransform {
  transform(value: unknown): string {
    const date = asDate(value);
    return date ? formatDate(date, 'dd.MM.yyyy', REGIONAL_LOCALE) : '';
  }
}

@Pipe({ name: 'appDateTime' })
export class AppDateTimePipe implements PipeTransform {
  transform(value: unknown): string {
    const date = asDate(value);
    return date ? formatDate(date, 'dd.MM.yyyy HH:mm', REGIONAL_LOCALE) : '';
  }
}

@Pipe({ name: 'appNumber' })
export class AppNumberPipe implements PipeTransform {
  transform(value: unknown, digits = '1.2-2'): string {
    const number = asNumber(value);
    return number == null ? '' : formatNumber(number, REGIONAL_LOCALE, digits);
  }
}

@Pipe({ name: 'appCurrency' })
export class AppCurrencyPipe implements PipeTransform {
  transform(value: unknown): string {
    const number = asNumber(value);
    return number == null ? '' : `${formatNumber(number, REGIONAL_LOCALE, '1.2-2')} KM`;
  }
}
