import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** GENERADO por Forja — metadata de la entidad. No editar: se regenera. */
export const TALLA_DESCRIPTOR: EntityDescriptor = {
  entidad: 'talla',
  label: 'Talla',
  campos: [
    { nombre: 'nombre', label: 'Nombre', tipo: 'texto', primario: true },
    { nombre: 'tipo', label: 'Tipo', tipo: 'texto' },
    { nombre: 'orden', label: 'Orden', tipo: 'numero' },
    { nombre: 'activo', label: 'Activo', tipo: 'bool' },
  ],
  vistas: ['table', 'cards', 'master-detail', 'with-relations'],
  vistaDefault: 'table',
  hijas: [
    { entidad: 'Variante', fk: 'tallaId', icon: 'qr_code_2' },
  ],
};
