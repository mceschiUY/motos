import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

// ═══════════════════════════════════════════════════════════════════════════════
// MUTATION SERVICE - ZAS Mutation Engine API
// Servicio para comunicacion con el backend de mutaciones
// ═══════════════════════════════════════════════════════════════════════════════

export interface MutacionRequest {
  solicitud: string;
  contexto?: ContextoActual;
}

export interface ContextoActual {
  modulo?: string;
  entidad?: string;
  rutaActual?: string;
  datosAdicionales?: Record<string, unknown>;
}

export interface MutacionResponse {
  mutacionId: number;
  mutationSummary: string;
  previewHtml: string;
  structuralImpact: ImpactoEstructural;
  riskAnalysis?: string;
  architectureAudit: ArchitectureAudit;
}

export interface ImpactoEstructural {
  database?: string;
  backendChanges: CambioArchivo[];
  frontendChanges: CambioArchivo[];
}

export interface CambioArchivo {
  path: string;
  action: 'create' | 'modify' | 'delete';
  code: string;
  language: string;
  description?: string;
}

export interface ArchitectureAudit {
  isCompliant: boolean;
  compliancePercentage: number;
  warnings: string[];
  recommendations: string[];
  message?: string;
}

export interface EjecucionResultado {
  success: boolean;
  mutacionId: number;
  archivosCreados: number;
  archivosModificados: number;
  archivosEliminados: number;
  archivosAfectados: string[];
}

export interface MutacionHistorial {
  id: number;
  solicitudOriginal: string;
  resumenTecnico: string;
  estado: MutacionEstado;
  fechaSolicitud: Date;
  fechaEjecucion?: Date;
  usuarioNombre: string;
  architectureCompliance: number;
  cantidadArchivos: number;
}

export interface MutacionEstadisticas {
  totalMutaciones: number;
  mutacionesEjecutadas: number;
  mutacionesFallidas: number;
  mutacionesRevertidas: number;
  promedioCompliance: number;
  archivosGenerados: number;
  archivosModificados: number;
  porEstado: Record<string, number>;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export enum MutacionEstado {
  Pendiente = 0,
  Analizada = 1,
  Previsualizada = 2,
  Ejecutada = 3,
  Fallida = 4,
  Revertida = 5
}

export const ESTADO_LABELS: Record<MutacionEstado, string> = {
  [MutacionEstado.Pendiente]: 'Pendiente',
  [MutacionEstado.Analizada]: 'Analizada',
  [MutacionEstado.Previsualizada]: 'Previsualizada',
  [MutacionEstado.Ejecutada]: 'Ejecutada',
  [MutacionEstado.Fallida]: 'Fallida',
  [MutacionEstado.Revertida]: 'Revertida'
};

@Injectable({
  providedIn: 'root'
})
export class MutationService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Mutacion`;

  /**
   * Analiza una solicitud de mutacion y genera preview + impacto estructural.
   * No ejecuta cambios, solo prepara la informacion para revision.
   */
  analyze(request: MutacionRequest): Observable<MutacionResponse> {
    return this.http.post<MutacionResponse>(`${this.baseUrl}/analyze`, request);
  }

  /**
   * Ejecuta una mutacion previamente analizada.
   * Escribe fisicamente los archivos generados.
   */
  execute(mutacionId: number): Observable<EjecucionResultado> {
    return this.http.post<EjecucionResultado>(`${this.baseUrl}/${mutacionId}/execute`, {});
  }

  /**
   * Ejecuta cambios directamente sin pasar por la base de datos.
   * Recibe los archivos a escribir y los aplica inmediatamente.
   */
  executeDirect(changes: CambioArchivo[]): Observable<EjecucionResultado> {
    return this.http.post<EjecucionResultado>(`${this.baseUrl}/execute-direct`, { changes });
  }

  /**
   * Revierte archivos usando git checkout.
   * Restaura los archivos a su estado anterior en git.
   */
  revertFiles(paths: string[]): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(`${this.baseUrl}/revert-files`, { paths });
  }

  /**
   * Revierte una mutacion ejecutada (rollback).
   * Restaura los archivos a su estado anterior.
   */
  rollback(mutacionId: number): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(`${this.baseUrl}/${mutacionId}/rollback`, {});
  }

  /**
   * Obtiene una mutacion por ID con todos sus detalles
   */
  getById(id: number): Observable<MutacionResponse> {
    return this.http.get<MutacionResponse>(`${this.baseUrl}/${id}`);
  }

  /**
   * Obtiene el historial de mutaciones con paginacion
   */
  getHistorial(page: number = 1, pageSize: number = 20, usuarioId?: number): Observable<PaginatedResult<MutacionHistorial>> {
    let url = `${this.baseUrl}/historial?page=${page}&pageSize=${pageSize}`;
    if (usuarioId) {
      url += `&usuarioId=${usuarioId}`;
    }
    return this.http.get<PaginatedResult<MutacionHistorial>>(url);
  }

  /**
   * Obtiene las ultimas mutaciones
   */
  getRecientes(cantidad: number = 10): Observable<MutacionHistorial[]> {
    return this.http.get<MutacionHistorial[]>(`${this.baseUrl}/recientes?cantidad=${cantidad}`);
  }

  /**
   * Obtiene estadisticas de mutaciones para el dashboard
   */
  getEstadisticas(desde?: Date, hasta?: Date): Observable<MutacionEstadisticas> {
    let url = `${this.baseUrl}/estadisticas`;
    const params: string[] = [];
    if (desde) params.push(`desde=${desde.toISOString()}`);
    if (hasta) params.push(`hasta=${hasta.toISOString()}`);
    if (params.length > 0) url += '?' + params.join('&');
    return this.http.get<MutacionEstadisticas>(url);
  }
}
