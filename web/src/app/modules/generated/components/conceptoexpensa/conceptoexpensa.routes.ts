import { Routes } from '@angular/router';

export const CONCEPTOEXPENSA_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./conceptoexpensa-list/conceptoexpensa-list.component').then(m => m.ConceptoexpensaListComponent)
  }
];

export default CONCEPTOEXPENSA_ROUTES;
