/** Escena "Pedido" (revisión de escenas 2026-09-12): `GET /api/artesanal/pedido/{id}/ficha`. */
export interface FichaPedidoLinea {
  id: number;
  varianteId: number;
  sku: string | null;
  productoId: number;
  productoNombre: string | null;
  marca: string | null;
  imagenPrincipalId: number | null;
  talla: string | null;
  color: string | null;
  colorHex: string | null;
  cantidad: number;
  precioUnitarioUsd: number;
  subtotalUsd: number;
  precioListaUsd: number;
  /** Saldo en el depósito del pedido; null cuando el pedido ya salió (no aplica). */
  disponibleDeposito: number | null;
}

export interface FichaPedido {
  id: number;
  numero: string | null;
  fecha: string;
  estado: string;
  totalUsd: number;
  comisionUsd: number;
  comisionPorcentaje: number;
  observaciones: string | null;
  motivoAnulacion: string | null;
  clienteId: number;
  clienteNombre: string | null;
  clienteTipo: string | null;
  clienteCiudad: string | null;
  clienteDireccion: string | null;
  clienteTelefono: string | null;
  vendedorId: number;
  vendedorNombre: string | null;
  depositoId: number;
  depositoNombre: string | null;
  agenciaId: number | null;
  agenciaNombre: string | null;
  envioId: number | null;
  envioCodigo: string | null;
  envioEstado: string | null;
  envioFechaEnvio: string | null;
  envioFechaEntrega: string | null;
  lineas: FichaPedidoLinea[];
  unidades: number;
  lineasConFaltante: number;
}
