import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** Metadata de la entidad Categoría (catálogo). */
export const CATEGORIA_DESCRIPTOR: EntityDescriptor = {
  entidad: 'categoria',
  label: 'Categoría',
  campos: [
    { nombre: 'nombre', label: 'Nombre', tipo: 'texto', primario: true },
    { nombre: 'activo', label: 'Activo', tipo: 'bool' },
  ],
  vistas: ['table', 'cards'],
  vistaDefault: 'table',
  hijas: [],
};
