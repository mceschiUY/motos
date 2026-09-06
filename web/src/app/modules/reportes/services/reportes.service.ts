import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  ExportRequest,
  ColumnInfo,
  FormatoInfo,
  ReporteProgramado,
  CrearReporteProgramadoRequest,
  ReporteHistorial,
  ReporteEstadisticas
} from '../models/reportes.model';

@Injectable({
  providedIn: 'root'
})
export class ReportesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/reportes`;

  // =============================================================================
  // EXPORTACIÓN
  // =============================================================================

  /**
   * Exporta datos a Excel o CSV y descarga el archivo
   */
  exportar(request: ExportRequest): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/exportar`, request, {
      responseType: 'blob'
    });
  }

  /**
   * Exporta y dispara la descarga del archivo automáticamente
   */
  exportarYDescargar(request: ExportRequest): void {
    this.exportar(request).subscribe({
      next: (blob) => {
        const extension = request.formato === 'csv' ? 'csv' : 'xlsx';
        const filename = `${request.entidad}_${new Date().toISOString().slice(0,10)}.${extension}`;
        this.descargarArchivo(blob, filename);
      },
      error: (err) => {
        console.error('Error exportando:', err);
      }
    });
  }

  /**
   * Obtiene las columnas disponibles de una entidad
   */
  getColumnas(entidad: string): Observable<ColumnInfo[]> {
    return this.http.get<ColumnInfo[]>(`${this.baseUrl}/columnas/${entidad}`);
  }

  /**
   * Obtiene los formatos de exportación disponibles
   */
  getFormatos(): Observable<FormatoInfo[]> {
    return this.http.get<FormatoInfo[]>(`${this.baseUrl}/formatos`);
  }

  // =============================================================================
  // HISTORIAL
  // =============================================================================

  /**
   * Obtiene el historial de reportes generados
   */
  getHistorial(limit: number = 50, entidad?: string): Observable<ReporteHistorial[]> {
    let params = new HttpParams().set('limit', limit.toString());
    if (entidad) {
      params = params.set('entidad', entidad);
    }
    return this.http.get<ReporteHistorial[]>(`${this.baseUrl}/historial`, { params });
  }

  /**
   * Obtiene mis reportes generados
   */
  getMisReportes(limit: number = 50): Observable<ReporteHistorial[]> {
    return this.http.get<ReporteHistorial[]>(`${this.baseUrl}/historial/mis-reportes`, {
      params: { limit: limit.toString() }
    });
  }

  /**
   * Obtiene estadísticas de reportes
   */
  getEstadisticas(desde?: Date, hasta?: Date): Observable<ReporteEstadisticas> {
    let params = new HttpParams();
    if (desde) params = params.set('desde', desde.toISOString());
    if (hasta) params = params.set('hasta', hasta.toISOString());
    return this.http.get<ReporteEstadisticas>(`${this.baseUrl}/estadisticas`, { params });
  }

  /**
   * Limpia historial antiguo
   */
  limpiarHistorial(diasRetencion: number = 90): Observable<number> {
    return this.http.delete<number>(`${this.baseUrl}/historial/limpiar`, {
      params: { diasRetencion: diasRetencion.toString() }
    });
  }

  // =============================================================================
  // REPORTES PROGRAMADOS
  // =============================================================================

  /**
   * Obtiene todos los reportes programados
   */
  getProgramados(): Observable<ReporteProgramado[]> {
    return this.http.get<ReporteProgramado[]>(`${this.baseUrl}/programados`);
  }

  /**
   * Obtiene un reporte programado por ID
   */
  getProgramado(id: number): Observable<ReporteProgramado> {
    return this.http.get<ReporteProgramado>(`${this.baseUrl}/programados/${id}`);
  }

  /**
   * Crea un nuevo reporte programado
   */
  crearProgramado(request: CrearReporteProgramadoRequest): Observable<number> {
    return this.http.post<number>(`${this.baseUrl}/programados`, request);
  }

  /**
   * Activa un reporte programado
   */
  activarProgramado(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/programados/${id}/activar`, {});
  }

  /**
   * Desactiva un reporte programado
   */
  desactivarProgramado(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/programados/${id}/desactivar`, {});
  }

  /**
   * Elimina un reporte programado
   */
  eliminarProgramado(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/programados/${id}`);
  }

  // =============================================================================
  // UTILIDADES
  // =============================================================================

  private descargarArchivo(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    window.URL.revokeObjectURL(url);
  }
}
