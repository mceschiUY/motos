// Contrato de GET api/artesanal/centro-control (CentroControlDto). Solo USD.

export type AccionTipo = 'sla' | 'despachar' | 'visita' | 'stock' | 'meta' | 'pedido_nuevo' | 'sin_visita';
export type Acento = 'danger' | 'warning' | 'primary' | 'success';

export interface CentroControlKpis {
  pedidosNuevosHoy: number;
  pedidosADespachar: number;
  visitasHoy: number;
  enviosFueraSla: number;
  enviosSlaVencidos: number;
  ventasMesUsd: number;
  ventasMesAnteriorUsd: number;
  /** null cuando el mes anterior fue 0 (no hay base de comparación). */
  variacionMesPorcentaje: number | null;
  skuStockBajo: number;
  skuStockNegativo: number;
  comisionesMesUsd: number;
}

/** Una fila del feed "qué hacer hoy": qué pasa, qué hacer y a dónde ir. */
export interface Accion {
  tipo: AccionTipo;
  acento: Acento;
  icono: string;
  titulo: string;
  detalle: string;
  /** Ruta del front, puede traer query string (ej. `/agenda?vendedorId=3`). */
  ruta: string;
  fecha: string | null;
}

export interface PipelineEstado {
  estado: string;
  cantidad: number;
  totalUsd: number;
}

export interface VendedorResumen {
  id: number;
  nombre: string;
  zona: string | null;
  vendidoMesUsd: number;
  objetivoUsd: number;
  avancePorcentaje: number;
  comisionMesUsd: number;
  visitasMes: number;
}

export interface TopProducto {
  productoId: number;
  nombre: string;
  marca: string | null;
  unidadesMes: number;
  totalUsd: number;
}

export interface StockDeposito {
  depositoId: number;
  nombre: string;
  unidades: number;
}

export interface CentroControl {
  fecha: string;
  periodo: string;
  umbralStockBajo: number;
  kpis: CentroControlKpis;
  acciones: Accion[];
  pipeline: PipelineEstado[];
  vendedores: VendedorResumen[];
  topProductos: TopProducto[];
  stockPorDeposito: StockDeposito[];
}
