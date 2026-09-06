// ============================================================================
// MODELOS DE AUDITORIA - SiteMotos
// ============================================================================

export interface AuditLog {
  id: number;
  timestamp: Date;
  userId: number | null;
  userName: string | null;
  action: string;
  entityType: string;
  entityId: string | null;
  oldValues: string | null;
  newValues: string | null;
  ipAddress: string | null;
  userAgent: string | null;
  requestPath: string | null;
  durationMs: number | null;
  success: boolean;
  errorMessage: string | null;
}

export interface AuditLogPagedResponse {
  items: AuditLog[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface AuditLogFiltro {
  desde?: Date;
  hasta?: Date;
  userId?: number;
  userName?: string;
  actions?: string[];
  entityType?: string;
  entityId?: string;
  success?: boolean;
  search?: string;
  page: number;
  pageSize: number;
  sortBy: string;
  sortDesc: boolean;
}

export interface AuditStats {
  totalRegistros: number;
  desde: Date;
  hasta: Date;
  porAccion: { [key: string]: number };
  topUsuarios: UserActivity[];
}

export interface UserActivity {
  userId: number;
  userName: string | null;
  acciones: number;
}

// Constantes de acciones
export const AUDIT_ACTIONS = {
  CREATE: 'Create',
  UPDATE: 'Update',
  DELETE: 'Delete',
  READ: 'Read',
  LOGIN: 'Login',
  LOGOUT: 'Logout',
  LOGIN_FAILED: 'LoginFailed',
  EXPORT: 'Export'
};

// Helpers
export function getActionLabel(action: string): string {
  switch (action) {
    case 'Create': return 'Crear';
    case 'Update': return 'Modificar';
    case 'Delete': return 'Eliminar';
    case 'Read': return 'Leer';
    case 'Login': return 'Inicio sesion';
    case 'Logout': return 'Cierre sesion';
    case 'LoginFailed': return 'Login fallido';
    case 'Export': return 'Exportar';
    default: return action;
  }
}

export function getActionIcon(action: string): string {
  switch (action) {
    case 'Create': return 'add_circle';
    case 'Update': return 'edit';
    case 'Delete': return 'delete';
    case 'Read': return 'visibility';
    case 'Login': return 'login';
    case 'Logout': return 'logout';
    case 'LoginFailed': return 'block';
    case 'Export': return 'file_download';
    default: return 'info';
  }
}

export function getActionColor(action: string): string {
  switch (action) {
    case 'Create': return 'accent-green';
    case 'Update': return 'accent-blue';
    case 'Delete': return 'accent-red';
    case 'Read': return 'accent-gray';
    case 'Login': return 'accent-purple';
    case 'Logout': return 'accent-orange';
    case 'LoginFailed': return 'accent-red';
    case 'Export': return 'accent-cyan';
    default: return 'accent-gray';
  }
}
