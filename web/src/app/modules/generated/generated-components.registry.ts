import { Routes } from '@angular/router';

// Registro de componentes generados por Kosmos
// Este archivo se actualiza automaticamente cuando se generan nuevos componentes

export interface GeneratedComponentInfo {
  name: string;
  path: string;
  listComponent: string;
  formComponent: string;
  detailComponent: string;
  icon: string;
  label: string;
}

// Lista de componentes generados
export const GENERATED_COMPONENTS: GeneratedComponentInfo[] = [
  // Los componentes generados se agregaran aqui automaticamente
  {
    name: 'documento',
    path: 'documento',
    listComponent: 'DocumentoListComponent',
    formComponent: 'DocumentoFormComponent',
    detailComponent: 'DocumentoDetailComponent',
    icon: 'description',
    label: 'Documento'
  },

  {
    name: 'conceptoexpensa',
    path: 'conceptoexpensa',
    listComponent: 'ConceptoexpensaListComponent',
    formComponent: 'ConceptoexpensaFormComponent',
    detailComponent: 'ConceptoexpensaDetailComponent',
    icon: 'list_alt',
    label: 'ConceptoExpensa'
  },
  {
    name: 'cliente',
    path: 'cliente',
    listComponent: 'ClienteListComponent',
    formComponent: 'ClienteFormComponent',
    detailComponent: 'ClienteDetailComponent',
    icon: 'person',
    label: 'Cliente'
  },
  {
    name: 'envio',
    path: 'envio',
    listComponent: 'EnvioListComponent',
    formComponent: 'EnvioFormComponent',
    detailComponent: 'EnvioDetailComponent',
    icon: 'list_alt',
    label: 'Envio'
  },
  {
    name: 'agencia',
    path: 'agencia',
    listComponent: 'AgenciaListComponent',
    formComponent: 'AgenciaFormComponent',
    detailComponent: 'AgenciaDetailComponent',
    icon: 'list_alt',
    label: 'Agencia'
  },
  {
    name: 'observacion',
    path: 'observacion',
    listComponent: 'ObservacionListComponent',
    formComponent: 'ObservacionFormComponent',
    detailComponent: 'ObservacionDetailComponent',
    icon: 'list_alt',
    label: 'Observacion'
  },
  {
    name: 'parametrosla',
    path: 'parametrosla',
    listComponent: 'ParametroslaListComponent',
    formComponent: 'ParametroslaFormComponent',
    detailComponent: 'ParametroslaDetailComponent',
    icon: 'list_alt',
    label: 'ParametroSLA'
  },];

// Rutas dinamicas para los componentes generados
export const GENERATED_ROUTES: Routes = [
  // Las rutas se agregaran automaticamente cuando se generen componentes
  {
    path: 'documento',
    loadComponent: () => import('./components/documento/documento-list/documento-list.component').then(m => m.DocumentoListComponent)
  },

  {
    path: 'conceptoexpensa',
    loadComponent: () => import('./components/conceptoexpensa/conceptoexpensa-list/conceptoexpensa-list.component').then(m => m.ConceptoexpensaListComponent)
  },
  {
    path: 'cliente',
    loadComponent: () => import('./components/cliente/cliente-list/cliente-list.component').then(m => m.ClienteListComponent)
  },
  {
    path: 'cliente/:id',
    loadComponent: () => import('./components/cliente/cliente-ficha/cliente-ficha.component').then(m => m.ClienteFichaComponent)
  },
  {
    path: 'envio',
    loadComponent: () => import('./components/envio/envio-list/envio-list.component').then(m => m.EnvioListComponent)
  },
  {
    path: 'envio/:id',
    loadComponent: () => import('./components/envio/envio-ficha/envio-ficha.component').then(m => m.EnvioFichaComponent)
  },
  // Ruta de navegación: Envio filtrado por Cliente
  {
    path: 'envio/by-cliente/:clienteId',
    loadComponent: () => import('./components/envio/envio-list/envio-list.component').then(m => m.EnvioListComponent)
  },
  // Ruta de navegación: Envio filtrado por Agencia
  {
    path: 'envio/by-agencia/:agenciaId',
    loadComponent: () => import('./components/envio/envio-list/envio-list.component').then(m => m.EnvioListComponent)
  },
  {
    path: 'agencia',
    loadComponent: () => import('./components/agencia/agencia-list/agencia-list.component').then(m => m.AgenciaListComponent)
  },
  {
    path: 'agencia/:id',
    loadComponent: () => import('./components/agencia/agencia-ficha/agencia-ficha.component').then(m => m.AgenciaFichaComponent)
  },
  {
    path: 'observacion',
    loadComponent: () => import('./components/observacion/observacion-list/observacion-list.component').then(m => m.ObservacionListComponent)
  },
  {
    path: 'observacion/:id',
    loadComponent: () => import('./components/observacion/observacion-ficha/observacion-ficha.component').then(m => m.ObservacionFichaComponent)
  },
  // Ruta de navegación: Observacion filtrado por Envio
  {
    path: 'observacion/by-envio/:envioId',
    loadComponent: () => import('./components/observacion/observacion-list/observacion-list.component').then(m => m.ObservacionListComponent)
  },
  {
    path: 'parametrosla',
    loadComponent: () => import('./components/parametrosla/parametrosla-list/parametrosla-list.component').then(m => m.ParametroslaListComponent)
  },
  {
    path: 'parametrosla/:id',
    loadComponent: () => import('./components/parametrosla/parametrosla-ficha/parametrosla-ficha.component').then(m => m.ParametroslaFichaComponent)
  },];
