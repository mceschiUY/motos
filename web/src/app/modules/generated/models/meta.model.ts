export interface Meta {
  id: number;
  vendedorId: number;
  vendedorDisplay?: string;
  /** Período mensual 'YYYY-MM'. */
  periodo: string;
  objetivoUsd: number;
}
