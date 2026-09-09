export interface Pedido {
  id: number;
  /** PED-000n. Lo sella el backend al crear: el form no lo manda. */
  numero: string;
  fecha: Date | string;
  clienteId: number;
  clienteDisplay?: string;
  vendedorId: number;
  vendedorDisplay?: string;
  depositoId: number;
  depositoDisplay?: string;
  /** Agencia con la que se despacha; el envío nace con ella. */
  agenciaId?: number | null;
  agenciaDisplay?: string;
  /** borrador | confirmado | preparado | despachado | entregado | anulado */
  estado: string;
  totalUsd: number;
  comisionUsd: number;
  observaciones?: string | null;
  envioId?: number | null;
  envioDisplay?: string;
  motivoAnulacion?: string | null;
  /** Cuántas líneas tiene (lo calcula la consulta). */
  lineas?: number;
}
