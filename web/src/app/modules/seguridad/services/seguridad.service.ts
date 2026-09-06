import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  Capability,
  Rol,
  Perfil,
  Usuario,
  CrearUsuarioDto,
  ModificarUsuarioDto,
  CambiarPasswordDto,
  CrearRolDto,
  CrearPerfilDto,
  AsignarCapabilitiesDto,
  AsignarRolesDto
} from '../models/seguridad.model';

@Injectable({
  providedIn: 'root'
})
export class SeguridadService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  // ============================================================================
  // CAPABILITIES
  // ============================================================================

  getCapabilities(): Observable<Capability[]> {
    return this.http.get<Capability[]>(`${this.baseUrl}/seguridad/capabilities`);
  }

  getCapabilitiesByModulo(modulo: string): Observable<Capability[]> {
    return this.http.get<Capability[]>(`${this.baseUrl}/seguridad/capabilities/modulo/${modulo}`);
  }

  getModulos(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/seguridad/capabilities/modulos`);
  }

  // ============================================================================
  // ROLES
  // ============================================================================

  getRoles(): Observable<Rol[]> {
    return this.http.get<Rol[]>(`${this.baseUrl}/seguridad/roles`);
  }

  getRol(id: number): Observable<Rol> {
    return this.http.get<Rol>(`${this.baseUrl}/seguridad/roles/${id}`);
  }

  crearRol(dto: CrearRolDto): Observable<number> {
    return this.http.post<number>(`${this.baseUrl}/seguridad/roles`, dto);
  }

  modificarRol(id: number, dto: CrearRolDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/seguridad/roles/${id}`, dto);
  }

  eliminarRol(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/seguridad/roles/${id}`);
  }

  toggleRolActivo(id: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/seguridad/roles/${id}/toggle-activo`, {});
  }

  asignarCapabilities(dto: AsignarCapabilitiesDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/seguridad/roles/${dto.rolId}/capabilities`, dto);
  }

  getRolCapabilities(rolId: number): Observable<Capability[]> {
    return this.http.get<Capability[]>(`${this.baseUrl}/seguridad/roles/${rolId}/capabilities`);
  }

  // ============================================================================
  // PERFILES
  // ============================================================================

  getPerfiles(): Observable<Perfil[]> {
    return this.http.get<Perfil[]>(`${this.baseUrl}/seguridad/perfiles`);
  }

  getPerfil(id: number): Observable<Perfil> {
    return this.http.get<Perfil>(`${this.baseUrl}/seguridad/perfiles/${id}`);
  }

  crearPerfil(dto: CrearPerfilDto): Observable<number> {
    return this.http.post<number>(`${this.baseUrl}/seguridad/perfiles`, dto);
  }

  modificarPerfil(id: number, dto: CrearPerfilDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/seguridad/perfiles/${id}`, dto);
  }

  eliminarPerfil(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/seguridad/perfiles/${id}`);
  }

  togglePerfilActivo(id: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/seguridad/perfiles/${id}/toggle-activo`, {});
  }

  asignarRoles(dto: AsignarRolesDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/seguridad/perfiles/${dto.perfilId}/roles`, dto);
  }

  getPerfilRoles(perfilId: number): Observable<Rol[]> {
    return this.http.get<Rol[]>(`${this.baseUrl}/seguridad/perfiles/${perfilId}/roles`);
  }

  // ============================================================================
  // USUARIOS
  // ============================================================================

  getUsuarios(): Observable<Usuario[]> {
    return this.http.get<Usuario[]>(`${this.baseUrl}/seguridad/usuarios`);
  }

  getUsuario(id: number): Observable<Usuario> {
    return this.http.get<Usuario>(`${this.baseUrl}/seguridad/usuarios/${id}`);
  }

  crearUsuario(dto: CrearUsuarioDto): Observable<number> {
    return this.http.post<number>(`${this.baseUrl}/seguridad/usuarios`, dto);
  }

  modificarUsuario(dto: ModificarUsuarioDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/seguridad/usuarios/${dto.id}`, dto);
  }

  eliminarUsuario(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/seguridad/usuarios/${id}`);
  }

  toggleUsuarioActivo(id: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/seguridad/usuarios/${id}/toggle-activo`, {});
  }

  cambiarPassword(dto: CambiarPasswordDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/seguridad/usuarios/${dto.usuarioId}/password`, dto);
  }

  desbloquearUsuario(id: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/seguridad/usuarios/${id}/desbloquear`, {});
  }
}
