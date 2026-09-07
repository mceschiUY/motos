// GENERADO POR FORJA — manifiesto de las entidades del producto.
// Lo consumen la búsqueda global (Ctrl+K), el pulso, el modo pantalla y el ASISTENTE DE VOZ
// (los campos le dicen qué preguntar en un alta y cómo llenar cada control).
export interface CampoForm {
  nombre: string;
  label: string;
  tipo: 'texto' | 'numero' | 'fecha' | 'bool' | 'enum' | 'ref';
  requerido: boolean;
  valores?: string[];
  ref?: string;
}

export interface BusquedaEntidad {
  api: string;
  ruta: string;
  label: string;
  icon: string;
  campoTitulo: string;
  conEstado: boolean;
  campos: CampoForm[];
}

export const BUSQUEDA_ENTIDADES: BusquedaEntidad[] = [
  { api: 'marca', ruta: '/marca', label: 'Marca', icon: 'sell', campoTitulo: 'nombre', conEstado: false,
    campos: [{ nombre: 'nombre', label: 'Nombre', tipo: 'texto', requerido: true }, { nombre: 'pais', label: 'País', tipo: 'texto', requerido: false }, { nombre: 'activo', label: 'Activo', tipo: 'bool', requerido: false }] },
  { api: 'categoria', ruta: '/categoria', label: 'Categoría', icon: 'category', campoTitulo: 'nombre', conEstado: false,
    campos: [{ nombre: 'nombre', label: 'Nombre', tipo: 'texto', requerido: true }, { nombre: 'categoriaPadreId', label: 'Categoría padre', tipo: 'numero', requerido: false }, { nombre: 'activo', label: 'Activo', tipo: 'bool', requerido: false }] },
  { api: 'talla', ruta: '/talla', label: 'Talla', icon: 'straighten', campoTitulo: 'nombre', conEstado: false,
    campos: [{ nombre: 'nombre', label: 'Nombre', tipo: 'texto', requerido: true }, { nombre: 'tipo', label: 'Tipo', tipo: 'texto', requerido: false }, { nombre: 'orden', label: 'Orden', tipo: 'numero', requerido: false }, { nombre: 'activo', label: 'Activo', tipo: 'bool', requerido: false }] },
  { api: 'color', ruta: '/color', label: 'Color', icon: 'palette', campoTitulo: 'nombre', conEstado: false,
    campos: [{ nombre: 'nombre', label: 'Nombre', tipo: 'texto', requerido: true }, { nombre: 'codigoHex', label: 'Código Hex', tipo: 'texto', requerido: false }, { nombre: 'activo', label: 'Activo', tipo: 'bool', requerido: false }] },
  { api: 'producto', ruta: '/producto', label: 'Producto', icon: 'inventory_2', campoTitulo: 'nombre', conEstado: false,
    campos: [{ nombre: 'codigo', label: 'Código', tipo: 'texto', requerido: true }, { nombre: 'nombre', label: 'Nombre', tipo: 'texto', requerido: true }, { nombre: 'marcaId', label: 'Marca', tipo: 'ref', requerido: true, ref: 'marca' }, { nombre: 'categoriaId', label: 'Categoría', tipo: 'ref', requerido: true, ref: 'categoria' }, { nombre: 'genero', label: 'Género', tipo: 'enum', requerido: true, valores: ['unisex', 'hombre', 'mujer', 'nino'] }, { nombre: 'temporada', label: 'Temporada', tipo: 'texto', requerido: false }, { nombre: 'material', label: 'Material', tipo: 'texto', requerido: false }, { nombre: 'pesoGramos', label: 'Peso (g)', tipo: 'numero', requerido: false }, { nombre: 'tipoCasco', label: 'Tipo de casco', tipo: 'enum', requerido: false, valores: ['integral', 'modular', 'jet', 'cross', 'na'] }, { nombre: 'homologacion', label: 'Homologación', tipo: 'enum', requerido: false, valores: ['ece2206', 'dot', 'snell', 'na'] }, { nombre: 'activo', label: 'Activo', tipo: 'bool', requerido: false }] },
  { api: 'variante', ruta: '/variante', label: 'Variante', icon: 'qr_code_2', campoTitulo: 'sku', conEstado: false,
    campos: [{ nombre: 'productoId', label: 'Producto', tipo: 'ref', requerido: true, ref: 'producto' }, { nombre: 'tallaId', label: 'Talla', tipo: 'ref', requerido: false, ref: 'talla' }, { nombre: 'colorId', label: 'Color', tipo: 'ref', requerido: false, ref: 'color' }, { nombre: 'sku', label: 'SKU', tipo: 'texto', requerido: true }, { nombre: 'codigoBarras', label: 'Código de barras', tipo: 'texto', requerido: false }, { nombre: 'costoEstandar', label: 'Costo', tipo: 'numero', requerido: false }, { nombre: 'precioLista', label: 'Precio', tipo: 'numero', requerido: false }, { nombre: 'activo', label: 'Activo', tipo: 'bool', requerido: false }] },
  { api: 'deposito', ruta: '/deposito', label: 'Depósito', icon: 'warehouse', campoTitulo: 'nombre', conEstado: false,
    campos: [{ nombre: 'codigo', label: 'Código', tipo: 'texto', requerido: true }, { nombre: 'nombre', label: 'Nombre', tipo: 'texto', requerido: true }, { nombre: 'direccion', label: 'Dirección', tipo: 'texto', requerido: false }, { nombre: 'activo', label: 'Activo', tipo: 'bool', requerido: false }] },
  { api: 'movimientostock', ruta: '/movimientostock', label: 'Movimiento de stock', icon: 'swap_vert', campoTitulo: 'documentoOrigen', conEstado: false,
    campos: [{ nombre: 'tipo', label: 'Tipo', tipo: 'enum', requerido: true, valores: ['entrada', 'salida', 'ajuste', 'transferencia'] }, { nombre: 'varianteId', label: 'SKU', tipo: 'ref', requerido: true, ref: 'variante' }, { nombre: 'depositoId', label: 'Depósito', tipo: 'ref', requerido: true, ref: 'deposito' }, { nombre: 'depositoDestinoId', label: 'Depósito destino', tipo: 'ref', requerido: false, ref: 'deposito' }, { nombre: 'cantidad', label: 'Cantidad', tipo: 'numero', requerido: true }, { nombre: 'costoUnitario', label: 'Costo unitario', tipo: 'numero', requerido: false }, { nombre: 'documentoOrigen', label: 'Documento', tipo: 'texto', requerido: false }, { nombre: 'motivo', label: 'Motivo', tipo: 'texto', requerido: false }] },
  { api: 'agencia', ruta: '/agencia', label: 'Agencia', icon: 'list_alt', campoTitulo: 'nombre', conEstado: false,
    campos: [{ nombre: 'nombre', label: 'Nombre', tipo: 'texto', requerido: true }] },
  { api: 'cliente', ruta: '/cliente', label: 'Cliente', icon: 'person', campoTitulo: 'nombre', conEstado: false,
    campos: [{ nombre: 'nombre', label: 'Nombre', tipo: 'texto', requerido: true }, { nombre: 'telefono', label: 'Teléfono', tipo: 'texto', requerido: false }, { nombre: 'direccionEntrega', label: 'Dirección Entrega', tipo: 'texto', requerido: false }] },
  { api: 'envio', ruta: '/envio', label: 'Envio', icon: 'list_alt', campoTitulo: 'codigoRastreo', conEstado: true,
    campos: [{ nombre: 'codigoRastreo', label: 'Código Rastreo', tipo: 'texto', requerido: true }, { nombre: 'estado', label: 'Estado', tipo: 'enum', requerido: true, valores: ['recibido', 'facturado', 'despachado', 'entregado', 'anulado'] }, { nombre: 'fechaRecibido', label: 'Fecha Recibido', tipo: 'fecha', requerido: true }, { nombre: 'fechaFactura', label: 'Fecha Factura', tipo: 'fecha', requerido: false }, { nombre: 'fechaEnvio', label: 'Fecha Envio', tipo: 'fecha', requerido: false }, { nombre: 'fechaEntrega', label: 'Fecha Entrega', tipo: 'fecha', requerido: false }, { nombre: 'motivoAnulacion', label: 'Motivo Anulacion', tipo: 'texto', requerido: false }, { nombre: 'clienteId', label: 'Cliente', tipo: 'ref', requerido: true, ref: 'cliente' }, { nombre: 'agenciaId', label: 'Agencia', tipo: 'ref', requerido: true, ref: 'agencia' }] },
  { api: 'observacion', ruta: '/observacion', label: 'Observación', icon: 'list_alt', campoTitulo: 'texto', conEstado: false,
    campos: [{ nombre: 'texto', label: 'Texto', tipo: 'texto', requerido: true }, { nombre: 'fechaHora', label: 'Fecha Hora', tipo: 'fecha', requerido: true }, { nombre: 'usuario', label: 'Usuario', tipo: 'texto', requerido: true }, { nombre: 'envioId', label: 'Envio', tipo: 'ref', requerido: true, ref: 'envio' }] },
  { api: 'parametrosla', ruta: '/parametrosla', label: 'Parametro SLA', icon: 'list_alt', campoTitulo: 'etapa', conEstado: true,
    campos: [{ nombre: 'etapa', label: 'Etapa', tipo: 'enum', requerido: true, valores: ['facturacion', 'despacho', 'entrega'] }, { nombre: 'rangoAlertaUmbralAdvertenciaDias', label: 'Rango Alerta Umbral Advertencia Dias', tipo: 'numero', requerido: true }, { nombre: 'rangoAlertaLimiteDias', label: 'Rango Alerta Limite Dias', tipo: 'numero', requerido: true }] }
];
