export interface Producto {
  id: number;
  codigo: string;
  nombre: string;
  marcaId: number;
  marcaDisplay?: string | null;
  categoriaId: number;
  categoriaDisplay?: string | null;
  descripcion?: string | null;
  genero: string;
  temporada?: string | null;
  material?: string | null;
  pesoGramos?: number | null;
  tipoCasco?: string | null;
  homologacion?: string | null;
  homologacionVigente?: boolean | null;
  fechaVencHomologacion?: string | null;
  activo: boolean;
  /** Etapa C — catálogo premium (plan §3.7). Aparece primero en la grilla del catálogo. */
  destacado?: boolean | null;
  /** Etiqueta "Nuevo" en la card del catálogo. */
  novedad?: boolean | null;
  /** Markdown simple: peso, materiales, certificaciones, talle recomendado. */
  fichaTecnica?: string | null;
  /** Id en PC_DOCUMENTOS de la foto de portada (se elige desde la galería de la ficha). */
  imagenPrincipalId?: number | null;
}
