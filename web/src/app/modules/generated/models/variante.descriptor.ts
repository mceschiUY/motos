import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** GENERADO por Forja — metadata de la entidad. No editar: se regenera. */
export const VARIANTE_DESCRIPTOR: EntityDescriptor = {
  entidad: 'variante',
  label: 'Variante',
  campos: [
    { nombre: 'sku', label: 'SKU', tipo: 'texto', primario: true },
    { nombre: 'productoId', label: 'Producto', tipo: 'fk', display: 'productoDisplay' },
    { nombre: 'tallaId', label: 'Talla', tipo: 'fk', display: 'tallaDisplay' },
    { nombre: 'colorId', label: 'Color', tipo: 'fk', display: 'colorDisplay' },
    { nombre: 'codigoBarras', label: 'Cód. barras', tipo: 'texto', enLista: false },
    { nombre: 'costoEstandar', label: 'Costo', tipo: 'moneda', enLista: false },
    { nombre: 'precioLista', label: 'Precio', tipo: 'moneda' },
    { nombre: 'activo', label: 'Activo', tipo: 'bool' },
  ],
  vistas: ['table', 'cards', 'master-detail', 'with-relations'],
  vistaDefault: 'table',
  hijas: [
    { entidad: 'MovimientoStock', fk: 'varianteId', icon: 'swap_vert' },
  ],
};
