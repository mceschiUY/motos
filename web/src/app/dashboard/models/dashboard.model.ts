// Interfaces para el Dashboard

export interface DashboardStats {
  seguridad: SeguridadStats;
  documentos: DocumentoStats;
}

export interface SeguridadStats {
  usuarios: UsuarioStats;
  perfiles: PerfilStats;
  roles: RolStats;
  capabilities: CapabilityStats;
}

export interface UsuarioStats {
  total: number;
  activos: number;
  inactivos: number;
  bloqueados: number;
  conLoginReciente: number;
  distribucionPorPerfil: DistribucionItem[];
}

export interface PerfilStats {
  total: number;
  activos: number;
  promedioUsuariosPorPerfil: number;
}

export interface RolStats {
  total: number;
  activos: number;
  promedioCapabilitiesPorRol: number;
}

export interface CapabilityStats {
  total: number;
  activos: number;
  distribucionPorModulo: DistribucionItem[];
  modulos: string[];
}

export interface DocumentoStats {
  total: number;
  porTipoRelacion: DistribucionItem[];
}

export interface DistribucionItem {
  nombre: string;
  cantidad: number;
  porcentaje?: number;
}

export interface QuickAccessItem {
  title: string;
  description: string;
  icon: string;
  route: string;
  color: 'cyan' | 'green' | 'purple' | 'blue' | 'orange';
  stats?: { label: string; value: number }[];
}

export interface AlertItem {
  id: number;
  tipo: 'warning' | 'error' | 'info';
  titulo: string;
  mensaje: string;
  accion?: { label: string; route: string };
}
