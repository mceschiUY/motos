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
    ]
  },
  {
    label: 'Gestion de clientes',
    icon: 'folder',
    items: [
        { path: 'cliente', label: 'Cliente', icon: 'person', supportsDetail: true, baseRoute: '/cliente' },
      { path: 'envio', label: 'Envio', icon: 'list_alt', supportsDetail: true, baseRoute: '/envio' },
  ]
  },
  {
    label: 'Gestion de agencias',
    icon: 'folder',
    items: [
        { path: 'agencia', label: 'Agencia', icon: 'list_alt', supportsDetail: true, baseRoute: '/agencia' },
  ]
  },
  {
    label: 'Operaciones y flujo de estados del envio',
    icon: 'folder',
    items: [
        { path: 'observacion', label: 'Observación', icon: 'list_alt', supportsDetail: true, baseRoute: '/observacion' },
  ]
  },
  {
    label: 'General',
    icon: 'folder',
    items: [
        { path: 'parametrosla', label: 'Parametro SLA', icon: 'list_alt', supportsDetail: true, baseRoute: '/parametrosla' },
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
