import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** GENERADO por Forja — metadata de la entidad. No editar: se regenera. */
export const AGENCIA_DESCRIPTOR: EntityDescriptor = {
  entidad: 'agencia',
  label: 'Agencia',
  campos: [
    { nombre: 'nombre', label: 'Nombre', tipo: 'texto', primario: true },
  ],
  vistas: ['table', 'cards', 'master-detail', 'with-relations'],
  vistaDefault: 'table',
  hijas: [
    { entidad: 'Envio', fk: 'agenciaId', icon: 'list_alt' },
  ],
};
