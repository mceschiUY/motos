import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { GENERATED_ROUTES } from './modules/generated/generated-components.registry';
import { modoMovilGuard } from './modules/artesanal/movil/modo-movil.service';

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
  // Seguimiento público del envío: sin login ni layout, lo abre el cliente desde el QR.
  { path: 'seguimiento/:codigo', loadComponent: () => import('./modules/artesanal/seguimiento/seguimiento-publico.component').then(m => m.SeguimientoPublicoComponent) },
  // Marco de celular para la demo por Teams (plan Etapa F6): el sitio en un teléfono dibujado,
  // con el tablero de gerencia al lado. Sin layout; son iframes del mismo origen.
  {
    path: 'celular',
    canActivate: [authGuard],
    loadComponent: () => import('./modules/artesanal/celular/marco-celular.component').then(m => m.MarcoCelularComponent)
  },
  // App del vendedor (plan Etapa F4): shell móvil con barra inferior, sin sidebar. Va ANTES del
  // layout de escritorio; sus hijas reutilizan las escenas artesanales (ver movil.routes.ts).
  {
    path: 'm',
    canActivate: [authGuard],
    loadComponent: () => import('./modules/artesanal/movil/movil-layout.component').then(m => m.MovilLayoutComponent),
    loadChildren: () => import('./modules/artesanal/movil/movil.routes').then(m => m.MOVIL_ROUTES)
  },
  // Rutas protegidas con layout. Con el modo móvil activo, modoMovilGuard manda la misma URL a /m.
  {
    path: '',
    loadComponent: () => import('./layout/site-layout.component').then(m => m.SiteLayoutComponent),
    canActivate: [authGuard, modoMovilGuard],
    children: [
      // HOY: el centro de control como feed de acciones (escena artesanal, home del sitio).
      {
        path: '',
        loadComponent: () => import('./modules/artesanal/hoy/hoy.component').then(m => m.HoyComponent)
      },
      // Resumen genérico por entidad (el home anterior): queda accesible por si hace falta.
      {
        path: 'inicio-generico',
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
      // Catálogo comercial: la grilla que se le muestra al cliente y la ficha que se imprime.
      // Van ANTES de GENERATED_ROUTES; 'catalogo' no es una entidad, no hay choque de rutas.
      {
        path: 'catalogo',
        loadComponent: () => import('./modules/artesanal/catalogo/catalogo.component').then(m => m.CatalogoComponent)
      },
      {
        path: 'catalogo/:id',
        loadComponent: () => import('./modules/artesanal/catalogo/ficha-comercial.component').then(m => m.FichaComercialComponent)
      },
      // Escena: ficha de SKU (pisa la ficha generada de Variante y la de Movimiento de stock).
      { path: 'variante/:id', loadComponent: () => import('./modules/artesanal/sku/sku.component').then(m => m.SkuComponent) },
      { path: 'movimientostock/:id', loadComponent: () => import('./modules/artesanal/sku/movimiento-redirect.component').then(m => m.MovimientoRedirectComponent) },
      // Escena: cliente 360 (pisa la ficha generada de Cliente).
      { path: 'cliente/:id', loadComponent: () => import('./modules/artesanal/cliente360/cliente360.component').then(m => m.Cliente360Component) },
      // Escena: panel del vendedor (pisa la ficha generada de Vendedor).
      { path: 'vendedor/:id', loadComponent: () => import('./modules/artesanal/vendedor-panel/vendedor-panel.component').then(m => m.VendedorPanelComponent) },
      // Escena: pedido (pisa la ficha generada de Pedido; revisión de escenas 2026-09-12).
      { path: 'pedido/:id', loadComponent: () => import('./modules/artesanal/pedido/pedido.component').then(m => m.PedidoEscenaComponent) },
      // Escena: seguimiento del envío (pisa la ficha generada de Envío).
      { path: 'envio/:id', loadComponent: () => import('./modules/artesanal/seguimiento/seguimiento-envio.component').then(m => m.SeguimientoEnvioComponent) },
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
