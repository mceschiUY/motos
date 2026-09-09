import { Routes } from '@angular/router';
import { rutasDeEntidad } from '../../../../core/routing/generated-routes.factory';

export const actividadRoutes: Routes = rutasDeEntidad(
  () => import('./actividad-list/actividad-list.component').then(m => m.ActividadListComponent),
  () => import('./actividad-ficha/actividad-ficha.component').then(m => m.ActividadFichaComponent),
);