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
}
