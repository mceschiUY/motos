/**
 * Read models del catálogo comercial (artesanal, solo lectura).
 * Filas de `GET /api/artesanal/catalogo` y `GET /api/artesanal/catalogo/{id}`.
 * Todo en USD (plan §7: no hay otra moneda en el sistema).
 */

/** Una card de la grilla del catálogo. */
export interface CatalogoItem {
  id: number;
  codigo: string | null;
  nombre: string | null;
  marcaId: number;
  marcaDisplay: string | null;
  categoriaId: number;
  categoriaDisplay: string | null;
  genero: string | null;
  destacado: boolean;
  novedad: boolean;
  /** Id en PC_DOCUMENTOS de la portada; el contenido se pide aparte, en base64. */
  imagenPrincipalId: number | null;
  /** MIN(precioLista) de las variantes activas. 0 si el producto todavía no tiene SKU. */
  precioDesdeUsd: number;
  skus: number;
  /** Unidades en stock sumando todos los depósitos. Puede ser negativa (Kardex sin entrada). */
  unidades: number;
}

/** Una celda de la matriz talla × color de la ficha comercial. */
export interface FichaProductoVariante {
  varianteId: number;
  sku: string | null;
  tallaId: number | null;
  tallaDisplay: string | null;
  tallaOrden: number;
  colorId: number | null;
  colorDisplay: string | null;
  colorHex: string | null;
  precioListaUsd: number;
  costoEstandarUsd: number;
  margenUsd: number;
  margenPorcentaje: number;
  disponible: number;
  activo: boolean;
}

/** Ficha comercial: la hoja que se le muestra (o imprime) al cliente. */
export interface FichaProducto {
  id: number;
  codigo: string | null;
  nombre: string | null;
  marcaId: number;
  marcaDisplay: string | null;
  categoriaId: number;
  categoriaDisplay: string | null;
  genero: string | null;
  temporada: string | null;
  material: string | null;
  pesoGramos: number | null;
  descripcion: string | null;
  fichaTecnica: string | null;
  tipoCasco: string | null;
  homologacion: string | null;
  homologacionVigente: boolean | null;
  fechaVencHomologacion: string | null;
  destacado: boolean;
  novedad: boolean;
  imagenPrincipalId: number | null;
  depositoId: number | null;
  depositoDisplay: string | null;
  precioDesdeUsd: number;
  unidadesTotales: number;
  margenPromedioPorcentaje: number;
  variantes: FichaProductoVariante[];
}

/** Un cambio de precio o costo, del historial de una variante. */
export interface PrecioVariante {
  id: number;
  varianteId: number;
  /** `precio` (PrecioLista) o `costo` (CostoEstandar). */
  campo: string;
  valorAnterior: number;
  valorNuevo: number;
  diferenciaUsd: number;
  diferenciaPorcentaje: number;
  fecha: string;
  usuario: string | null;
}
