import { Routes } from '@angular/router';
import { rutasDeEntidad } from '../../../../core/routing/generated-routes.factory';

export const pedidoRoutes: Routes = rutasDeEntidad(
  () => import('./pedido-list/pedido-list.component').then(m => m.PedidoListComponent),
  () => import('./pedido-ficha/pedido-ficha.component').then(m => m.PedidoFichaComponent),
);