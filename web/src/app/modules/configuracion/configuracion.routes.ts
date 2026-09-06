import { Routes } from '@angular/router';

export const CONFIGURACION_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/configuracion-sitio/configuracion-sitio.component')
      .then(m => m.ConfiguracionSitioComponent)
  }
];
