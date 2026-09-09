import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** GENERADO por Forja — metadata de la entidad. No editar: se regenera. */
export const PEDIDOLINEA_DESCRIPTOR: EntityDescriptor = {
  entidad: 'pedidolinea',
  label: 'Línea de pedido',
  campos: [
    { nombre: 'varianteId', label: 'SKU', tipo: 'fk', display: 'varianteDisplay', primario: true },
    { nombre: 'productoDisplay', label: 'Producto', tipo: 'texto' },
    { nombre: 'cantidad', label: 'Cantidad', tipo: 'numero' },
    { nombre: 'precioUnitarioUsd', label: 'Precio US$', tipo: 'moneda' },
    { nombre: 'subtotalUsd', label: 'Subtotal US$', tipo: 'moneda' },
    { nombre: 'pedidoId', label: 'Pedido', tipo: 'fk', display: 'pedidoDisplay', enLista: false },
  ],
  // Sin ciclo: la línea no tiene estados propios, vive el del pedido.
  vistas: ['table', 'cards', 'master-detail'],
  vistaDefault: 'table',
};
