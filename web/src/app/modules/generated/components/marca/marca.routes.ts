import { Routes } from '@angular/router';
import { rutasDeEntidad } from '../../../../core/routing/generated-routes.factory';

export const marcaRoutes: Routes = rutasDeEntidad(
  () => import('./marca-list/marca-list.component').then(m => m.MarcaListComponent),
  () => import('./marca-ficha/marca-ficha.component').then(m => m.MarcaFichaComponent),
);