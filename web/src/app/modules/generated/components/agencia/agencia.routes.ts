import { Routes } from '@angular/router';
import { rutasDeEntidad } from '../../../../core/routing/generated-routes.factory';

export const agenciaRoutes: Routes = rutasDeEntidad(
  () => import('./agencia-list/agencia-list.component').then(m => m.AgenciaListComponent),
  () => import('./agencia-ficha/agencia-ficha.component').then(m => m.AgenciaFichaComponent),
);