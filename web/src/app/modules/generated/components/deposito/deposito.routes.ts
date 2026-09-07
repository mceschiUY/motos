import { Routes } from '@angular/router';
import { rutasDeEntidad } from '../../../../core/routing/generated-routes.factory';

export const depositoRoutes: Routes = rutasDeEntidad(
  () => import('./deposito-list/deposito-list.component').then(m => m.DepositoListComponent),
  () => import('./deposito-ficha/deposito-ficha.component').then(m => m.DepositoFichaComponent),
);