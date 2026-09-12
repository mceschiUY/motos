/**
 * Read model de la ficha de SKU (artesanal, solo lectura).
 * Fila de `GET /api/artesanal/sku/{id}` (SkuArtesanalController → FichaSkuQuery).
 * Todo en USD (plan §7: no hay otra moneda en el sistema).
 */

export type SemaforoStock = 'ok' | 'bajo' | 'sin_stock' | 'negativo';

export interface FichaSkuDeposito {
  depositoId: number;
  depositoNombre: string | null;
  saldo: number;
}

/** Un movimiento del Kardex del SKU, con el saldo total corrido después de aplicarlo. */
export interface FichaSkuMovimiento {
  movimientoId: number;
  fecha: string;
  /** entrada | salida | ajuste | transferencia */
  tipo: string | null;
  cantidad: number;
  /** Efecto sobre el stock total, con signo (transferencia entre depósitos = 0). */
  delta: number;
  saldoAcumulado: number;
  depositoId: number;
  depositoNombre: string | null;
  depositoDestinoId: number | null;
  depositoDestinoNombre: string | null;
  documentoOrigen: string | null;
  /** Pedido cuyo número coincide con el documento de origen, si lo hay. */
  pedidoId: number | null;
  motivo: string | null;
  costoUnitario: number | null;
  usuario: string | null;
}

/** Un pedido abierto (ni entregado ni anulado) que contiene el SKU. */
export interface FichaSkuPedido {
  pedidoId: number;
  numero: string | null;
  estado: string | null;
  clienteNombre: string | null;
  cantidad: number;
  fecha: string;
}

export interface FichaSku {
  varianteId: number;
  sku: string | null;
  codigoBarras: string | null;
  activo: boolean;
  productoId: number;
  productoNombre: string | null;
  productoCodigo: string | null;
  marcaDisplay: string | null;
  categoriaDisplay: string | null;
  /** Id en PC_DOCUMENTOS de la portada del producto; el contenido se pide aparte, en base64. */
  imagenPrincipalId: number | null;
  tallaDisplay: string | null;
  colorDisplay: string | null;
  colorHex: string | null;
  precioListaUsd: number;
  costoEstandarUsd: number;
  margenUsd: number;
  margenPorcentaje: number;

  stockPorDeposito: FichaSkuDeposito[];
  stockTotal: number;
  /** Unidades en pedidos borrador/confirmado/preparado (todavía no salieron del Kardex). */
  comprometido: number;
  disponibleNeto: number;
  unidadesVendidas30d: number;
  /** Días de stock al ritmo de los últimos 30 días; null si no hubo ventas. */
  coberturaDias: number | null;
  semaforoStock: SemaforoStock;

  /** Del más nuevo al más viejo. */
  kardex: FichaSkuMovimiento[];
  pedidosAbiertos: FichaSkuPedido[];
  /** Todas las variantes del mismo producto (esta incluida), ordenadas por talla y color. */
  hermanos: FichaSkuHermano[];
}

/** Un SKU del mismo producto: talla, color y cuánto hay. */
export interface FichaSkuHermano {
  varianteId: number;
  sku: string | null;
  activo: boolean;
  esActual: boolean;
  tallaId: number | null;
  tallaDisplay: string | null;
  tallaOrden: number;
  colorId: number | null;
  colorDisplay: string | null;
  colorHex: string | null;
  precioListaUsd: number;
  saldo: number;
  comprometido: number;
  semaforo: SemaforoStock;
}
