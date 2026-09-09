import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** GENERADO por Forja — metadata de la entidad. No editar: se regenera. */
export const ENVIO_DESCRIPTOR: EntityDescriptor = {
  entidad: 'envio',
  label: 'Envio',
  campos: [
    { nombre: 'codigoRastreo', label: 'Código Rastreo', tipo: 'texto', primario: true },
    // Etapa 0 (2026-09-07): sin facturación en el horizonte, 'facturado' se LEE como "Confirmado". El valor persistido no cambia.
    { nombre: 'estado', label: 'Estado', tipo: 'enum', valores: ['recibido', 'facturado', 'despachado', 'entregado', 'anulado'], etiquetas: { facturado: 'Confirmado' } },
    { nombre: 'fechaRecibido', label: 'Fecha Recibido', tipo: 'fecha' },
    { nombre: 'fechaFactura', label: 'Fecha Factura', tipo: 'fecha' },
    { nombre: 'fechaEnvio', label: 'Fecha Envio', tipo: 'fecha' },
    { nombre: 'fechaEntrega', label: 'Fecha Entrega', tipo: 'fecha', enLista: false },
    { nombre: 'motivoAnulacion', label: 'Motivo Anulacion', tipo: 'texto', enLista: false },
    { nombre: 'clienteId', label: 'Cliente', tipo: 'fk', display: 'clienteDisplay', enLista: false },
    { nombre: 'agenciaId', label: 'Agencia', tipo: 'fk', display: 'agenciaDisplay', enLista: false },
  ],
  ciclo: { campo: 'estado', estados: ['recibido', 'facturado', 'despachado', 'entregado', 'anulado'] },
  vistas: ['table', 'cards', 'master-detail', 'with-relations', 'timeline', 'calendario', 'kanban'],
  vistaDefault: 'table',
  acciones: [
    { key: 'pasar-a-facturado', label: 'Confirmar Envio', icon: 'play_arrow' },
    { key: 'pasar-a-despachado', label: 'Pasar a despachado Envio', icon: 'play_arrow' },
    { key: 'pasar-a-entregado', label: 'Pasar a entregado Envio', icon: 'play_arrow' },
    { key: 'pasar-a-anulado', label: 'Pasar a anulado Envio', icon: 'play_arrow' },
  ],
  hijas: [
    { entidad: 'Observacion', fk: 'envioId', icon: 'list_alt' },
  ],
};
