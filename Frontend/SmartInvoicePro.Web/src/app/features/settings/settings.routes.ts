import { Routes } from '@angular/router';

export const SETTINGS_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./settings-form/settings-form.component').then((m) => m.SettingsFormComponent) },
];
