export interface Actividad {
  id: number;
  vendedorId: number;
  vendedorDisplay?: string;
  clienteId: number;
  clienteDisplay?: string;
  /** visita | llamada | whatsapp | email */
  tipo: string;
  fecha: Date | string;
  /** pedido | sin_pedido | reprogramar | sin_contacto */
  resultado: string;
  notas?: string | null;
  proximaAccion?: Date | string | null;
  pedidoId?: number | null;
}
