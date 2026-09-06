import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { GENERATED_ROUTES } from './modules/generated/generated-components.registry';

export const routes: Routes = [
  // Ruta de login (sin proteccion)
  {
    path: 'login',
    loadComponent: () => import('./auth/login/login.component').then(m => m.LoginComponent)
  },
  // Ventana standalone de mutation (sin layout, para abrir en popup)
  {
    path: 'mutation-window',
    loadComponent: () => import('./layout/components/mutation-window/mutation-window.component').then(m => m.MutationWindowComponent)
  },
  // Modo pantalla: el negocio en la TV de la oficina (fullscreen, sin layout)
  {
    path: 'pantalla',
    canActivate: [authGuard],
    loadComponent: () => import('./pantalla/pantalla.component').then(m => m.PantallaComponent)
  },
  // Rutas protegidas con layout
  {
    path: '',
    loadComponent: () => import('./layout/site-layout.component').then(m => m.SiteLayoutComponent),
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./modules/generated/dashboard/producto-home.component').then(m => m.ProductoHomeComponent)
      },
      // Modulo de Seguridad
      {
        path: 'seguridad',
        loadChildren: () => import('./modules/seguridad/seguridad.routes').then(m => m.SEGURIDAD_ROUTES)
      },
      // Modulo de Configuracion
      {
        path: 'configuracion',
        loadChildren: () => import('./modules/configuracion/configuracion.routes').then(m => m.CONFIGURACION_ROUTES)
      },
      // Modulo de Auditoria
      {
        path: 'auditoria',
        loadChildren: () => import('./modules/auditoria/auditoria.routes').then(m => m.AUDITORIA_ROUTES)
      },
      // Modulo de Reportes
      {
        path: 'reportes',
        loadChildren: () => import('./modules/reportes/reportes.routes').then(m => m.REPORTES_ROUTES)
      },
      // Modulo de Evolution Board
      {
        path: 'evolution',
        loadChildren: () => import('./modules/evolution-board/evolution-board.routes').then(m => m.EVOLUTION_BOARD_ROUTES)
      },
      // Rutas generadas dinamicamente
      ...GENERATED_ROUTES
    ]
  },
  // Wildcard
  {
    path: '**',
    redirectTo: ''
  }
];
