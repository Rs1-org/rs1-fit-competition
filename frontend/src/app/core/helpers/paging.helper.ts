import { HttpParams } from '@angular/common/http';
import { EMPTY, Observable, expand, map, reduce } from 'rxjs';
import { PagedResult } from '../models/paged-result';

/** Backend cap for pageSize (see GetAll* query validators). */
export const MAX_PAGE_SIZE = 100;

/** Builds paging + filter query params, skipping filter values that are not set. */
export function toPagedParams(page: number, pageSize: number, filter: object = {}): HttpParams {
  let params = new HttpParams().set('page', page).set('pageSize', pageSize);
  for (const [key, value] of Object.entries(filter)) {
    if (value !== null && value !== undefined && value !== '') params = params.set(key, value);
  }
  return params;
}

/** Filter (+ sort) query params without paging, skipping values that are not set (exports). */
export function toFilterParams(filter: object = {}): HttpParams {
  let params = new HttpParams();
  for (const [key, value] of Object.entries(filter)) {
    if (value !== null && value !== undefined && value !== '') params = params.set(key, value);
  }
  return params;
}

/** Loads every page of a paged endpoint and returns all items (for dropdowns). */
export function fetchAllPages<T>(getPage: (page: number) => Observable<PagedResult<T>>): Observable<T[]> {
  return getPage(1).pipe(
    expand((result) => (result.hasNextPage ? getPage(result.page + 1) : EMPTY)),
    map((result) => result.items),
    reduce((all, items) => all.concat(items), [] as T[]),
  );
}
