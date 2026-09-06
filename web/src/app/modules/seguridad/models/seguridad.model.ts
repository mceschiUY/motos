// ============================================================================
// MODELOS DE SEGURIDAD - SiteMotos
// ============================================================================

export interface Capability {
  id: number;
  nombre: string;
  descripcion: string;
  modulo: string;
  activo: boolean;
}

export interface Rol {
  id: number;
  nombre: string;
  descripcion: string;
  activo: boolean;
  fechaCreacion: Date;
  capabilities?: Capability[];
  cantidadCapabilities?: number;
}

export interface Perfil {
  id: number;
  nombre: string;
  descripcion: string;
  activo: boolean;
  fechaCreacion: Date;
  roles?: Rol[];
  cantidadRoles?: number;
  cantidadUsuarios?: number;
}

export interface Usuario {
  id: number;
  userName: string;
  email: string;
  nombreCompleto: string;
  perfilId: number;
  perfilNombre?: string;
  activo: boolean;
  fechaCreacion: Date;
  ultimoLogin?: Date;
  bloqueadoHasta?: Date;
}

// DTOs para crear/modificar
export interface CrearUsuarioDto {
  userName: string;
  email: string;
  nombreCompleto: string;
  password: string;
  perfilId: number;
}

export interface ModificarUsuarioDto {
  id: number;
  userName: string;
  email: string;
  nombreCompleto: string;
  perfilId: number;
}

export interface CambiarPasswordDto {
  usuarioId: number;
  nuevaPassword: string;
}

export interface CrearRolDto {
  nombre: string;
  descripcion: string;
}

export interface CrearPerfilDto {
  nombre: string;
  descripcion: string;
}

export interface AsignarCapabilitiesDto {
  rolId: number;
  capabilityIds: number[];
}

export interface AsignarRolesDto {
  perfilId: number;
  rolIds: number[];
}
