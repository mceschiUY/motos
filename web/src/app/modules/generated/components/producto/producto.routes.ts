import { Routes } from '@angular/router';
import { rutasDeEntidad } from '../../../../core/routing/generated-routes.factory';

export const productoRoutes: Routes = rutasDeEntidad(
  () => import('./producto-list/producto-list.component').then(m => m.ProductoListComponent),
  () => import('./producto-ficha/producto-ficha.component').then(m => m.ProductoFichaComponent),
);