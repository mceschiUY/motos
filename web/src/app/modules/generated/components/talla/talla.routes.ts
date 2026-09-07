import { Routes } from '@angular/router';
import { rutasDeEntidad } from '../../../../core/routing/generated-routes.factory';

export const tallaRoutes: Routes = rutasDeEntidad(
  () => import('./talla-list/talla-list.component').then(m => m.TallaListComponent),
  () => import('./talla-ficha/talla-ficha.component').then(m => m.TallaFichaComponent),
);