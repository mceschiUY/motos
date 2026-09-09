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

// Grupos del menu generados por constelacion
// Se agregaran automaticamente cuando se generen componentes con BigBang
export const GENERATED_MENU_GROUPS: GeneratedMenuGroup[] = [
  // Los grupos de constelaciones se agregaran aqui automaticamente
  {
    label: 'Catálogo',
    icon: 'inventory_2',
    items: [
      { path: 'marca', label: 'Marca', icon: 'sell', supportsDetail: true, baseRoute: '/marca' },
      { path: 'categoria', label: 'Categoría', icon: 'category', supportsDetail: true, baseRoute: '/categoria' },
      { path: 'talla', label: 'Talla', icon: 'straighten', supportsDetail: true, baseRoute: '/talla' },
      { path: 'color', label: 'Color', icon: 'palette', supportsDetail: true, baseRoute: '/color' },
      { path: 'producto', label: 'Producto', icon: 'inventory_2', supportsDetail: true, baseRoute: '/producto' },
      { path: 'variante', label: 'Variante / SKU', icon: 'qr_code_2', supportsDetail: true, baseRoute: '/variante' },
    ]
  },
  {
    label: 'Inventario',
    icon: 'warehouse',
    items: [
      { path: 'deposito', label: 'Depósito', icon: 'warehouse', supportsDetail: true, baseRoute: '/deposito' },
      { path: 'movimientostock', label: 'Movimiento de stock', icon: 'swap_vert', supportsDetail: true, baseRoute: '/movimientostock' },
      { path: 'existencias', label: 'Existencias', icon: 'inventory', supportsDetail: false, baseRoute: '/existencias' },
    ]
  },
  {
    // Comercial: fuerza de ventas (Etapa A). Agenda es artesanal, como Existencias.
    label: 'Comercial',
    icon: 'storefront',
    items: [
      { path: 'cliente', label: 'Cliente', icon: 'person', supportsDetail: true, baseRoute: '/cliente' },
      { path: 'vendedor', label: 'Vendedor', icon: 'badge', supportsDetail: true, baseRoute: '/vendedor' },
      { path: 'actividad', label: 'Actividad', icon: 'event_note', supportsDetail: true, baseRoute: '/actividad' },
      { path: 'pedido', label: 'Pedido', icon: 'receipt_long', supportsDetail: true, baseRoute: '/pedido' },
      { path: 'meta', label: 'Meta', icon: 'flag', supportsDetail: true, baseRoute: '/meta' },
      { path: 'agenda', label: 'Agenda', icon: 'event', supportsDetail: false, baseRoute: '/agenda' },
      { path: 'comisiones', label: 'Comisiones', icon: 'payments', supportsDetail: false, baseRoute: '/comisiones' },
    ]
  },
  {
    // Operaciones: todo el ciclo del envío (Etapa 0 fundió "Gestión de agencias" y "General").
    label: 'Operaciones',
    icon: 'route',
    items: [
      { path: 'envio', label: 'Envío', icon: 'local_shipping', supportsDetail: true, baseRoute: '/envio' },
      { path: 'agencia', label: 'Agencia', icon: 'business', supportsDetail: true, baseRoute: '/agencia' },
      { path: 'observacion', label: 'Observación', icon: 'comment', supportsDetail: true, baseRoute: '/observacion' },
      { path: 'parametrosla', label: 'Parámetro SLA', icon: 'timer', supportsDetail: true, baseRoute: '/parametrosla' },
    ]
  },
];

// Item fijo de Inicio
const HOME_ITEM: GeneratedMenuItem = { path: '', label: 'Inicio', icon: 'home', supportsDetail: false, baseRoute: '/' };

// Lista plana de items (para compatibilidad y busquedas)
export const GENERATED_MENU_ITEMS: GeneratedMenuItem[] = [
  HOME_ITEM,
  ...GENERATED_MENU_GROUPS.flatMap(g => g.items),
];
