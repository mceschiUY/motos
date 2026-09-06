import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin, map, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { DashboardStats, AlertItem } from '../models/dashboard.model';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  /**
   * Obtiene todas las estadisticas del dashboard en paralelo
   */
  getDashboardStats(): Observable<DashboardStats> {
    return forkJoin({
      usuarios: this.http.get<any[]>(`${this.baseUrl}/seguridad/usuarios`).pipe(catchError(() => of([]))),
      perfiles: this.http.get<any[]>(`${this.baseUrl}/seguridad/perfiles`).pipe(catchError(() => of([]))),
      roles: this.http.get<any[]>(`${this.baseUrl}/seguridad/roles`).pipe(catchError(() => of([]))),
      capabilities: this.http.get<any[]>(`${this.baseUrl}/seguridad/capabilities`).pipe(catchError(() => of([]))),
      modulos: this.http.get<string[]>(`${this.baseUrl}/seguridad/capabilities/modulos`).pipe(catchError(() => of([]))),
      documentos: this.http.get<any[]>(`${this.baseUrl}/Documentos`).pipe(catchError(() => of([])))
    }).pipe(
      map(data => this.processStats(data))
    );
  }

  /**
   * Genera alertas basadas en los datos actuales
   */
  getAlerts(): Observable<AlertItem[]> {
    return this.http.get<any[]>(`${this.baseUrl}/seguridad/usuarios`).pipe(
      map(usuarios => this.generateAlerts(usuarios)),
      catchError(() => of([]))
    );
  }

  private processStats(data: {
    usuarios: any[];
    perfiles: any[];
    roles: any[];
    capabilities: any[];
    modulos: string[];
    documentos: any[];
  }): DashboardStats {
    const now = new Date();
    const oneDayAgo = new Date(now.getTime() - 24 * 60 * 60 * 1000);

    // Procesar usuarios
    const usuariosActivos = data.usuarios.filter((u: any) => u.activo);
    const usuariosBloqueados = data.usuarios.filter((u: any) =>
      u.bloqueadoHasta && new Date(u.bloqueadoHasta) > now
    );
    const loginReciente = data.usuarios.filter((u: any) =>
      u.ultimoLogin && new Date(u.ultimoLogin) > oneDayAgo
    );

    // Distribucion por perfil
    const perfilCounts = new Map<string, number>();
    data.usuarios.forEach((u: any) => {
      const perfil = u.perfilNombre || 'Sin Perfil';
      perfilCounts.set(perfil, (perfilCounts.get(perfil) || 0) + 1);
    });

    // Distribucion de capabilities por modulo
    const moduloCounts = new Map<string, number>();
    data.capabilities.forEach((c: any) => {
      const modulo = c.modulo || 'Sin Modulo';
      moduloCounts.set(modulo, (moduloCounts.get(modulo) || 0) + 1);
    });

    // Distribucion de documentos por relacion
    const relacionCounts = new Map<string, number>();
    data.documentos.forEach((d: any) => {
      const relacion = d.relacionNombre || 'General';
      relacionCounts.set(relacion, (relacionCounts.get(relacion) || 0) + 1);
    });

    const totalUsuarios = data.usuarios.length || 1;
    const totalCapabilities = data.capabilities.length || 1;

    return {
      seguridad: {
        usuarios: {
          total: data.usuarios.length,
          activos: usuariosActivos.length,
          inactivos: data.usuarios.length - usuariosActivos.length,
          bloqueados: usuariosBloqueados.length,
          conLoginReciente: loginReciente.length,
          distribucionPorPerfil: Array.from(perfilCounts.entries()).map(([nombre, cantidad]) => ({
            nombre,
            cantidad,
            porcentaje: Math.round((cantidad / totalUsuarios) * 100)
          })).sort((a, b) => b.cantidad - a.cantidad)
        },
        perfiles: {
          total: data.perfiles.length,
          activos: data.perfiles.filter((p: any) => p.activo).length,
          promedioUsuariosPorPerfil: data.perfiles.length > 0
            ? Math.round(data.usuarios.length / data.perfiles.length)
            : 0
        },
        roles: {
          total: data.roles.length,
          activos: data.roles.filter((r: any) => r.activo).length,
          promedioCapabilitiesPorRol: data.roles.length > 0
            ? Math.round(data.roles.reduce((sum: number, r: any) => sum + (r.cantidadCapabilities || 0), 0) / data.roles.length)
            : 0
        },
        capabilities: {
          total: data.capabilities.length,
          activos: data.capabilities.filter((c: any) => c.activo).length,
          distribucionPorModulo: Array.from(moduloCounts.entries()).map(([nombre, cantidad]) => ({
            nombre,
            cantidad,
            porcentaje: Math.round((cantidad / totalCapabilities) * 100)
          })).sort((a, b) => b.cantidad - a.cantidad),
          modulos: data.modulos
        }
      },
      documentos: {
        total: data.documentos.length,
        porTipoRelacion: Array.from(relacionCounts.entries()).map(([nombre, cantidad]) => ({
          nombre,
          cantidad
        })).sort((a, b) => b.cantidad - a.cantidad)
      }
    };
  }

  private generateAlerts(usuarios: any[]): AlertItem[] {
    const alerts: AlertItem[] = [];
    const now = new Date();

    // Alertas de usuarios bloqueados
    const bloqueados = usuarios.filter(u => u.bloqueadoHasta && new Date(u.bloqueadoHasta) > now);
    if (bloqueados.length > 0) {
      alerts.push({
        id: 1,
        tipo: 'warning',
        titulo: 'Usuarios Bloqueados',
        mensaje: `Hay ${bloqueados.length} usuario(s) bloqueado(s) por intentos fallidos de login`,
        accion: { label: 'Ver usuarios', route: '/seguridad/usuarios' }
      });
    }

    // Alertas de usuarios inactivos
    const inactivos = usuarios.filter(u => !u.activo);
    if (inactivos.length > 5) {
      alerts.push({
        id: 2,
        tipo: 'info',
        titulo: 'Usuarios Inactivos',
        mensaje: `${inactivos.length} usuarios estan desactivados en el sistema`,
        accion: { label: 'Revisar', route: '/seguridad/usuarios' }
      });
    }

    return alerts;
  }
}
