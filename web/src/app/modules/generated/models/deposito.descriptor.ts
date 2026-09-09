import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** GENERADO por Forja — metadata de la entidad. No editar: se regenera. */
export const DEPOSITO_DESCRIPTOR: EntityDescriptor = {
  entidad: 'deposito',
  label: 'Depósito',
  campos: [
    { nombre: 'codigo', label: 'Código', tipo: 'texto', primario: true },
    { nombre: 'nombre', label: 'Nombre', tipo: 'texto' },
    { nombre: 'direccion', label: 'Dirección', tipo: 'texto', enLista: false },
    { nombre: 'activo', label: 'Activo', tipo: 'bool' },
  ],
  vistas: ['table', 'cards', 'master-detail', 'with-relations'],
  vistaDefault: 'table',
  hijas: [
    { entidad: 'MovimientoStock', fk: 'depositoId', icon: 'swap_vert' },
  ],
};
