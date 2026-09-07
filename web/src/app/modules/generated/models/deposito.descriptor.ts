import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** Metadata de la entidad Depósito (stock). */
export const DEPOSITO_DESCRIPTOR: EntityDescriptor = {
  entidad: 'deposito',
  label: 'Depósito',
  campos: [
    { nombre: 'codigo', label: 'Código', tipo: 'texto', primario: true },
    { nombre: 'nombre', label: 'Nombre', tipo: 'texto' },
    { nombre: 'direccion', label: 'Dirección', tipo: 'texto', enLista: false },
    { nombre: 'activo', label: 'Activo', tipo: 'bool' },
  ],
  vistas: ['table', 'cards'],
  vistaDefault: 'table',
  hijas: [],
};
