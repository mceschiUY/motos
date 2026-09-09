import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** GENERADO por Forja — metadata de la entidad. No editar: se regenera. */
export const MOVIMIENTOSTOCK_DESCRIPTOR: EntityDescriptor = {
  entidad: 'movimientostock',
  label: 'Movimiento de stock',
  campos: [
    { nombre: 'fecha', label: 'Fecha', tipo: 'fecha', primario: true },
    { nombre: 'tipo', label: 'Tipo', tipo: 'enum', valores: ['entrada', 'salida', 'ajuste', 'transferencia'] },
    { nombre: 'varianteId', label: 'SKU', tipo: 'fk', display: 'varianteDisplay' },
    { nombre: 'depositoId', label: 'Depósito', tipo: 'fk', display: 'depositoDisplay' },
    { nombre: 'depositoDestinoId', label: 'Depósito destino', tipo: 'fk', display: 'depositoDestinoDisplay', enLista: false },
    { nombre: 'cantidad', label: 'Cantidad', tipo: 'numero' },
    { nombre: 'costoUnitario', label: 'Costo unit.', tipo: 'moneda', enLista: false },
    { nombre: 'documentoOrigen', label: 'Documento', tipo: 'texto', enLista: false },
    { nombre: 'motivo', label: 'Motivo', tipo: 'texto', enLista: false },
    { nombre: 'usuario', label: 'Usuario', tipo: 'texto', enLista: false },
  ],
  vistas: ['table', 'cards', 'master-detail', 'timeline', 'calendario'],
  vistaDefault: 'table',
  hijas: [],
};
