import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** GENERADO por Forja — metadata de la entidad. No editar: se regenera. */
export const COLOR_DESCRIPTOR: EntityDescriptor = {
  entidad: 'color',
  label: 'Color',
  campos: [
    { nombre: 'nombre', label: 'Nombre', tipo: 'texto', primario: true },
    { nombre: 'codigoHex', label: 'Código Hex', tipo: 'texto' },
    { nombre: 'activo', label: 'Activo', tipo: 'bool' },
  ],
  vistas: ['table', 'cards', 'master-detail', 'with-relations'],
  vistaDefault: 'table',
  hijas: [
    { entidad: 'Variante', fk: 'colorId', icon: 'qr_code_2' },
  ],
};
