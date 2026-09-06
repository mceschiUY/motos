/**
 * Descriptor de entidad (A1.5 — multi-vista genérica): la METADATA que antes estaba
 * disuelta en ~800 líneas de HTML estampado por entidad. El generador emite UN objeto
 * de estos por entidad; los componentes genéricos del core (entity-table, y después
 * kanban/cards/…) lo interpretan. Sin tipos por entidad: los items viajan como any.
 */

/** Tipo FUNCIONAL de un campo: decide render, alineación y formato. */
export type TipoCampo =
  | 'texto'
  | 'numero'
  | 'moneda'
  | 'fecha'
  | 'bool'
  | 'enum'
  | 'fk';

export interface CampoDescriptor {
  /** Nombre camelCase de la propiedad en el item (ej. "fechaCierre"). */
  nombre: string;
  /** Etiqueta humana (ej. "Fecha de cierre"). */
  label: string;
  tipo: TipoCampo;
  /** fk: propiedad display resuelta por el backend (ej. "clienteDisplay"). */
  display?: string;
  /** enum: valores posibles (para chips/filtros futuros). */
  valores?: string[];
  /** Columna con jerarquía visual (col-primary). Default: el primer campo. */
  primario?: boolean;
  /** Aparece como columna de la tabla (el resto queda para ficha/form). */
  enLista?: boolean;
}

export interface CicloDescriptor {
  /** Propiedad camelCase del estado (ej. "estado"). */
  campo: string;
  /** Estados en orden de ciclo (columnas del kanban, paradas del stepper). */
  estados: string[];
}

export interface AccionCustomDescriptor {
  /** Key kebab de la capability (la ruta HTTP: "pasar-a-aprobada"). */
  key: string;
  label: string;
  icon: string;
}

export interface RelacionHijaDescriptor {
  /** Entidad hija (Pascal, ej. "Oportunidad"). */
  entidad: string;
  /** FK camelCase en la hija (ej. "clienteId"). */
  fk: string;
  icon: string;
}

export interface EntityDescriptor {
  /** Nombre lower/normalizado (rutas, ej. "oportunidad"). */
  entidad: string;
  /** Etiqueta humana singular (ej. "Oportunidad"). */
  label: string;
  campos: CampoDescriptor[];
  ciclo?: CicloDescriptor;
  /** Vistas habilitadas ("table", "kanban", "cards", …). */
  vistas: string[];
  vistaDefault: string;
  acciones?: AccionCustomDescriptor[];
  hijas?: RelacionHijaDescriptor[];
}

/** Campos que se muestran como columnas (enLista !== false, sin ids técnicos), tope 5. */
export function columnasDe(d: EntityDescriptor): CampoDescriptor[] {
  return d.campos
    .filter(c => c.enLista !== false && c.nombre.toLowerCase() !== 'id')
    .slice(0, 5);
}
