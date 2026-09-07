import { Routes } from '@angular/router';
import { rutasDeEntidad } from '../../../../core/routing/generated-routes.factory';

export const colorRoutes: Routes = rutasDeEntidad(
  () => import('./color-list/color-list.component').then(m => m.ColorListComponent),
  () => import('./color-ficha/color-ficha.component').then(m => m.ColorFichaComponent),
);