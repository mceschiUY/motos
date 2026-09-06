export interface Observacion {
  id: number;
  texto: string;
  fechaHora: Date | string;
  usuario: string;
  envioId: number;
  envioDisplay?: string;
}
