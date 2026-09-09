import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** GENERADO por Forja — metadata de la entidad. No editar: se regenera. */
export const ACTIVIDAD_DESCRIPTOR: EntityDescriptor = {
  entidad: 'actividad',
  label: 'Actividad',
  campos: [
    { nombre: 'fecha', label: 'Fecha', tipo: 'fecha', primario: true },
    { nombre: 'clienteId', label: 'Cliente', tipo: 'fk', display: 'clienteDisplay' },
    { nombre: 'tipo', label: 'Tipo', tipo: 'enum', valores: ['visita', 'llamada', 'whatsapp', 'email'] },
    { nombre: 'resultado', label: 'Resultado', tipo: 'enum', valores: ['pedido', 'sin_pedido', 'reprogramar', 'sin_contacto'],
      etiquetas: { pedido: 'Pedido', sin_pedido: 'Sin pedido', reprogramar: 'Reprogramar', sin_contacto: 'Sin contacto' } },
    { nombre: 'proximaAccion', label: 'Próxima acción', tipo: 'fecha' },
    { nombre: 'vendedorId', label: 'Vendedor', tipo: 'fk', display: 'vendedorDisplay', enLista: false },
    { nombre: 'notas', label: 'Notas', tipo: 'texto', enLista: false },
    { nombre: 'pedidoId', label: 'Pedido', tipo: 'numero', enLista: false },
  ],
  // Sin `ciclo`: el resultado es el desenlace del contacto, no un estado de vida → sin kanban.
  vistas: ['table', 'cards', 'master-detail', 'timeline', 'calendario'],
  vistaDefault: 'table',
};
