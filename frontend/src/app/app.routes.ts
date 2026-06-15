import { Routes } from '@angular/router';

import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./loans/components/loans-page/loans-page.component').then(
        m => m.LoansPageComponent,
      ),
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./auth/components/login/login.component').then(
        m => m.LoginComponent,
      ),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./auth/components/register/register.component').then(
        m => m.RegisterComponent,
      ),
  },
  { path: '**', redirectTo: '' },
];
