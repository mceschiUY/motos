// Interfaz para los items del menu
export interface GeneratedMenuItem {
  path: string;
  label: string;
  icon: string;
  /** Indica si la entidad soporta navegacion con ID (vista detalle) */
  supportsDetail?: boolean;
  /** Ruta base completa para navegacion programatica */
  baseRoute?: string;
}

// Interfaz para agrupar items del menu por constelacion
export interface GeneratedMenuGroup {
  label: string;
  icon: string;
  items: GeneratedMenuItem[];
}

// Menú de RELATO (plan "escenas, no tablas", 2026-09-12): los grupos siguen el día de la
// distribuidora (catálogo → vender → despachar → equipo) y las escenas artesanales van
// primero. Los CRUD generados que no son escena quedan plegados en "Administración": siguen
// existiendo, solo dejan de ser el destino de la navegación. Las rutas de detalle
// (cliente/:id, vendedor/:id, variante/:id, envio/:id, movimientostock/:id) las resuelven
// las escenas de app.routes.ts, que van antes de GENERATED_ROUTES.
export const GENERATED_MENU_GROUPS: GeneratedMenuGroup[] = [
  {
    label: 'Catálogo',
    icon: 'storefront',
    items: [
      { path: 'catalogo', label: 'Catálogo', icon: 'storefront', supportsDetail: false, baseRoute: '/catalogo' },
      { path: 'existencias', label: 'Existencias', icon: 'inventory', supportsDetail: false, baseRoute: '/existencias' },
    ]
  },
  {
    label: 'Vender',
    icon: 'handshake',
    items: [
      { path: 'agenda', label: 'Agenda', icon: 'event', supportsDetail: false, baseRoute: '/agenda' },
      { path: 'pedidos/nuevo', label: 'Nuevo pedido', icon: 'add_shopping_cart', supportsDetail: false, baseRoute: '/pedidos/nuevo' },
      { path: 'pedido', label: 'Pedidos', icon: 'receipt_long', supportsDetail: true, baseRoute: '/pedido' },
      { path: 'cliente', label: 'Clientes', icon: 'person', supportsDetail: true, baseRoute: '/cliente' },
    ]
  },
  {
    label: 'Despachar',
    icon: 'local_shipping',
    items: [
      { path: 'envio', label: 'Envíos', icon: 'local_shipping', supportsDetail: true, baseRoute: '/envio' },
    ]
  },
  {
    label: 'Equipo',
    icon: 'groups',
    items: [
      { path: 'vendedor', label: 'Vendedores', icon: 'badge', supportsDetail: true, baseRoute: '/vendedor' },
      { path: 'actividad', label: 'Actividades', icon: 'event_note', supportsDetail: true, baseRoute: '/actividad' },
      { path: 'comisiones', label: 'Comisiones', icon: 'payments', supportsDetail: false, baseRoute: '/comisiones' },
    ]
  },
  {
    // Maestros y tablas de soporte: se llega acá solo para cargar datos.
    label: 'Administración',
    icon: 'tune',
    items: [
      { path: 'producto', label: 'Productos', icon: 'inventory_2', supportsDetail: true, baseRoute: '/producto' },
      { path: 'variante', label: 'Variantes / SKU', icon: 'qr_code_2', supportsDetail: true, baseRoute: '/variante' },
      { path: 'marca', label: 'Marcas', icon: 'sell', supportsDetail: true, baseRoute: '/marca' },
      { path: 'categoria', label: 'Categorías', icon: 'category', supportsDetail: true, baseRoute: '/categoria' },
      { path: 'talla', label: 'Tallas', icon: 'straighten', supportsDetail: true, baseRoute: '/talla' },
      { path: 'color', label: 'Colores', icon: 'palette', supportsDetail: true, baseRoute: '/color' },
      { path: 'deposito', label: 'Depósitos', icon: 'warehouse', supportsDetail: true, baseRoute: '/deposito' },
      { path: 'movimientostock', label: 'Movimientos de stock', icon: 'swap_vert', supportsDetail: true, baseRoute: '/movimientostock' },
      { path: 'agencia', label: 'Agencias', icon: 'business', supportsDetail: true, baseRoute: '/agencia' },
      { path: 'parametrosla', label: 'Parámetros SLA', icon: 'timer', supportsDetail: true, baseRoute: '/parametrosla' },
      { path: 'meta', label: 'Metas', icon: 'flag', supportsDetail: true, baseRoute: '/meta' },
      { path: 'observacion', label: 'Observaciones', icon: 'comment', supportsDetail: true, baseRoute: '/observacion' },
    ]
  },
];

// Item fijo de Inicio
const HOME_ITEM: GeneratedMenuItem = { path: '', label: 'Hoy', icon: 'home', supportsDetail: false, baseRoute: '/' };

// Lista plana de items (para compatibilidad y busquedas)
export const GENERATED_MENU_ITEMS: GeneratedMenuItem[] = [
  HOME_ITEM,
  ...GENERATED_MENU_GROUPS.flatMap(g => g.items),
];
