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
  actividades: number;
  visitas: number;
  conPedido: number;
  tasaCierre: number;
  clientesAsignados: number;
  clientesSinVisitar30d: number;
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
