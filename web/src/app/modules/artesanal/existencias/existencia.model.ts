/**
 * Existencias agrupables (plan Etapa D, escena "¿qué se está acabando?", 2026-09-12).
 * Fila de `GET /api/artesanal/existencias`: una por SKU (variante activa de producto activo),
 * aunque no tenga movimientos. `saldo` es el del depósito elegido o el total de todos.
 */
export type SemaforoStock = 'negativo' | 'sin_stock' | 'bajo' | 'ok';

export interface ExistenciaSaldoDeposito {
  depositoId: number;
  nombre: string | null;
  saldo: number;
}

export interface ExistenciaFila {
  varianteId: number;
  sku: string | null;
  productoId: number;
  productoNombre: string | null;
  productoCodigo: string | null;
  marca: string | null;
  categoriaId: number | null;
  categoria: string | null;
  /** Categoría de primer nivel: la que agrupa. */
  categoriaRaiz: string | null;
  talla: string | null;
  color: string | null;
  colorHex: string | null;
  precioListaUsd: number;
  saldo: number;
  porDeposito: ExistenciaSaldoDeposito[];
  comprometido: number;
  vendidas30d: number;
  coberturaDias: number | null;
  semaforo: SemaforoStock;
}

export interface ExistenciasDeposito { id: number; nombre: string | null; }

export interface Existencias {
  umbralStockBajo: number;
  depositoId: number | null;
  depositos: ExistenciasDeposito[];
  filas: ExistenciaFila[];
}

/** Compatibilidad con quien aún importe el tipo viejo (fila plana SKU × depósito). */
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
