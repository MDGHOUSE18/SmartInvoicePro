import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./features/landing/landing-page.component').then((m) => m.LandingPageComponent),
    canActivate: [guestGuard],
  },
  {
    path: 'auth',
    loadComponent: () => import('./layout/auth-layout/auth-layout.component').then((m) => m.AuthLayoutComponent),
    loadChildren: () => import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES),
  },
  {
    path: '',
    loadComponent: () => import('./layout/main-layout/main-layout.component').then((m) => m.MainLayoutComponent),
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadChildren: () => import('./features/dashboard/dashboard.routes').then((m) => m.DASHBOARD_ROUTES),
      },
      {
        path: 'customers',
        loadChildren: () => import('./features/customers/customers.routes').then((m) => m.CUSTOMER_ROUTES),
      },
      {
        path: 'products',
        loadChildren: () => import('./features/products/products.routes').then((m) => m.PRODUCT_ROUTES),
      },
      {
        path: 'invoices',
        loadChildren: () => import('./features/invoices/invoices.routes').then((m) => m.INVOICE_ROUTES),
      },
      {
        path: 'payments',
        loadChildren: () => import('./features/payments/payments.routes').then((m) => m.PAYMENT_ROUTES),
      },
      {
        path: 'reports',
        canActivate: [adminGuard],
        loadChildren: () => import('./features/reports/reports.routes').then((m) => m.REPORT_ROUTES),
      },
      {
        path: 'settings',
        loadChildren: () => import('./features/settings/settings.routes').then((m) => m.SETTINGS_ROUTES),
      },
      {
        path: 'profile',
        loadChildren: () => import('./features/profile/profile.routes').then((m) => m.PROFILE_ROUTES),
      },
      {
        path: 'notifications',
        loadChildren: () => import('./features/notifications/notifications.routes').then((m) => m.NOTIFICATION_ROUTES),
      },
      {
        path: 'audit-logs',
        canActivate: [adminGuard],
        loadChildren: () => import('./features/audit-logs/audit-logs.routes').then((m) => m.AUDIT_LOG_ROUTES),
      },
      {
        path: 'demo',
        canActivate: [adminGuard],
        loadChildren: () => import('./features/demo/demo.routes').then((m) => m.DEMO_ROUTES),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
