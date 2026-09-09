import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** GENERADO por Forja — metadata de la entidad. No editar: se regenera. */
export const CLIENTE_DESCRIPTOR: EntityDescriptor = {
  entidad: 'cliente',
  label: 'Cliente',
  campos: [
    { nombre: 'nombre', label: 'Nombre', tipo: 'texto', primario: true },
    { nombre: 'tipo', label: 'Tipo', tipo: 'enum', valores: ['tienda', 'distribuidor', 'online', 'particular'] },
    { nombre: 'ciudad', label: 'Ciudad', tipo: 'texto' },
    { nombre: 'vendedorId', label: 'Vendedor', tipo: 'fk', display: 'vendedorDisplay' },
    { nombre: 'telefono', label: 'Teléfono', tipo: 'texto' },
    { nombre: 'direccionEntrega', label: 'Dirección Entrega', tipo: 'texto', enLista: false },
    { nombre: 'contacto', label: 'Contacto', tipo: 'texto', enLista: false },
    { nombre: 'email', label: 'Email', tipo: 'texto', enLista: false },
    { nombre: 'notas', label: 'Notas', tipo: 'texto', enLista: false },
    { nombre: 'latitud', label: 'Latitud', tipo: 'numero', enLista: false },
    { nombre: 'longitud', label: 'Longitud', tipo: 'numero', enLista: false },
  ],
  vistas: ['table', 'cards', 'master-detail', 'with-relations'],
  vistaDefault: 'table',
  hijas: [
    { entidad: 'Envio', fk: 'clienteId', icon: 'local_shipping' },
    { entidad: 'Actividad', fk: 'clienteId', icon: 'event_note' },
  ],
};
