export interface MovimientoStock {
  id: number;
  varianteId: number;
  varianteDisplay?: string | null;
  depositoId: number;
  depositoDisplay?: string | null;
  depositoDestinoId?: number | null;
  depositoDestinoDisplay?: string | null;
  tipo: string;
  cantidad: number;
  costoUnitario?: number | null;
  motivo?: string | null;
  documentoOrigen?: string | null;
  fecha: string;
  usuario?: string | null;
}
