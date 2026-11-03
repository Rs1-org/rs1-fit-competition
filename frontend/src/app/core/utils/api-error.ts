import { HttpErrorResponse } from '@angular/common/http';
import { FormGroup } from '@angular/forms';

/** Turns the API's error body ({ message, errors: [{ propertyName, errorMessage }] }) into one readable line. */
export function apiErrorMessage(err: unknown, fallback = 'Something went wrong. Please try again.'): string {
  if (!(err instanceof HttpErrorResponse)) return fallback;
  if (err.status === 401 || err.status === 403) return 'You are not allowed to do this.';
  const body = err.error as { message?: string; errors?: { errorMessage: string }[] } | null;
  if (body?.errors?.length) return body.errors.map((e) => e.errorMessage).join(' ');
  return body?.message ?? fallback;
}

/**
 * Puts backend field errors next to their inputs (InputBox shows `server` errors).
 * Returns the line for a toast or form message, or '' when every error found its field.
 */
export function showServerErrors(form: FormGroup, err: unknown, fallback: string): string {
  const errors = err instanceof HttpErrorResponse
    ? (err.error?.errors as { propertyName?: string; errorMessage: string }[] | undefined) ?? []
    : [];
  let unmatched = !errors.length;
  for (const e of errors) {
    const name = e.propertyName ? e.propertyName.charAt(0).toLowerCase() + e.propertyName.slice(1) : '';
    const control = name ? form.get(name) : null;
    if (control) {
      control.setErrors({ server: e.errorMessage });
      control.markAsTouched();
    } else {
      unmatched = true;
    }
  }
  return unmatched ? apiErrorMessage(err, fallback) : '';
}
