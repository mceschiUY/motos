import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  AuditLog,
  AuditLogPagedResponse,
  AuditLogFiltro,
  AuditStats
} from '../models/auditoria.model';

@Injectable({
  providedIn: 'root'
})
export class AuditoriaService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/auditoria`;

  /**
   * Obtiene registros de auditoria con filtros y paginacion
   */
  getAuditLogs(filtro: Partial<AuditLogFiltro>): Observable<AuditLogPagedResponse> {
    let params = new HttpParams();

    if (filtro.desde) params = params.set('desde', filtro.desde.toISOString());
    if (filtro.hasta) params = params.set('hasta', filtro.hasta.toISOString());
    if (filtro.userId) params = params.set('userId', filtro.userId.toString());
    if (filtro.userName) params = params.set('userName', filtro.userName);
    if (filtro.actions && filtro.actions.length > 0) {
      params = params.set('actions', filtro.actions.join(','));
    }
    if (filtro.entityType) params = params.set('entityType', filtro.entityType);
    if (filtro.entityId) params = params.set('entityId', filtro.entityId);
    if (filtro.success !== undefined) params = params.set('success', filtro.success.toString());
    if (filtro.search) params = params.set('search', filtro.search);
    if (filtro.page) params = params.set('page', filtro.page.toString());
    if (filtro.pageSize) params = params.set('pageSize', filtro.pageSize.toString());
    if (filtro.sortBy) params = params.set('sortBy', filtro.sortBy);
    if (filtro.sortDesc !== undefined) params = params.set('sortDesc', filtro.sortDesc.toString());

    return this.http.get<AuditLogPagedResponse>(this.baseUrl, { params });
  }

  /**
   * Obtiene los ultimos N registros
   */
  getRecientes(cantidad: number = 50): Observable<AuditLog[]> {
    return this.http.get<AuditLog[]>(`${this.baseUrl}/recientes`, {
      params: { cantidad: cantidad.toString() }
    });
  }

  /**
   * Obtiene registros de un usuario especifico
   */
  getByUsuario(userId: number, limit: number = 100): Observable<AuditLog[]> {
    return this.http.get<AuditLog[]>(`${this.baseUrl}/usuario/${userId}`, {
      params: { limit: limit.toString() }
    });
  }

  /**
   * Obtiene historial de una entidad
   */
  getByEntidad(entityType: string, entityId: string): Observable<AuditLog[]> {
    return this.http.get<AuditLog[]>(`${this.baseUrl}/entidad/${entityType}/${entityId}`);
  }

  /**
   * Obtiene registros por tipo de accion
   */
  getByAccion(action: string, limit: number = 100): Observable<AuditLog[]> {
    return this.http.get<AuditLog[]>(`${this.baseUrl}/accion/${action}`, {
      params: { limit: limit.toString() }
    });
  }

  /**
   * Obtiene lista de acciones disponibles
   */
  getAcciones(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/acciones`);
  }

  /**
   * Obtiene estadisticas de auditoria
   */
  getEstadisticas(desde?: Date, hasta?: Date): Observable<AuditStats> {
    let params = new HttpParams();
    if (desde) params = params.set('desde', desde.toISOString());
    if (hasta) params = params.set('hasta', hasta.toISOString());

    return this.http.get<AuditStats>(`${this.baseUrl}/estadisticas`, { params });
  }

  /**
   * Elimina registros antiguos (requiere permiso especial)
   */
  limpiar(diasRetencion: number = 90): Observable<number> {
    return this.http.delete<number>(`${this.baseUrl}/limpiar`, {
      params: { diasRetencion: diasRetencion.toString() }
    });
  }
}
