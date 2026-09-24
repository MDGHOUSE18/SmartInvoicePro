import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { catchError, switchMap, throwError } from 'rxjs';

let refreshInProgress = false;

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.getAccessToken();

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || req.url.includes('/auth/login') || req.url.includes('/auth/refresh')) {
        return throwError(() => error);
      }

      if (refreshInProgress) {
        return auth.refreshToken().pipe(
          switchMap((result) => {
            if (!result) {
              auth.logout();
              return throwError(() => error);
            }
            const retryReq = req.clone({
              setHeaders: { Authorization: `Bearer ${result.accessToken}` },
            });
            return next(retryReq);
          })
        );
      }

      refreshInProgress = true;
      return auth.refreshToken().pipe(
        switchMap((result) => {
          refreshInProgress = false;
          if (!result) {
            auth.logout();
            return throwError(() => error);
          }
          const retryReq = req.clone({
            setHeaders: { Authorization: `Bearer ${result.accessToken}` },
          });
          return next(retryReq);
        }),
        catchError((refreshError) => {
          refreshInProgress = false;
          auth.logout();
          return throwError(() => refreshError);
        })
      );
    })
  );
};
