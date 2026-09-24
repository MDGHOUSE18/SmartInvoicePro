import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { map, of, switchMap } from 'rxjs';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) {
    return true;
  }

  if (auth.getAccessToken()) {
    return auth.loadCurrentUser().pipe(
      map((user) => {
        if (user) return true;
        router.navigate(['/auth/login']);
        return false;
      })
    );
  }

  router.navigate(['/auth/login']);
  return false;
};

export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) {
    router.navigate(['/dashboard']);
    return false;
  }

  if (auth.getAccessToken()) {
    return auth.loadCurrentUser().pipe(
      switchMap((user) => {
        if (user) {
          router.navigate(['/dashboard']);
          return of(false);
        }
        return of(true);
      })
    );
  }

  return true;
};
