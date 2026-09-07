import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** GENERADO por Forja — metadata de la entidad. No editar: se regenera. */
export const COLOR_DESCRIPTOR: EntityDescriptor = {
  entidad: 'color',
  label: 'Color',
  campos: [
    { nombre: 'nombre', label: 'Nombre', tipo: 'texto', primario: true },
  ],
  vistas: ['table', 'cards', 'master-detail', 'with-relations'],
  vistaDefault: 'table',
  hijas: [
    { entidad: 'Envio', fk: 'colorId', icon: 'list_alt' },
  ],
};
