import { Injectable, signal } from '@angular/core';
import { Observable, defer, finalize } from 'rxjs';

/** How a request is surfaced: a top progress bar, a blocking overlay, or nothing. */
export type LoaderMode = 'auto' | 'bar' | 'overlay' | 'none';
export type ActiveLoaderMode = 'bar' | 'overlay';

/** Overlay only appears for work that outlasts this, so fast calls don't flash. */
const OVERLAY_DELAY_MS = 250;
/** Bar lingers briefly so back-to-back requests read as one. */
const BAR_HIDE_DELAY_MS = 200;

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private pending = 0;
  private blocking = 0;
  private barHideTimer?: ReturnType<typeof setTimeout>;
  private overlayShowTimer?: ReturnType<typeof setTimeout>;

  readonly barVisible = signal(false);
  readonly overlayVisible = signal(false);

  start(mode: ActiveLoaderMode): void {
    this.pending++;
    clearTimeout(this.barHideTimer);
    this.barVisible.set(true);

    if (mode === 'overlay' && ++this.blocking === 1) {
      this.overlayShowTimer = setTimeout(() => this.overlayVisible.set(true), OVERLAY_DELAY_MS);
    }
  }

  stop(mode: ActiveLoaderMode): void {
    this.pending = Math.max(0, this.pending - 1);
    if (this.pending === 0) {
      this.barHideTimer = setTimeout(() => this.barVisible.set(false), BAR_HIDE_DELAY_MS);
    }

    if (mode === 'overlay') {
      this.blocking = Math.max(0, this.blocking - 1);
      if (this.blocking === 0) {
        clearTimeout(this.overlayShowTimer);
        this.overlayVisible.set(false);
      }
    }
  }

  /** Shows the loader for any observable work that isn't an HTTP call. */
  track<T>(source: Observable<T>, mode: ActiveLoaderMode = 'overlay'): Observable<T> {
    return defer(() => {
      this.start(mode);
      return source.pipe(finalize(() => this.stop(mode)));
    });
  }
}
