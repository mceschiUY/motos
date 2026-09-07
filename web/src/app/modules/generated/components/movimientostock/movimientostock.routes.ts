import { Routes } from '@angular/router';
import { rutasDeEntidad } from '../../../../core/routing/generated-routes.factory';

export const movimientostockRoutes: Routes = rutasDeEntidad(
  () => import('./movimientostock-list/movimientostock-list.component').then(m => m.MovimientoStockListComponent),
  () => import('./movimientostock-ficha/movimientostock-ficha.component').then(m => m.MovimientoStockFichaComponent),
);