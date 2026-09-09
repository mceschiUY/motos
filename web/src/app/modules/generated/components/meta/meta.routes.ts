import { Routes } from '@angular/router';
import { rutasDeEntidad } from '../../../../core/routing/generated-routes.factory';

export const metaRoutes: Routes = rutasDeEntidad(
  () => import('./meta-list/meta-list.component').then(m => m.MetaListComponent),
  () => import('./meta-ficha/meta-ficha.component').then(m => m.MetaFichaComponent),
);