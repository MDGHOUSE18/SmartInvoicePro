import { HttpContext, HttpContextToken, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { ActiveLoaderMode, LoaderMode, LoadingService } from '../services/loading.service';

export const LOADER_MODE = new HttpContextToken<LoaderMode>(() => 'auto');

/** Overrides the automatic loader choice for a single request. */
export const withLoader = (mode: LoaderMode, context = new HttpContext()): HttpContext =>
  context.set(LOADER_MODE, mode);

// Auth calls have their own button spinners; notifications are polled in the background.
const SILENT_URLS = [/\/auth\//, /\/notifications(\/|\?|$)/];

function resolveMode(req: HttpRequest<unknown>): LoaderMode {
  const mode = req.context.get(LOADER_MODE);
  if (mode !== 'auto') return mode;
  if (SILENT_URLS.some((pattern) => pattern.test(req.url))) return 'none';
  // Writes and file downloads are "heavy": block the screen until they finish.
  if (req.method !== 'GET' || req.responseType === 'blob') return 'overlay';
  return 'bar';
}

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const mode = resolveMode(req);
  if (mode === 'none' || mode === 'auto') return next(req);

  const loading = inject(LoadingService);
  const active: ActiveLoaderMode = mode;
  loading.start(active);
  return next(req).pipe(finalize(() => loading.stop(active)));
};
