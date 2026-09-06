import { Routes } from '@angular/router';

export const DOCUMENTO_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./documento-list/documento-list.component').then(m => m.DocumentoListComponent)
  }
];

export default DOCUMENTO_ROUTES;
