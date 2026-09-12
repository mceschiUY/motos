/**
 * Agenda del vendedor — read models artesanales (contrato con `Controllers/Artesanal`).
 * Ver doc/plan.md §4.2 y §4.8.
 */
export interface ParadaAgenda {
  /** Fecha de la parada: ProximaAccion (planificada) o Fecha (realizada). ISO date. */
  fecha: string;
  tipo: 'planificada' | 'realizada';
  actividadId: number;
  clienteId: number;
  clienteDisplay: string | null;
  /** Contexto de la parada (revisión de escenas 2026-09-12). */
  telefono: string | null;
  diasSinVisita: number | null;
  ultimoPedidoFecha: string | null;
  ultimoPedidoTotalUsd: number | null;
  pedidosAbiertos: number;
  ciudad: string | null;
  latitud: number | null;
  longitud: number | null;
  vendedorId: number;
  vendedorDisplay: string | null;
  tipoActividad: string;
  resultado: string;
  notas: string | null;
}

export interface AvanceVendedor {
  vendedorId: number;
  periodo: string;
  objetivoUsd: number;
  vendidoUsd: number;
  /** Comisión sellada en el período (suma de ComisionUsd de los pedidos entregados). */
  comisionUsd: number;
  actividades: number;
  visitas: number;
  conPedido: number;
  tasaCierre: number;
  clientesAsignados: number;
  clientesSinVisitar30d: number;
  /** Días del parámetro crm.dias_sin_visita con el que se contó (para las etiquetas). */
  diasSinVisitaUmbral: number;
}

export interface ClienteSinVisitar {
  clienteId: number;
  clienteDisplay: string | null;
  ciudad: string | null;
  vendedorId: number | null;
  vendedorDisplay: string | null;
  ultimaActividad: string | null;
  diasSinVisita: number;
}
