export interface Variante {
  id: number;
  productoId: number;
  productoDisplay?: string | null;
  tallaId?: number | null;
  tallaDisplay?: string | null;
  colorId?: number | null;
  colorDisplay?: string | null;
  sku: string;
  codigoBarras?: string | null;
  costoEstandar: number;
  precioLista: number;
  activo: boolean;
}
