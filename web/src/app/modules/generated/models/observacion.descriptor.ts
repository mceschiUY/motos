import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** GENERADO por Forja — metadata de la entidad. No editar: se regenera. */
export const OBSERVACION_DESCRIPTOR: EntityDescriptor = {
  entidad: 'observacion',
  label: 'Observación',
  campos: [
    { nombre: 'texto', label: 'Texto', tipo: 'texto', primario: true },
    { nombre: 'fechaHora', label: 'Fecha Hora', tipo: 'fecha' },
    { nombre: 'usuario', label: 'Usuario', tipo: 'texto' },
    { nombre: 'envioId', label: 'Envio', tipo: 'fk', display: 'envioDisplay' },
  ],
  vistas: ['table', 'cards', 'master-detail', 'timeline', 'calendario'],
  vistaDefault: 'table',
};
