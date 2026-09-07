import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** Metadata de la entidad Marca (catálogo). */
export const MARCA_DESCRIPTOR: EntityDescriptor = {
  entidad: 'marca',
  label: 'Marca',
  campos: [
    { nombre: 'nombre', label: 'Nombre', tipo: 'texto', primario: true },
    { nombre: 'pais', label: 'País', tipo: 'texto' },
    { nombre: 'activo', label: 'Activo', tipo: 'bool' },
  ],
  vistas: ['table', 'cards'],
  vistaDefault: 'table',
  hijas: [],
};
