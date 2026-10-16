import { SortParams } from '../models/import-export';

/** Clicking a column: a new column starts ascending, the active column flips asc ↔ desc. */
export function nextSort(current: SortParams, key: string): SortParams {
  if (current.sortBy !== key) return { sortBy: key, sortDir: 'asc' };
  return { sortBy: key, sortDir: current.sortDir === 'asc' ? 'desc' : 'asc' };
}

/** Value for the column header's `aria-sort` attribute. */
export function ariaSort(current: SortParams, key: string): 'ascending' | 'descending' | 'none' {
  if (current.sortBy !== key) return 'none';
  return current.sortDir === 'desc' ? 'descending' : 'ascending';
}

/** Arrow shown next to the column label. */
export function sortIndicator(current: SortParams, key: string): string {
  if (current.sortBy !== key) return '↕';
  return current.sortDir === 'desc' ? '▼' : '▲';
}
