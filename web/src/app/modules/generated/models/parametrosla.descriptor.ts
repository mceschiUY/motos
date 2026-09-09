import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** GENERADO por Forja — metadata de la entidad. No editar: se regenera. */
export const PARAMETROSLA_DESCRIPTOR: EntityDescriptor = {
  entidad: 'parametrosla',
  label: 'Parametro SLA',
  campos: [
    { nombre: 'etapa', label: 'Etapa', tipo: 'enum', valores: ['facturacion', 'despacho', 'entrega'], etiquetas: { facturacion: 'Confirmación' }, primario: true },
    { nombre: 'rangoAlertaUmbralAdvertenciaDias', label: 'Rango Alerta Umbral Advertencia Dias', tipo: 'numero' },
    { nombre: 'rangoAlertaLimiteDias', label: 'Rango Alerta Limite Dias', tipo: 'numero' },
  ],
  ciclo: { campo: 'etapa', estados: ['facturacion', 'despacho', 'entrega'] },
  vistas: ['table', 'cards', 'master-detail', 'kanban'],
  vistaDefault: 'table',
};
