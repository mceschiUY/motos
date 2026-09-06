import { Routes } from '@angular/router';
import { rutasDeEntidad } from '../../../../core/routing/generated-routes.factory';

export const parametroslaRoutes: Routes = rutasDeEntidad(
  () => import('./parametrosla-list/parametrosla-list.component').then(m => m.ParametroslaListComponent),
  () => import('./parametrosla-ficha/parametrosla-ficha.component').then(m => m.ParametroslaFichaComponent),
);