import { Routes } from '@angular/router';

export const REPORT_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./reports-home/reports-home.component').then((m) => m.ReportsHomeComponent) },
];
