import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** GENERADO por Forja — metadata de la entidad. No editar: se regenera. */
export const CLIENTE_DESCRIPTOR: EntityDescriptor = {
  entidad: 'cliente',
  label: 'Cliente',
  campos: [
    { nombre: 'nombre', label: 'Nombre', tipo: 'texto', primario: true },
    { nombre: 'telefono', label: 'Teléfono', tipo: 'texto' },
    { nombre: 'direccionEntrega', label: 'Dirección Entrega', tipo: 'texto' },
  ],
  vistas: ['table', 'cards', 'master-detail', 'with-relations'],
  vistaDefault: 'table',
  hijas: [
    { entidad: 'Envio', fk: 'clienteId', icon: 'list_alt' },
  ],
};
