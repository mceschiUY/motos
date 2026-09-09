import { Routes } from '@angular/router';
import { rutasDeEntidad } from '../../../../core/routing/generated-routes.factory';

export const vendedorRoutes: Routes = rutasDeEntidad(
  () => import('./vendedor-list/vendedor-list.component').then(m => m.VendedorListComponent),
  () => import('./vendedor-ficha/vendedor-ficha.component').then(m => m.VendedorFichaComponent),
);