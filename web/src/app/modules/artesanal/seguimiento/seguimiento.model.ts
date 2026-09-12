/**
 * Contrato del seguimiento del envío (Application/Artesanal/Logistica/SeguimientoEnvio).
 * Los campos marcados "solo interno" viajan en null cuando `publico` es true.
 */
export type EstadoSla = 'ok' | 'advertencia' | 'vencido';
export type ClaveEtapa = 'recibido' | 'confirmado' | 'despachado' | 'entregado';

export interface SeguimientoEtapa {
  clave: ClaveEtapa;
  label: string;
  /** Fecha del hito; null si todavía no se alcanzó. */
  fecha: string | null;
  cumplida: boolean;
  /** Hito en curso (si el envío sigue vivo) o último alcanzado (entregado). */
  actual: boolean;
  /** En curso: días desde el hito anterior hasta hoy. Cumplida: días que tardó. */
  diasTranscurridos: number | null;
  umbralDias: number | null;
  limiteDias: number | null;
  estadoSla: EstadoSla | null;
}

export interface SeguimientoLinea {
  varianteId: number;
  sku: string | null;
  productoNombre: string | null;
  talla: string | null;
  color: string | null;
  cantidad: number;
  /** solo interno */
  precioUnitarioUsd: number | null;
  /** solo interno */
  subtotalUsd: number | null;
}

export interface SeguimientoPedido {
  id: number;
  numero: string | null;
  fecha: string;
  estado: string | null;
  vendedorNombre: string | null;
  /** solo interno */
  totalUsd: number | null;
  lineas: SeguimientoLinea[];
}

export interface SeguimientoObservacion {
  id: number;
  fechaHora: string;
  /** solo interno */
  usuario: string | null;
  texto: string | null;
}

export interface SeguimientoEnvio {
  envioId: number;
  codigoRastreo: string;
  estado: string;
  estadoLabel: string;
  motivoAnulacion: string | null;
  publico: boolean;
  cliente: {
    id: number;
    nombre: string | null;
    ciudad: string | null;
    direccionEntrega: string | null;
    /** solo interno */
    telefono: string | null;
  };
  agencia: { id: number; nombre: string | null };
  etapas: SeguimientoEtapa[];
  slaGlobal: EstadoSla | null;
  pedido: SeguimientoPedido | null;
  observaciones: SeguimientoObservacion[];
}
