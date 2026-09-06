// GENERADO POR FORJA — manifiesto de las entidades del producto.
// Lo consumen la búsqueda global (Ctrl+K), el pulso, el modo pantalla y el ASISTENTE DE VOZ
// (los campos le dicen qué preguntar en un alta y cómo llenar cada control).
export interface CampoForm {
  nombre: string;
  label: string;
  tipo: 'texto' | 'numero' | 'fecha' | 'bool' | 'enum' | 'ref';
  requerido: boolean;
  valores?: string[];
  ref?: string;
}

export interface BusquedaEntidad {
  api: string;
  ruta: string;
  label: string;
  icon: string;
  campoTitulo: string;
  conEstado: boolean;
  campos: CampoForm[];
}

export const BUSQUEDA_ENTIDADES: BusquedaEntidad[] = [
  { api: 'agencia', ruta: '/agencia', label: 'Agencia', icon: 'list_alt', campoTitulo: 'nombre', conEstado: false,
    campos: [{ nombre: 'nombre', label: 'Nombre', tipo: 'texto', requerido: true }] },
  { api: 'cliente', ruta: '/cliente', label: 'Cliente', icon: 'person', campoTitulo: 'nombre', conEstado: false,
    campos: [{ nombre: 'nombre', label: 'Nombre', tipo: 'texto', requerido: true }, { nombre: 'telefono', label: 'Teléfono', tipo: 'texto', requerido: false }, { nombre: 'direccionEntrega', label: 'Dirección Entrega', tipo: 'texto', requerido: false }] },
  { api: 'envio', ruta: '/envio', label: 'Envio', icon: 'list_alt', campoTitulo: 'codigoRastreo', conEstado: true,
    campos: [{ nombre: 'codigoRastreo', label: 'Código Rastreo', tipo: 'texto', requerido: true }, { nombre: 'estado', label: 'Estado', tipo: 'enum', requerido: true, valores: ['recibido', 'facturado', 'despachado', 'entregado', 'anulado'] }, { nombre: 'fechaRecibido', label: 'Fecha Recibido', tipo: 'fecha', requerido: true }, { nombre: 'fechaFactura', label: 'Fecha Factura', tipo: 'fecha', requerido: false }, { nombre: 'fechaEnvio', label: 'Fecha Envio', tipo: 'fecha', requerido: false }, { nombre: 'fechaEntrega', label: 'Fecha Entrega', tipo: 'fecha', requerido: false }, { nombre: 'motivoAnulacion', label: 'Motivo Anulacion', tipo: 'texto', requerido: false }, { nombre: 'clienteId', label: 'Cliente', tipo: 'ref', requerido: true, ref: 'cliente' }, { nombre: 'agenciaId', label: 'Agencia', tipo: 'ref', requerido: true, ref: 'agencia' }] },
  { api: 'observacion', ruta: '/observacion', label: 'Observación', icon: 'list_alt', campoTitulo: 'texto', conEstado: false,
    campos: [{ nombre: 'texto', label: 'Texto', tipo: 'texto', requerido: true }, { nombre: 'fechaHora', label: 'Fecha Hora', tipo: 'fecha', requerido: true }, { nombre: 'usuario', label: 'Usuario', tipo: 'texto', requerido: true }, { nombre: 'envioId', label: 'Envio', tipo: 'ref', requerido: true, ref: 'envio' }] },
  { api: 'parametrosla', ruta: '/parametrosla', label: 'Parametro SLA', icon: 'list_alt', campoTitulo: 'etapa', conEstado: true,
    campos: [{ nombre: 'etapa', label: 'Etapa', tipo: 'enum', requerido: true, valores: ['facturacion', 'despacho', 'entrega'] }, { nombre: 'rangoAlertaUmbralAdvertenciaDias', label: 'Rango Alerta Umbral Advertencia Dias', tipo: 'numero', requerido: true }, { nombre: 'rangoAlertaLimiteDias', label: 'Rango Alerta Limite Dias', tipo: 'numero', requerido: true }] }
];
