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
  { name: 'marca', path: 'marca', listComponent: 'MarcaListComponent', formComponent: 'MarcaFormComponent', detailComponent: 'MarcaDetailComponent', icon: 'sell', label: 'Marca' },
  { name: 'categoria', path: 'categoria', listComponent: 'CategoriaListComponent', formComponent: 'CategoriaFormComponent', detailComponent: 'CategoriaDetailComponent', icon: 'category', label: 'Categoria' },
  { name: 'talla', path: 'talla', listComponent: 'TallaListComponent', formComponent: 'TallaFormComponent', detailComponent: 'TallaDetailComponent', icon: 'straighten', label: 'Talla' },
  { name: 'color', path: 'color', listComponent: 'ColorListComponent', formComponent: 'ColorFormComponent', detailComponent: 'ColorDetailComponent', icon: 'palette', label: 'Color' },
  { name: 'producto', path: 'producto', listComponent: 'ProductoListComponent', formComponent: 'ProductoFormComponent', detailComponent: 'ProductoDetailComponent', icon: 'inventory_2', label: 'Producto' },
  { name: 'variante', path: 'variante', listComponent: 'VarianteListComponent', formComponent: 'VarianteFormComponent', detailComponent: 'VarianteDetailComponent', icon: 'qr_code_2', label: 'Variante' },
  { name: 'deposito', path: 'deposito', listComponent: 'DepositoListComponent', formComponent: 'DepositoFormComponent', detailComponent: 'DepositoDetailComponent', icon: 'warehouse', label: 'Deposito' },
  { name: 'movimientostock', path: 'movimientostock', listComponent: 'MovimientoStockListComponent', formComponent: 'MovimientoStockFormComponent', detailComponent: 'MovimientoStockDetailComponent', icon: 'swap_vert', label: 'Movimiento de stock' },
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
    name: 'cliente',
    path: 'cliente',
    listComponent: 'ClienteListComponent',
    formComponent: 'ClienteFormComponent',
    detailComponent: 'ClienteDetailComponent',
    icon: 'person',
    label: 'Cliente'
  },
  {
    name: 'vendedor',
    path: 'vendedor',
    listComponent: 'VendedorListComponent',
    formComponent: 'VendedorFormComponent',
    detailComponent: 'VendedorDetailComponent',
    icon: 'badge',
    label: 'Vendedor'
  },
  {
    name: 'actividad',
    path: 'actividad',
    listComponent: 'ActividadListComponent',
    formComponent: 'ActividadFormComponent',
    detailComponent: 'ActividadDetailComponent',
    icon: 'event_note',
    label: 'Actividad'
  },
  {
    name: 'meta',
    path: 'meta',
    listComponent: 'MetaListComponent',
    formComponent: 'MetaFormComponent',
    detailComponent: 'MetaDetailComponent',
    icon: 'flag',
    label: 'Meta'
  },
  {
    name: 'pedido',
    path: 'pedido',
    listComponent: 'PedidoListComponent',
    formComponent: 'PedidoFormComponent',
    detailComponent: 'PedidoDetailComponent',
    icon: 'receipt_long',
    label: 'Pedido'
  },
  {
    name: 'pedidolinea',
    path: 'pedidolinea',
    listComponent: 'PedidoLineaListComponent',
    formComponent: 'PedidoLineaFormComponent',
    detailComponent: 'PedidoLineaDetailComponent',
    icon: 'list',
    label: 'Línea de pedido'
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
  { path: 'marca', loadComponent: () => import('./components/marca/marca-list/marca-list.component').then(m => m.MarcaListComponent) },
  { path: 'marca/:id', loadComponent: () => import('./components/marca/marca-ficha/marca-ficha.component').then(m => m.MarcaFichaComponent) },
  { path: 'categoria', loadComponent: () => import('./components/categoria/categoria-list/categoria-list.component').then(m => m.CategoriaListComponent) },
  { path: 'categoria/:id', loadComponent: () => import('./components/categoria/categoria-ficha/categoria-ficha.component').then(m => m.CategoriaFichaComponent) },
  { path: 'talla', loadComponent: () => import('./components/talla/talla-list/talla-list.component').then(m => m.TallaListComponent) },
  { path: 'talla/:id', loadComponent: () => import('./components/talla/talla-ficha/talla-ficha.component').then(m => m.TallaFichaComponent) },
  { path: 'color', loadComponent: () => import('./components/color/color-list/color-list.component').then(m => m.ColorListComponent) },
  { path: 'color/:id', loadComponent: () => import('./components/color/color-ficha/color-ficha.component').then(m => m.ColorFichaComponent) },
  { path: 'producto', loadComponent: () => import('./components/producto/producto-list/producto-list.component').then(m => m.ProductoListComponent) },
  { path: 'producto/:id', loadComponent: () => import('./components/producto/producto-ficha/producto-ficha.component').then(m => m.ProductoFichaComponent) },
  { path: 'variante', loadComponent: () => import('./components/variante/variante-list/variante-list.component').then(m => m.VarianteListComponent) },
  { path: 'variante/:id', loadComponent: () => import('./components/variante/variante-ficha/variante-ficha.component').then(m => m.VarianteFichaComponent) },
  { path: 'deposito', loadComponent: () => import('./components/deposito/deposito-list/deposito-list.component').then(m => m.DepositoListComponent) },
  { path: 'deposito/:id', loadComponent: () => import('./components/deposito/deposito-ficha/deposito-ficha.component').then(m => m.DepositoFichaComponent) },
  { path: 'movimientostock', loadComponent: () => import('./components/movimientostock/movimientostock-list/movimientostock-list.component').then(m => m.MovimientoStockListComponent) },
  { path: 'movimientostock/:id', loadComponent: () => import('./components/movimientostock/movimientostock-ficha/movimientostock-ficha.component').then(m => m.MovimientoStockFichaComponent) },
  {
    path: 'documento',
    loadComponent: () => import('./components/documento/documento-list/documento-list.component').then(m => m.DocumentoListComponent)
  },

  {
    path: 'cliente',
    loadComponent: () => import('./components/cliente/cliente-list/cliente-list.component').then(m => m.ClienteListComponent)
  },
  {
    path: 'cliente/:id',
    loadComponent: () => import('./components/cliente/cliente-ficha/cliente-ficha.component').then(m => m.ClienteFichaComponent)
  },
  // Ruta de navegación: Cliente filtrado por Vendedor
  {
    path: 'cliente/by-vendedor/:vendedorId',
    loadComponent: () => import('./components/cliente/cliente-list/cliente-list.component').then(m => m.ClienteListComponent)
  },
  {
    path: 'vendedor',
    loadComponent: () => import('./components/vendedor/vendedor-list/vendedor-list.component').then(m => m.VendedorListComponent)
  },
  {
    path: 'vendedor/:id',
    loadComponent: () => import('./components/vendedor/vendedor-ficha/vendedor-ficha.component').then(m => m.VendedorFichaComponent)
  },
  {
    path: 'actividad',
    loadComponent: () => import('./components/actividad/actividad-list/actividad-list.component').then(m => m.ActividadListComponent)
  },
  {
    path: 'actividad/:id',
    loadComponent: () => import('./components/actividad/actividad-ficha/actividad-ficha.component').then(m => m.ActividadFichaComponent)
  },
  // Ruta de navegación: Actividad filtrada por Cliente
  {
    path: 'actividad/by-cliente/:clienteId',
    loadComponent: () => import('./components/actividad/actividad-list/actividad-list.component').then(m => m.ActividadListComponent)
  },
  // Ruta de navegación: Actividad filtrada por Vendedor
  {
    path: 'actividad/by-vendedor/:vendedorId',
    loadComponent: () => import('./components/actividad/actividad-list/actividad-list.component').then(m => m.ActividadListComponent)
  },
  {
    path: 'meta',
    loadComponent: () => import('./components/meta/meta-list/meta-list.component').then(m => m.MetaListComponent)
  },
  {
    path: 'meta/:id',
    loadComponent: () => import('./components/meta/meta-ficha/meta-ficha.component').then(m => m.MetaFichaComponent)
  },
  // Ruta de navegación: Meta filtrada por Vendedor
  {
    path: 'meta/by-vendedor/:vendedorId',
    loadComponent: () => import('./components/meta/meta-list/meta-list.component').then(m => m.MetaListComponent)
  },
  {
    path: 'pedido',
    loadComponent: () => import('./components/pedido/pedido-list/pedido-list.component').then(m => m.PedidoListComponent)
  },
  {
    path: 'pedido/:id',
    loadComponent: () => import('./components/pedido/pedido-ficha/pedido-ficha.component').then(m => m.PedidoFichaComponent)
  },
  // Ruta de navegación: Pedido filtrado por Cliente
  {
    path: 'pedido/by-cliente/:clienteId',
    loadComponent: () => import('./components/pedido/pedido-list/pedido-list.component').then(m => m.PedidoListComponent)
  },
  // Ruta de navegación: Pedido filtrado por Vendedor
  {
    path: 'pedido/by-vendedor/:vendedorId',
    loadComponent: () => import('./components/pedido/pedido-list/pedido-list.component').then(m => m.PedidoListComponent)
  },
  {
    path: 'pedidolinea',
    loadComponent: () => import('./components/pedidolinea/pedidolinea-list/pedidolinea-list.component').then(m => m.PedidoLineaListComponent)
  },
  {
    path: 'pedidolinea/:id',
    loadComponent: () => import('./components/pedidolinea/pedidolinea-ficha/pedidolinea-ficha.component').then(m => m.PedidoLineaFichaComponent)
  },
  // Ruta de navegación: Línea filtrada por Pedido
  {
    path: 'pedidolinea/by-pedido/:pedidoId',
    loadComponent: () => import('./components/pedidolinea/pedidolinea-list/pedidolinea-list.component').then(m => m.PedidoLineaListComponent)
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
  // Ruta de navegación: Producto filtrado por Marca
  {
    path: 'producto/by-marca/:marcaId',
    loadComponent: () => import('./components/producto/producto-list/producto-list.component').then(m => m.ProductoListComponent)
  },
  // Ruta de navegación: Producto filtrado por Categoria
  {
    path: 'producto/by-categoria/:categoriaId',
    loadComponent: () => import('./components/producto/producto-list/producto-list.component').then(m => m.ProductoListComponent)
  },
  // Ruta de navegación: Variante filtrado por Producto
  {
    path: 'variante/by-producto/:productoId',
    loadComponent: () => import('./components/variante/variante-list/variante-list.component').then(m => m.VarianteListComponent)
  },
  // Ruta de navegación: Variante filtrado por Talla
  {
    path: 'variante/by-talla/:tallaId',
    loadComponent: () => import('./components/variante/variante-list/variante-list.component').then(m => m.VarianteListComponent)
  },
  // Ruta de navegación: Variante filtrado por Color
  {
    path: 'variante/by-color/:colorId',
    loadComponent: () => import('./components/variante/variante-list/variante-list.component').then(m => m.VarianteListComponent)
  },
  // Ruta de navegación: MovimientoStock filtrado por Deposito
  {
    path: 'movimientostock/by-deposito/:depositoId',
    loadComponent: () => import('./components/movimientostock/movimientostock-list/movimientostock-list.component').then(m => m.MovimientoStockListComponent)
  },
  // Ruta de navegación: MovimientoStock filtrado por Variante
  {
    path: 'movimientostock/by-variante/:varianteId',
    loadComponent: () => import('./components/movimientostock/movimientostock-list/movimientostock-list.component').then(m => m.MovimientoStockListComponent)
  },
  {
    path: 'parametrosla',
    loadComponent: () => import('./components/parametrosla/parametrosla-list/parametrosla-list.component').then(m => m.ParametroslaListComponent)
  },
  {
    path: 'parametrosla/:id',
    loadComponent: () => import('./components/parametrosla/parametrosla-ficha/parametrosla-ficha.component').then(m => m.ParametroslaFichaComponent)
  },];
