export interface Vendedor {
  id: number;
  nombre: string;
  telefono?: string | null;
  email?: string | null;
  zona?: string | null;
  comisionPorcentaje: number;
  /** Login del usuario del sitio asociado (para "lo mío"). */
  usuario?: string | null;
  activo: boolean;
}
