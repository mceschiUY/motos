export interface Categoria {
  id: number;
  nombre: string;
  categoriaPadreId?: number | null;
  activo: boolean;
}
