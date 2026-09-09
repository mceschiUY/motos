import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** GENERADO por Forja — metadata de la entidad. No editar: se regenera. */
export const VENDEDOR_DESCRIPTOR: EntityDescriptor = {
  entidad: 'vendedor',
  label: 'Vendedor',
  campos: [
    { nombre: 'nombre', label: 'Nombre', tipo: 'texto', primario: true },
    { nombre: 'zona', label: 'Zona', tipo: 'texto' },
    { nombre: 'comisionPorcentaje', label: 'Comisión %', tipo: 'numero' },
    { nombre: 'activo', label: 'Activo', tipo: 'bool' },
    { nombre: 'telefono', label: 'Teléfono', tipo: 'texto', enLista: false },
    { nombre: 'email', label: 'Email', tipo: 'texto', enLista: false },
    { nombre: 'usuario', label: 'Usuario del sitio', tipo: 'texto', enLista: false },
  ],
  vistas: ['table', 'cards', 'master-detail', 'with-relations'],
  vistaDefault: 'table',
  hijas: [
    { entidad: 'Cliente', fk: 'vendedorId', icon: 'person' },
    { entidad: 'Actividad', fk: 'vendedorId', icon: 'event_note' },
    { entidad: 'Meta', fk: 'vendedorId', icon: 'flag' },
  ],
};
