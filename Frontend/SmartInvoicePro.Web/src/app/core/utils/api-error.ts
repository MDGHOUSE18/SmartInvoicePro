import { HttpErrorResponse } from '@angular/common/http';

/** Prefer API ApiResponse.message / errors over raw HttpErrorResponse.message. */
export function getApiErrorMessage(err: unknown, fallback = 'Request failed'): string {
  if (err instanceof HttpErrorResponse) {
    const body = err.error as { message?: string; errors?: string[] } | string | null;
    if (body && typeof body === 'object') {
      if (Array.isArray(body.errors) && body.errors.length > 0) {
        return body.errors.join(' ');
      }
      if (typeof body.message === 'string' && body.message.trim()) {
        return body.message;
      }
    }
    if (typeof body === 'string' && body.trim()) {
      return body;
    }
    if (err.status === 0) {
      return 'Unable to reach the server. Check that the API is running.';
    }
    return fallback;
  }

  if (err instanceof Error && err.message) {
    return err.message;
  }

  return fallback;
}
