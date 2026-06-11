import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let message = 'An unexpected error occurred.';

      if (error.status === 401) {
        message = 'Session expired. Please log in again.';
        void router.navigate(['/login']);
      } else if (error.status === 0) {
        message = 'Unable to connect to the server.';
      } else if (error.error?.message) {
        const details = error.error.details?.length
          ? `: ${error.error.details.join(', ')}`
          : '';
        message = `${error.error.message}${details}`;
      } else if (error.status >= 500) {
        message = 'Server error. Please try again later.';
      }

      snackBar.open(message, 'Close', {
        duration: 5000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
        panelClass: error.status >= 500 ? ['snackbar-error'] : ['snackbar-warn'],
      });

      return throwError(() => error);
    }),
  );
};
