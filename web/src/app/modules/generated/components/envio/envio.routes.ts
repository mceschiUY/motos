import { Routes } from '@angular/router';
import { rutasDeEntidad } from '../../../../core/routing/generated-routes.factory';

export const envioRoutes: Routes = rutasDeEntidad(
  () => import('./envio-list/envio-list.component').then(m => m.EnvioListComponent),
  () => import('./envio-ficha/envio-ficha.component').then(m => m.EnvioFichaComponent),
);