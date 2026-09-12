import { Pipe, PipeTransform } from '@angular/core';

/**
 * Diccionario ÚNICO de etiquetas de los enums del dominio para las escenas artesanales
 * (revisión de escenas 2026-09-12): estados de pedido y envío, tipos y resultados de
 * actividad, tipos de movimiento de stock, semáforos, tipos de cliente. Antes cada escena
 * traducía a su manera ("Pedido" vs "Con pedido") o imprimía el valor crudo ("sin_pedido").
 * Los valores son los que guarda la base (minúsculas, con guion bajo); no se tocan.
 */
export const ETIQUETAS: Record<string, string> = {
  // Pedido (ciclos-vida.json)
  borrador: 'Borrador',
  confirmado: 'Confirmado',
  preparado: 'Preparado',
  despachado: 'Despachado',
  entregado: 'Entregado',
  anulado: 'Anulado',
  // Envío: "facturado" se lee "Confirmado" (Etapa 0 del plan)
  recibido: 'Recibido',
  facturado: 'Confirmado',
  // Etapas SLA
  facturacion: 'Confirmación',
  despacho: 'Despacho',
  entrega: 'Entrega',
  // Actividad: tipo
  visita: 'Visita',
  llamada: 'Llamada',
  whatsapp: 'WhatsApp',
  email: 'Email',
  // Actividad: resultado
  pedido: 'Con pedido',
  sin_pedido: 'Sin pedido',
  reprogramar: 'Reprogramar',
  sin_contacto: 'Sin contacto',
  // Movimiento de stock
  entrada: 'Entrada',
  salida: 'Salida',
  ajuste: 'Ajuste',
  transferencia: 'Transferencia',
  // Semáforo de stock (Existencias, ficha de SKU, catálogo)
  negativo: 'Negativo',
  sin_stock: 'Sin stock',
  bajo: 'Stock bajo',
  ok: 'OK',
  // Semáforo comercial (cliente, meta)
  verde: 'Al día',
  amarillo: 'Atención',
  rojo: 'Urgente',
  // SLA
  advertencia: 'Advertencia',
  vencido: 'Vencido',
  // Cliente: tipo
  tienda: 'Tienda',
  distribuidor: 'Distribuidor',
  online: 'Online',
  particular: 'Particular',
  // Producto
  unisex: 'Unisex',
  hombre: 'Hombre',
  mujer: 'Mujer',
  nino: 'Niño',
  integral: 'Integral',
  modular: 'Modular',
  jet: 'Jet',
  cross: 'Cross',
  na: 'No aplica',
  ece2206: 'ECE 22.06',
  dot: 'DOT',
  snell: 'Snell',
  // Tipos de acción del feed de Hoy
  sla: 'SLA',
  despachar: 'Despachar',
  stock: 'Stock',
  meta: 'Meta',
  pedido_nuevo: 'Pedido nuevo',
  sin_visita: 'Sin visita',
};

/** Etiqueta legible de un valor de enum; si no está en el diccionario, capitaliza y saca guiones. */
export function etiqueta(valor: string | null | undefined): string {
  if (valor == null || valor === '') return '';
  const v = String(valor);
  const conocida = ETIQUETAS[v] ?? ETIQUETAS[v.toLowerCase()];
  if (conocida) return conocida;
  const limpio = v.replace(/[_-]+/g, ' ');
  return limpio.charAt(0).toUpperCase() + limpio.slice(1);
}

/** `{{ pedido.estado | etiqueta }}` → "Confirmado". */
@Pipe({ name: 'etiqueta', standalone: true })
export class EtiquetaPipe implements PipeTransform {
  transform(valor: string | null | undefined): string { return etiqueta(valor); }
}
