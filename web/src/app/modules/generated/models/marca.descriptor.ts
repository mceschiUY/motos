import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** GENERADO por Forja — metadata de la entidad. No editar: se regenera. */
export const MARCA_DESCRIPTOR: EntityDescriptor = {
  entidad: 'marca',
  label: 'Marca',
  campos: [
    { nombre: 'nombre', label: 'Nombre', tipo: 'texto', primario: true },
    { nombre: 'pais', label: 'País', tipo: 'texto' },
    { nombre: 'activo', label: 'Activo', tipo: 'bool' },
  ],
  vistas: ['table', 'cards', 'master-detail', 'with-relations'],
  vistaDefault: 'table',
  hijas: [
    { entidad: 'Producto', fk: 'marcaId', icon: 'inventory_2' },
  ],
};
