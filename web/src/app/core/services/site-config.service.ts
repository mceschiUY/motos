import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import {
  SiteConfig,
  ConfiguracionItem,
  DEFAULT_SITE_CONFIG,
  mapToSiteConfig,
  UpdateConfigBatchItem
} from '../models/site-config.model';
import { Observable, tap, catchError, of } from 'rxjs';

/**
 * Servicio de Configuracion del Sitio
 *
 * Caracteristicas:
 * - Carga configuracion desde el backend
 * - Cache local con signals reactivos
 * - Valores por defecto si el backend no responde
 * - Metodos para administrar configuracion
 */
@Injectable({
  providedIn: 'root'
})
export class SiteConfigService {
  private readonly apiUrl = `${environment.apiUrl}/configuracion`;

  // Estado interno
  private readonly _config = signal<SiteConfig>(DEFAULT_SITE_CONFIG);
  private readonly _items = signal<ConfiguracionItem[]>([]);
  private readonly _loaded = signal(false);
  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);

  // Exponer como readonly
  readonly config = this._config.asReadonly();
  readonly items = this._items.asReadonly();
  readonly loaded = this._loaded.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();

  // Computed helpers para acceso rapido
  readonly sitioNombre = computed(() => this._config().sitioNombre);
  readonly sitioSubtitulo = computed(() => this._config().sitioSubtitulo);
  readonly sitioLogoUrl = computed(() => this._config().sitioLogoUrl);
  readonly empresaNombre = computed(() => this._config().empresaNombre);
  readonly empresaEmail = computed(() => this._config().empresaEmail);
  readonly empresaTelefono = computed(() => this._config().empresaTelefono);
  readonly empresaDireccion = computed(() => this._config().empresaDireccion);
  readonly legalCopyright = computed(() => this._config().legalCopyright);

  // Redes sociales
  readonly socialFacebook = computed(() => this._config().socialFacebook);
  readonly socialInstagram = computed(() => this._config().socialInstagram);
  readonly socialLinkedin = computed(() => this._config().socialLinkedin);
  readonly socialTwitter = computed(() => this._config().socialTwitter);
  readonly socialWhatsapp = computed(() => this._config().socialWhatsapp);
  readonly socialYoutube = computed(() => this._config().socialYoutube);

  // Tiene redes sociales configuradas?
  readonly hasRedesSociales = computed(() => {
    const c = this._config();
    return !!(c.socialFacebook || c.socialInstagram || c.socialLinkedin ||
              c.socialTwitter || c.socialWhatsapp || c.socialYoutube);
  });

  // Tiene datos de contacto?
  readonly hasContacto = computed(() => {
    const c = this._config();
    return !!(c.empresaEmail || c.empresaTelefono || c.empresaDireccion);
  });

  constructor(private http: HttpClient) {
    // Cargar configuracion al iniciar
    this.loadConfig();
  }

  /**
   * Carga la configuracion publica desde el backend
   */
  loadConfig(): void {
    if (this._loading()) return;

    this._loading.set(true);
    this._error.set(null);

    this.http.get<Record<string, string>>(`${this.apiUrl}/publica`)
      .pipe(
        tap(data => {
          this._config.set(mapToSiteConfig(data));
          this._loaded.set(true);
          this._loading.set(false);
        }),
        catchError(err => {
          console.warn('[SiteConfig] Error cargando config, usando valores por defecto:', err);
          this._config.set(DEFAULT_SITE_CONFIG);
          this._loaded.set(true);
          this._loading.set(false);
          this._error.set('Error cargando configuracion');
          return of(null);
        })
      )
      .subscribe();
  }

  /**
   * Recarga la configuracion desde el backend
   */
  refresh(): void {
    this._loaded.set(false);
    this.loadConfig();
  }

  /**
   * Obtiene un valor de configuracion por clave
   */
  getValue(key: keyof SiteConfig): string {
    return this._config()[key] || '';
  }

  // =============================================================================
  // METODOS ADMIN
  // =============================================================================

  /**
   * Obtiene todas las configuraciones (para admin)
   */
  getAllItems(): Observable<ConfiguracionItem[]> {
    return this.http.get<ConfiguracionItem[]>(this.apiUrl).pipe(
      tap(items => this._items.set(items))
    );
  }

  /**
   * Obtiene configuraciones por grupo
   */
  getByGrupo(grupo: string): Observable<ConfiguracionItem[]> {
    return this.http.get<ConfiguracionItem[]>(`${this.apiUrl}/grupo/${grupo}`);
  }

  /**
   * Actualiza una configuracion individual
   */
  updateItem(id: number, valor: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, { valor }).pipe(
      tap(() => this.refresh())
    );
  }

  /**
   * Actualiza multiples configuraciones en batch
   */
  updateBatch(items: UpdateConfigBatchItem[]): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/batch`, items).pipe(
      tap(() => this.refresh())
    );
  }
}
