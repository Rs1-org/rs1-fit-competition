import { HttpErrorResponse } from '@angular/common/http';

/** Turns the API's error body ({ message, errors: [{ propertyName, errorMessage }] }) into one readable line. */
export function apiErrorMessage(err: unknown, fallback = 'Something went wrong. Please try again.'): string {
  if (!(err instanceof HttpErrorResponse)) return fallback;
  if (err.status === 401 || err.status === 403) return 'You are not allowed to do this.';
  const body = err.error as { message?: string; errors?: { errorMessage: string }[] } | null;
  if (body?.errors?.length) return body.errors.map((e) => e.errorMessage).join(' ');
  return body?.message ?? fallback;
}
