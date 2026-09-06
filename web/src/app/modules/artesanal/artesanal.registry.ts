// Registry de vistas ARTESANALES (contrato 2026-08-29).
// Zona NO generada: la regeneración de Forja jamás escribe en modules/artesanal/.
// Cada vista construida por forja-vistas se registra acá (read-modify-write, como
// generated-menu.registry). Si una entrada falta o su componente no existe, el
// default de la entidad CAE SOLO a 'table' — la tabla es el paracaídas.

export interface VistaArtesanal {
  /** Entidad del universe a la que pertenece (lowercase, ej: 'edificio') */
  entidad: string;
  /** Clave única de la vista (ej: 'morosidad') — default se expresa como 'artesanal:morosidad' */
  key: string;
  /** Etiqueta para el view-toggle */
  label: string;
  /** Ícono Material */
  icon: string;
  /** Loader lazy del componente */
  loadComponent: () => Promise<any>;
}

export const VISTAS_ARTESANALES: VistaArtesanal[] = [
  // forja-vistas agrega entradas acá — NO escribir a mano
];

/** Resuelve la vista artesanal registrada para una entidad+key, o null (⇒ fallback a tabla). */
export function resolverVistaArtesanal(entidad: string, key: string): VistaArtesanal | null {
  return VISTAS_ARTESANALES.find(v => v.entidad === entidad && v.key === key) ?? null;
}

/** Todas las artesanales de una entidad (para listarlas en el view-toggle). */
export function vistasArtesanalesDe(entidad: string): VistaArtesanal[] {
  return VISTAS_ARTESANALES.filter(v => v.entidad === entidad);
}
