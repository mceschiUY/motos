import { Injectable, signal, effect, computed } from '@angular/core';

export interface UserPreferences {
  theme: 'dark' | 'light' | 'system';
  sidebarCollapsed: boolean;
  tablePageSize: number;
  language: string;
  compactMode: boolean;
  showAnimations: boolean;
}

const DEFAULT_PREFERENCES: UserPreferences = {
  theme: 'dark',
  sidebarCollapsed: false,
  tablePageSize: 10,
  language: 'es',
  compactMode: false,
  showAnimations: true
};

/**
 * Servicio de Preferencias de Usuario
 *
 * Persiste preferencias en localStorage.
 * Compatible con componentes generados - las preferencias se aplican globalmente.
 */
@Injectable({
  providedIn: 'root'
})
export class UserPreferencesService {
  private readonly STORAGE_KEY = 'app_user_preferences';

  // Estado de preferencias
  private readonly preferences = signal<UserPreferences>(this.loadPreferences());

  // Getters individuales para cada preferencia
  readonly theme = computed(() => this.preferences().theme);
  readonly sidebarCollapsed = computed(() => this.preferences().sidebarCollapsed);
  readonly tablePageSize = computed(() => this.preferences().tablePageSize);
  readonly language = computed(() => this.preferences().language);
  readonly compactMode = computed(() => this.preferences().compactMode);
  readonly showAnimations = computed(() => this.preferences().showAnimations);

  constructor() {
    // Persistir automaticamente cuando cambien las preferencias
    effect(() => {
      const prefs = this.preferences();
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(prefs));
    });

    // Aplicar clase de modo compacto
    effect(() => {
      document.documentElement.classList.toggle('compact-mode', this.compactMode());
    });

    // Aplicar clase para deshabilitar animaciones
    effect(() => {
      document.documentElement.classList.toggle('no-animations', !this.showAnimations());
    });
  }

  /**
   * Actualiza una o mas preferencias
   */
  update(partial: Partial<UserPreferences>): void {
    this.preferences.update(current => ({
      ...current,
      ...partial
    }));
  }

  /**
   * Obtiene todas las preferencias
   */
  getAll(): UserPreferences {
    return this.preferences();
  }

  /**
   * Resetea a valores por defecto
   */
  reset(): void {
    this.preferences.set(DEFAULT_PREFERENCES);
  }

  /**
   * Toggle sidebar collapsed
   */
  toggleSidebar(): void {
    this.update({ sidebarCollapsed: !this.sidebarCollapsed() });
  }

  /**
   * Toggle compact mode
   */
  toggleCompactMode(): void {
    this.update({ compactMode: !this.compactMode() });
  }

  private loadPreferences(): UserPreferences {
    try {
      const stored = localStorage.getItem(this.STORAGE_KEY);
      if (stored) {
        const parsed = JSON.parse(stored);
        return { ...DEFAULT_PREFERENCES, ...parsed };
      }
    } catch (e) {
      console.warn('Error loading preferences:', e);
    }
    return DEFAULT_PREFERENCES;
  }
}
