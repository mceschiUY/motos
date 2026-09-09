import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** GENERADO por Forja — metadata de la entidad. No editar: se regenera. */
export const PEDIDO_DESCRIPTOR: EntityDescriptor = {
  entidad: 'pedido',
  label: 'Pedido',
  campos: [
    { nombre: 'numero', label: 'Número', tipo: 'texto', primario: true },
    { nombre: 'fecha', label: 'Fecha', tipo: 'fecha' },
    { nombre: 'clienteId', label: 'Cliente', tipo: 'fk', display: 'clienteDisplay' },
    { nombre: 'estado', label: 'Estado', tipo: 'enum',
      valores: ['borrador', 'confirmado', 'preparado', 'despachado', 'entregado', 'anulado'] },
    { nombre: 'totalUsd', label: 'Total US$', tipo: 'moneda' },
    { nombre: 'vendedorId', label: 'Vendedor', tipo: 'fk', display: 'vendedorDisplay', enLista: false },
    { nombre: 'depositoId', label: 'Depósito', tipo: 'fk', display: 'depositoDisplay', enLista: false },
    { nombre: 'agenciaId', label: 'Agencia', tipo: 'fk', display: 'agenciaDisplay', enLista: false },
    { nombre: 'comisionUsd', label: 'Comisión US$', tipo: 'moneda', enLista: false },
    { nombre: 'observaciones', label: 'Observaciones', tipo: 'texto', enLista: false },
    { nombre: 'motivoAnulacion', label: 'Motivo de anulación', tipo: 'texto', enLista: false },
  ],
  ciclo: { campo: 'estado', estados: ['borrador', 'confirmado', 'preparado', 'despachado', 'entregado', 'anulado'] },
  vistas: ['table', 'cards', 'master-detail', 'with-relations', 'timeline', 'calendario', 'kanban'],
  vistaDefault: 'table',
  acciones: [
    { key: 'pasar-a-confirmado', label: 'Confirmar', icon: 'check_circle' },
    { key: 'pasar-a-preparado', label: 'Marcar preparado', icon: 'inventory' },
    { key: 'pasar-a-despachado', label: 'Despachar', icon: 'local_shipping' },
    { key: 'pasar-a-entregado', label: 'Confirmar entrega', icon: 'task_alt' },
    { key: 'pasar-a-anulado', label: 'Anular', icon: 'cancel' },
  ],
  hijas: [
    { entidad: 'PedidoLinea', fk: 'pedidoId', icon: 'list' },
  ],
};
