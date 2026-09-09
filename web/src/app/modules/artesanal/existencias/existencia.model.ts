/**
 * Existencia — read model de stock (artesanal, solo lectura).
 * Fila de `GET /api/MovimientoStock/existencias`: saldo por SKU/depósito calculado
 * sumando el Kardex. No tiene `id`: la clave natural es (varianteId, depositoId).
 * Los saldos en cero no vienen (HAVING <> 0); `disponible` puede ser negativo.
 */
export interface Existencia {
  varianteId: number;
  sku: string | null;
  productoDisplay: string | null;
  depositoId: number;
  depositoDisplay: string | null;
  disponible: number;
}

export function claveExistencia(e: Existencia): string {
  return `${e.varianteId}-${e.depositoId}`;
}
