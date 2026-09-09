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
      // Evolution Board: OCULTO (paridad con trenes, 2026-09-07). También está comentado
      // el ítem del menú en site-layout.component.html. Descomentar ambos para reactivarlo.
      // {
      //   path: 'evolution',
      //   loadChildren: () => import('./modules/evolution-board/evolution-board.routes').then(m => m.EVOLUTION_BOARD_ROUTES)
      // },
      // Vistas artesanales (no las toca la regen). Existencias: read model del Kardex.
      {
        path: 'existencias',
        loadComponent: () => import('./modules/artesanal/existencias/existencias.component').then(m => m.ExistenciasComponent)
      },
      // Agenda del vendedor: paradas por día/ciudad, mapa, ruta del día, meta del mes.
      {
        path: 'agenda',
        loadComponent: () => import('./modules/artesanal/agenda/agenda.component').then(m => m.AgendaComponent)
      },
      // Armado de pedido: la pantalla que vende (buscador de SKU con stock y precio).
      {
        path: 'pedidos/nuevo',
        loadComponent: () => import('./modules/artesanal/armado-pedido/armado-pedido.component').then(m => m.ArmadoPedidoComponent)
      },
      // Liquidación de comisiones: por vendedor y mes, sobre pedidos entregados.
      {
        path: 'comisiones',
        loadComponent: () => import('./modules/artesanal/comisiones/comisiones.component').then(m => m.ComisionesComponent)
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
