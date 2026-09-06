import { Injectable, signal, computed } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse, UsuarioInfo, AuthState } from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'auth_token';
  private readonly USER_KEY = 'auth_user';

  // Estado reactivo con signals
  private authState = signal<AuthState>({
    isAuthenticated: false,
    token: null,
    usuario: null
  });

  // Computed signals para acceso facil
  isAuthenticated = computed(() => this.authState().isAuthenticated);
  currentUser = computed(() => this.authState().usuario);
  token = computed(() => this.authState().token);

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.loadStoredAuth();
  }

  private loadStoredAuth(): void {
    const token = localStorage.getItem(this.TOKEN_KEY);
    const userJson = localStorage.getItem(this.USER_KEY);

    if (token && userJson) {
      try {
        const usuario = JSON.parse(userJson) as UsuarioInfo;
        this.authState.set({
          isAuthenticated: true,
          token,
          usuario
        });
      } catch {
        this.clearStorage();
      }
    }
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(
      `${environment.apiUrl}/auth/login`,
      credentials
    ).pipe(
      tap(response => {
        this.setAuth(response.token, response.usuario);
      }),
      catchError(error => {
        console.error('Error en login:', error);
        throw error;
      })
    );
  }

  logout(): void {
    this.clearStorage();
    this.authState.set({
      isAuthenticated: false,
      token: null,
      usuario: null
    });
    this.router.navigate(['/login']);
  }

  private setAuth(token: string, usuario: UsuarioInfo): void {
    localStorage.setItem(this.TOKEN_KEY, token);
    localStorage.setItem(this.USER_KEY, JSON.stringify(usuario));
    this.authState.set({
      isAuthenticated: true,
      token,
      usuario
    });
  }

  private clearStorage(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
  }

  getAuthHeaders(): HttpHeaders {
    const token = this.token();
    return new HttpHeaders({
      'Authorization': token ? `Bearer ${token}` : '',
      'Content-Type': 'application/json'
    });
  }

  hasCapability(capability: string): boolean {
    const user = this.currentUser();
    if (!user || !user.capabilities) return false;
    return user.capabilities.includes(capability);
  }

  hasAnyCapability(...capabilities: string[]): boolean {
    return capabilities.some(cap => this.hasCapability(cap));
  }

  // Bypass de desarrollo - solo para testing local
  devLogin(usuario: string): void {
    const devUser: UsuarioInfo = {
      id: 1,
      nombreUsuario: usuario,
      nombreCompleto: 'Usuario Desarrollo',
      email: `${usuario}@dev.local`,
      perfilId: 1,
      perfilNombre: 'Administrador',
      capabilities: ['Admin.Full']
    };
    const devToken = 'dev-token-' + Date.now();
    this.setAuth(devToken, devUser);
  }
}
