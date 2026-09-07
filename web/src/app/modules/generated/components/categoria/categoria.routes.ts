import { Routes } from '@angular/router';
import { rutasDeEntidad } from '../../../../core/routing/generated-routes.factory';

export const categoriaRoutes: Routes = rutasDeEntidad(
  () => import('./categoria-list/categoria-list.component').then(m => m.CategoriaListComponent),
  () => import('./categoria-ficha/categoria-ficha.component').then(m => m.CategoriaFichaComponent),
);