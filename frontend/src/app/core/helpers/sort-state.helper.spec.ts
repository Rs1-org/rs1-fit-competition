import { ariaSort, nextSort, sortIndicator } from './sort-state.helper';
import { exportFileName } from './download.helper';

describe('sort-state helper', () => {
  it('starts a new column ascending', () => {
    expect(nextSort({}, 'name')).toEqual({ sortBy: 'name', sortDir: 'asc' });
    expect(nextSort({ sortBy: 'capacity', sortDir: 'desc' }, 'name')).toEqual({ sortBy: 'name', sortDir: 'asc' });
  });

  it('flips the active column between asc and desc', () => {
    expect(nextSort({ sortBy: 'name', sortDir: 'asc' }, 'name')).toEqual({ sortBy: 'name', sortDir: 'desc' });
    expect(nextSort({ sortBy: 'name', sortDir: 'desc' }, 'name')).toEqual({ sortBy: 'name', sortDir: 'asc' });
  });

  it('reports aria-sort and the arrow for the active column only', () => {
    const sort = { sortBy: 'population', sortDir: 'desc' as const };
    expect(ariaSort(sort, 'population')).toBe('descending');
    expect(ariaSort(sort, 'name')).toBe('none');
    expect(sortIndicator(sort, 'population')).toBe('▼');
    expect(sortIndicator({ sortBy: 'name', sortDir: 'asc' }, 'name')).toBe('▲');
    expect(sortIndicator(sort, 'name')).toBe('↕');
  });
});

describe('download helper', () => {
  it('builds a dated file name with the format extension', () => {
    expect(exportFileName('cities', 'xlsx', new Date(2026, 9, 6))).toBe('cities-2026-10-06.xlsx');
    expect(exportFileName('stadiums', 'csv', new Date(2026, 0, 31))).toBe('stadiums-2026-01-31.csv');
  });
});
