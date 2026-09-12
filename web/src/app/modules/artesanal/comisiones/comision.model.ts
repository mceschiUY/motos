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

  // ─── Etapa H.6: meta, proyección y mes anterior (mismo corte, calculado en el backend) ───
  /** Meta del período (0 si no hay). */
  objetivoUsd: number;
  /** Pedidos del período todavía no entregados ni anulados. */
  pedidosPendientes: number;
  pendienteEntregaUsd: number;
  /** Sellada + pendiente × % de hoy: lo que cobraría si entrega todo lo abierto del mes. */
  comisionProyectadaUsd: number;
  periodoAnterior: string;
  anteriorPedidos: number;
  anteriorTotalUsd: number;
  anteriorComisionUsd: number;
  anteriorObjetivoUsd: number;
}
