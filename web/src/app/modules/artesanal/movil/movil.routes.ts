import { Routes } from '@angular/router';

/**
 * Rutas hijas de la app del vendedor (`/m`, plan Etapa F4). Reutilizan las escenas artesanales
 * del sitio; las que faltaban para el celular (mis clientes, mis pedidos) viven en esta carpeta.
 * Cualquier ruta desconocida cae a Mi día.
 */
export const MOVIL_ROUTES: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'agenda' },
  { path: 'agenda', loadComponent: () => import('../agenda/agenda.component').then(m => m.AgendaComponent) },
  { path: 'clientes', loadComponent: () => import('./movil-clientes.component').then(m => m.MovilClientesComponent) },
  { path: 'cliente', pathMatch: 'full', redirectTo: 'clientes' },
  { path: 'cliente/:id', loadComponent: () => import('../cliente360/cliente360.component').then(m => m.Cliente360Component) },
  { path: 'catalogo', loadComponent: () => import('../catalogo/catalogo.component').then(m => m.CatalogoComponent) },
  { path: 'catalogo/:id', loadComponent: () => import('../catalogo/ficha-comercial.component').then(m => m.FichaComercialComponent) },
  { path: 'pedidos/nuevo', loadComponent: () => import('../armado-pedido/armado-pedido.component').then(m => m.ArmadoPedidoComponent) },
  { path: 'pedidos', loadComponent: () => import('./movil-pedidos.component').then(m => m.MovilPedidosComponent) },
  { path: 'pedido', pathMatch: 'full', redirectTo: 'pedidos' },
  { path: 'pedido/:id', loadComponent: () => import('../pedido/pedido.component').then(m => m.PedidoEscenaComponent) },
  { path: 'envio/:id', loadComponent: () => import('../seguimiento/seguimiento-envio.component').then(m => m.SeguimientoEnvioComponent) },
  { path: 'variante/:id', loadComponent: () => import('../sku/sku.component').then(m => m.SkuComponent) },
  { path: 'vendedor/:id', loadComponent: () => import('../vendedor-panel/vendedor-panel.component').then(m => m.VendedorPanelComponent) },
  { path: 'actividad/:id', loadComponent: () => import('../../generated/components/actividad/actividad-ficha/actividad-ficha.component').then(m => m.ActividadFichaComponent) },
  { path: '**', redirectTo: 'agenda' },
];
