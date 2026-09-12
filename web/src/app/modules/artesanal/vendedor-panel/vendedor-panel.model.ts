/**
 * Panel del vendedor — read model artesanal (contrato con
 * `Controllers/Artesanal/VendedorArtesanalController` → `GET api/artesanal/vendedor/{id}/panel`).
 * Lo que no viene de acá sale de `AgendaService` (avance, agenda) — no se duplica.
 */
export interface PanelVendedor {
  vendedor: PanelVendedorCabecera;
  clientes: PanelVendedorCliente[];
  pedidosAbiertos: PanelVendedorPedido[];
  /** Total del mes no anulado ni entregado × % de comisión. */
  comisionProyectadaUsd: number;
  /** Días del parámetro crm.dias_sin_visita usado para el semáforo. */
  diasSinVisitaUmbral: number;
  /** Base de esa proyección (USD todavía no entregados del mes). */
  pendienteEntregaUsd: number;
  /** Siempre 8 filas, la más vieja primero; semanaInicio = lunes (ISO date). */
  ventasPorSemana: PanelVendedorSemana[];
  ranking: { posicion: number; total: number };
}

export interface PanelVendedorCabecera {
  id: number;
  nombre: string | null;
  zona: string | null;
  telefono: string | null;
  email: string | null;
  comisionPorcentaje: number;
  activo: boolean;
  usuario: string | null;
}

export type SemaforoCliente = 'verde' | 'amarillo' | 'rojo';

export interface PanelVendedorCliente {
  id: number;
  nombre: string | null;
  tipo: string | null;
  ciudad: string | null;
  diasSinVisita: number | null;
  ultimoPedidoFecha: string | null;
  pedidosAnio: number;
  totalAnioUsd: number;
  semaforo: SemaforoCliente;
}

export interface PanelVendedorPedido {
  id: number;
  numero: string | null;
  fecha: string;
  estado: string | null;
  clienteNombre: string | null;
  totalUsd: number;
}

export interface PanelVendedorSemana {
  semanaInicio: string;
  totalUsd: number;
  pedidos: number;
}
