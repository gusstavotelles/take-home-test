import { Routes } from '@angular/router';

import { LoansPageComponent } from './loans/components/loans-page/loans-page.component';

export const routes: Routes = [
  { path: '', component: LoansPageComponent },
  { path: '**', redirectTo: '' },
];
