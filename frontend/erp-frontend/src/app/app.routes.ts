import { Routes } from '@angular/router';
import { authGuard } from '@erp-platform/core';
import { permissionGuard } from '@erp-platform/core';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },

  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
  },

  // System pages (guard redirect targets).
  { path: 'login', loadComponent: () => import('./features/system/login.component').then((m) => m.LoginComponent) },
  { path: 'forbidden', loadComponent: () => import('./features/system/forbidden.component').then((m) => m.ForbiddenComponent) },

  {
    path: 'customers',
    canActivate: [authGuard, permissionGuard],
    data: { permission: 'customers.view', breadcrumb: "Customers" },
    loadComponent: () =>
      import('./features/customers/customers-list.component.generated').then((m) => m.CustomerListComponent),
  },

  // Generated module routes are inserted above this anchor by @erp-platform:module
  {
    path: 'products',
    canActivate: [authGuard, permissionGuard],
    data: { permission: 'products.view', breadcrumb: "Products" },
    loadComponent: () =>
      import('./features/products/products-list.component.generated').then((m) => m.ProductListComponent),
  },

  // erp-generated:routes

  // Security administration (persisted identity: real users & roles).
  {
    path: 'security/users',
    canActivate: [authGuard, permissionGuard],
    data: { permission: 'users.view', breadcrumb: 'Users' },
    loadComponent: () => import('./features/security/users/users-list.component').then((m) => m.UsersListComponent),
  },
  {
    path: 'security/roles',
    canActivate: [authGuard, permissionGuard],
    data: { permission: 'roles.view', breadcrumb: 'Roles' },
    loadComponent: () => import('./features/security/roles/roles-list.component').then((m) => m.RolesListComponent),
  },

  { path: '**', redirectTo: 'dashboard' },
];
