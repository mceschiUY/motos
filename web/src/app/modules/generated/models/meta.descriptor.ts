import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** GENERADO por Forja — metadata de la entidad. No editar: se regenera. */
export const META_DESCRIPTOR: EntityDescriptor = {
  entidad: 'meta',
  label: 'Meta',
  campos: [
    { nombre: 'periodo', label: 'Período', tipo: 'texto', primario: true },
    { nombre: 'vendedorId', label: 'Vendedor', tipo: 'fk', display: 'vendedorDisplay' },
    { nombre: 'objetivoUsd', label: 'Objetivo US$', tipo: 'moneda' },
  ],
  vistas: ['table', 'cards', 'master-detail'],
  vistaDefault: 'table',
};
