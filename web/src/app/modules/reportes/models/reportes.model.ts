// =============================================================================
// MODELOS DE REPORTES Y EXPORTACIÓN
// =============================================================================

export interface ExportRequest {
  entidad: string;
  formato: 'excel' | 'csv';
  titulo?: string;
  columnas?: string[];
  columnasHeaders?: { [key: string]: string };
  filtros?: { [key: string]: any };
  ordenarPor?: string;
  ordenDesc?: boolean;
  maxRegistros?: number;
}

export interface ColumnInfo {
  name: string;
  dataType: string;
  isNullable: string;
  maxLength?: number;
}

export interface FormatoInfo {
  codigo: string;
  nombre: string;
  extension: string;
  contentType: string;
}

// =============================================================================
// REPORTES PROGRAMADOS
// =============================================================================

export interface ReporteProgramado {
  id: number;
  nombre: string;
  tipoReporte: string;
  entidad?: string;
  formato: string;
  cronExpression?: string;
  destinatarios?: string[];
  parametros?: { [key: string]: any };
  columnas?: string[];
  activo: boolean;
  ultimaEjecucion?: Date;
  proximaEjecucion?: Date;
  creadoPor: number;
  fechaCreacion: Date;
}

export interface CrearReporteProgramadoRequest {
  nombre: string;
  tipoReporte: string;
  entidad?: string;
  formato: string;
  cronExpression?: string;
  destinatarios?: string[];
  parametros?: { [key: string]: any };
  columnas?: string[];
}

// =============================================================================
// HISTORIAL DE REPORTES
// =============================================================================

export interface ReporteHistorial {
  id: number;
  reporteProgramadoId?: number;
  tipoReporte: string;
  entidad?: string;
  formato: string;
  fechaGeneracion: Date;
  generadoPor?: number;
  tamanioBytes?: number;
  registros: number;
  estado: 'EnProceso' | 'Completado' | 'Error';
  errorMensaje?: string;
  duracionMs?: number;
}

// =============================================================================
// ESTADÍSTICAS
// =============================================================================

export interface ReporteEstadisticas {
  totalGenerados: number;
  exitosos: number;
  fallidos: number;
  totalBytes: number;
  totalRegistros: number;
  porFormato: { [key: string]: number };
  porEntidad: { [key: string]: number };
  desde: Date;
  hasta: Date;
}

// =============================================================================
// TIPOS DE REPORTE
// =============================================================================

export const TIPOS_REPORTE = {
  Listado: 'Listado',
  ResumenPeriodo: 'ResumenPeriodo',
  Estadisticas: 'Estadisticas',
  ActividadUsuario: 'ActividadUsuario',
  Personalizado: 'Personalizado'
} as const;

export const FORMATOS_EXPORT = {
  excel: { codigo: 'excel', nombre: 'Excel', extension: 'xlsx' },
  csv: { codigo: 'csv', nombre: 'CSV', extension: 'csv' }
} as const;

export const ESTADOS_REPORTE = {
  EnProceso: 'EnProceso',
  Completado: 'Completado',
  Error: 'Error'
} as const;
