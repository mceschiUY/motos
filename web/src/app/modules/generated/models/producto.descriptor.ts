import { EntityDescriptor } from '../../../core/models/entity-descriptor';

/** GENERADO por Forja — metadata de la entidad. No editar: se regenera. */
export const PRODUCTO_DESCRIPTOR: EntityDescriptor = {
  entidad: 'producto',
  label: 'Producto',
  campos: [
    { nombre: 'codigo', label: 'Código', tipo: 'texto', primario: true },
    { nombre: 'nombre', label: 'Nombre', tipo: 'texto' },
    { nombre: 'marcaId', label: 'Marca', tipo: 'fk', display: 'marcaDisplay' },
    { nombre: 'categoriaId', label: 'Categoría', tipo: 'fk', display: 'categoriaDisplay' },
    { nombre: 'genero', label: 'Género', tipo: 'enum', valores: ['unisex', 'hombre', 'mujer', 'nino'] },
    { nombre: 'temporada', label: 'Temporada', tipo: 'texto', enLista: false },
    { nombre: 'material', label: 'Material', tipo: 'texto', enLista: false },
    { nombre: 'pesoGramos', label: 'Peso (g)', tipo: 'numero', enLista: false },
    { nombre: 'tipoCasco', label: 'Tipo de casco', tipo: 'enum', valores: ['integral', 'modular', 'jet', 'cross', 'na'], enLista: false },
    { nombre: 'homologacion', label: 'Homologación', tipo: 'enum', valores: ['ece2206', 'dot', 'snell', 'na'], enLista: false },
    { nombre: 'homologacionVigente', label: 'Homologación vigente', tipo: 'bool', enLista: false },
    { nombre: 'fechaVencHomologacion', label: 'Venc. homologación', tipo: 'fecha', enLista: false },
    { nombre: 'descripcion', label: 'Descripción', tipo: 'texto', enLista: false },
    { nombre: 'fichaTecnica', label: 'Ficha técnica', tipo: 'texto', enLista: false },
    { nombre: 'destacado', label: 'Destacado', tipo: 'bool', enLista: false },
    { nombre: 'novedad', label: 'Novedad', tipo: 'bool', enLista: false },
    { nombre: 'activo', label: 'Activo', tipo: 'bool' },
  ],
  vistas: ['table', 'cards', 'master-detail', 'with-relations'],
  vistaDefault: 'table',
  hijas: [
    { entidad: 'Variante', fk: 'productoId', icon: 'qr_code_2' },
  ],
};
