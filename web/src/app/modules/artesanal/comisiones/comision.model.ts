/** Fila de la liquidación: lo que le corresponde a un vendedor en un mes (plan §4.4). */
export interface ComisionVendedor {
  vendedorId: number;
  vendedorDisplay: string | null;
  zona: string | null;
  comisionPorcentaje: number;
  periodo: string;
  /** Pedidos ENTREGADOS en el período: la comisión se devenga con la entrega. */
  pedidos: number;
  totalUsd: number;
  comisionUsd: number;
}
