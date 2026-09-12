/** Contrato de GET /api/artesanal/cliente/{id}/360 (Cliente360Dto del backend). */
export interface Cliente360 {
  // Cabecera
  id: number;
  nombre: string | null;
  tipo: string | null;
  ciudad: string | null;
  direccionEntrega: string | null;
  contacto: string | null;
  telefono: string | null;
  email: string | null;
  notas: string | null;
  latitud: number | null;
  longitud: number | null;
  vendedorId: number | null;
  vendedorNombre: string | null;
  vendedorTelefono: string | null;

  // Salud de la relación
  diasSinVisita: number | null;
  ultimaActividadFecha: string | null;
  ultimaActividadTipo: string | null;
  ultimaActividadResultado: string | null;
  proximaAccion: string | null;
  pedidosAnio: number;
  totalAnioUsd: number;
  ticketPromedioUsd: number;
  ultimoPedido: Cliente360Pedido | null;
  pedidosAbiertos: number;
  enviosEnCurso: number;
  semaforo: 'verde' | 'amarillo' | 'rojo';
  motivoSemaforo: string;
  /** Días del parámetro crm.dias_sin_visita usado para el semáforo. */
  diasSinVisitaUmbral: number;

  timeline: Cliente360TimelineItem[];
  topProductos: Cliente360TopProducto[];
}

export interface Cliente360Pedido {
  id: number;
  numero: string | null;
  fecha: string;
  estado: string | null;
  totalUsd: number;
}

export interface Cliente360TimelineItem {
  tipo: 'actividad' | 'pedido' | 'envio';
  id: number;
  fecha: string;
  titulo: string;
  detalle: string | null;
  /** Valor crudo del enum, para la pill. */
  estado: string | null;
  ruta: string;
}

export interface Cliente360TopProducto {
  productoId: number;
  productoNombre: string | null;
  unidades: number;
  totalUsd: number;
}
