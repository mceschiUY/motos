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
