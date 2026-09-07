import { Routes } from '@angular/router';
import { rutasDeEntidad } from '../../../../core/routing/generated-routes.factory';

export const varianteRoutes: Routes = rutasDeEntidad(
  () => import('./variante-list/variante-list.component').then(m => m.VarianteListComponent),
  () => import('./variante-ficha/variante-ficha.component').then(m => m.VarianteFichaComponent),
);