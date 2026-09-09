export interface PedidoLinea {
  id: number;
  pedidoId: number;
  pedidoDisplay?: string;
  varianteId: number;
  varianteDisplay?: string;
  productoDisplay?: string;
  tallaDisplay?: string;
  colorDisplay?: string;
  cantidad: number;
  /** Copia del precio de lista al armar el pedido. En 0 el backend usa el de la variante. */
  precioUnitarioUsd: number;
  subtotalUsd: number;
}
