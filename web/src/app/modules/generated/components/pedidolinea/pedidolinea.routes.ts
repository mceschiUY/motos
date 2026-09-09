import { Routes } from '@angular/router';
import { rutasDeEntidad } from '../../../../core/routing/generated-routes.factory';

export const pedidolineaRoutes: Routes = rutasDeEntidad(
  () => import('./pedidolinea-list/pedidolinea-list.component').then(m => m.PedidoLineaListComponent),
  () => import('./pedidolinea-ficha/pedidolinea-ficha.component').then(m => m.PedidoLineaFichaComponent),
);