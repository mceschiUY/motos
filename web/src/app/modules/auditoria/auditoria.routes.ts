import { Routes } from '@angular/router';

export const AUDITORIA_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/audit-log-list/audit-log-list.component')
      .then(m => m.AuditLogListComponent),
    title: 'Auditoria'
  }
];
