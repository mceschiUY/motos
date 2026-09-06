export interface Envio {
  id: number;
  codigoRastreo: string;
  estado: string;
  fechaRecibido: Date | string;
  fechaFactura: Date | string;
  fechaEnvio: Date | string;
  fechaEntrega: Date | string;
  motivoAnulacion: string;
  clienteId: number;
  clienteDisplay?: string;
  agenciaId: number;
  agenciaDisplay?: string;
}
