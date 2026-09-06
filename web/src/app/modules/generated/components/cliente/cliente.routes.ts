import { Routes } from '@angular/router';
import { rutasDeEntidad } from '../../../../core/routing/generated-routes.factory';

export const clienteRoutes: Routes = rutasDeEntidad(
  () => import('./cliente-list/cliente-list.component').then(m => m.ClienteListComponent),
  () => import('./cliente-ficha/cliente-ficha.component').then(m => m.ClienteFichaComponent),
);