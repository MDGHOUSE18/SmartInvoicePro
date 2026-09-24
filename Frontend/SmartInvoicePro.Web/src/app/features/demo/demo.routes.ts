import { Routes } from '@angular/router';

export const DEMO_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./demo-reset/demo-reset.component').then((m) => m.DemoResetComponent) },
];
