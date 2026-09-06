import { Routes } from '@angular/router';

export const REPORTES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/reportes-dashboard/reportes-dashboard.component')
      .then(m => m.ReportesDashboardComponent),
    title: 'Reportes'
  },
  {
    path: 'historial',
    loadComponent: () => import('./components/historial-list/historial-list.component')
      .then(m => m.HistorialListComponent),
    title: 'Historial de Reportes'
  },
  {
    path: 'programados',
    loadComponent: () => import('./components/programados-list/programados-list.component')
      .then(m => m.ProgramadosListComponent),
    title: 'Reportes Programados'
  }
];
