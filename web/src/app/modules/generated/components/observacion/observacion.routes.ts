import { Routes } from '@angular/router';
import { rutasDeEntidad } from '../../../../core/routing/generated-routes.factory';

export const observacionRoutes: Routes = rutasDeEntidad(
  () => import('./observacion-list/observacion-list.component').then(m => m.ObservacionListComponent),
  () => import('./observacion-ficha/observacion-ficha.component').then(m => m.ObservacionFichaComponent),
);