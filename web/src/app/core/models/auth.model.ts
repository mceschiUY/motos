// Modelos de autenticacion para SiteMotos

export interface LoginRequest {
  usuario: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  usuario: UsuarioInfo;
}

export interface UsuarioInfo {
  id: number;
  nombreUsuario: string;
  nombreCompleto: string;
  email: string;
  perfilId: number;
  perfilNombre: string;
  capabilities: string[];
}

export interface AuthState {
  isAuthenticated: boolean;
  token: string | null;
  usuario: UsuarioInfo | null;
}
