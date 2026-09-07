import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** Metadata de la entidad Talla (catálogo). */
export const TALLA_DESCRIPTOR: EntityDescriptor = {
  entidad: 'talla',
  label: 'Talla',
  campos: [
    { nombre: 'nombre', label: 'Nombre', tipo: 'texto', primario: true },
    { nombre: 'tipo', label: 'Tipo', tipo: 'texto' },
    { nombre: 'orden', label: 'Orden', tipo: 'numero' },
    { nombre: 'activo', label: 'Activo', tipo: 'bool' },
  ],
  vistas: ['table', 'cards'],
  vistaDefault: 'table',
  hijas: [],
};
