import { Injectable, signal, computed, effect } from '@angular/core';

export type ThemeMode = 'dark' | 'light' | 'system';

/**
 * Servicio de Temas (Dark/Light Mode)
 *
 * Caracteristicas:
 * - Soporte para modo oscuro, claro y automatico (sistema)
 * - Persistencia en localStorage
 * - Detecta preferencia del sistema operativo
 * - Aplica clase CSS al document para theming
 *
 * Compatible con Angular Material - hereda el tema automaticamente.
 */
@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly STORAGE_KEY = 'app_theme_mode';

  // Preferencia del sistema
  private readonly systemPrefersDark = signal(
    window.matchMedia('(prefers-color-scheme: dark)').matches
  );

  // Modo seleccionado por el usuario
  readonly mode = signal<ThemeMode>(this.loadStoredTheme());

  // Tema efectivo (resuelve 'system' al tema real)
  readonly effectiveTheme = computed(() => {
    const mode = this.mode();
    if (mode === 'system') {
      return this.systemPrefersDark() ? 'dark' : 'light';
    }
    return mode;
  });

  // Helpers
  readonly isDark = computed(() => this.effectiveTheme() === 'dark');
  readonly isLight = computed(() => this.effectiveTheme() === 'light');

  constructor() {
    // Escuchar cambios en preferencia del sistema
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    mediaQuery.addEventListener('change', (e) => {
      this.systemPrefersDark.set(e.matches);
    });

    // Aplicar tema cuando cambie
    effect(() => {
      this.applyTheme(this.effectiveTheme());
    });
  }

  /**
   * Cambia el modo de tema
   */
  setMode(mode: ThemeMode): void {
    this.mode.set(mode);
    localStorage.setItem(this.STORAGE_KEY, mode);
  }

  /**
   * Toggle entre dark y light (ignora system)
   */
  toggle(): void {
    const current = this.effectiveTheme();
    this.setMode(current === 'dark' ? 'light' : 'dark');
  }

  /**
   * Cicla entre los 3 modos: dark -> light -> system -> dark
   */
  cycle(): void {
    const modes: ThemeMode[] = ['dark', 'light', 'system'];
    const currentIndex = modes.indexOf(this.mode());
    const nextIndex = (currentIndex + 1) % modes.length;
    this.setMode(modes[nextIndex]);
  }

  private loadStoredTheme(): ThemeMode {
    const stored = localStorage.getItem(this.STORAGE_KEY) as ThemeMode;
    if (stored && ['dark', 'light', 'system'].includes(stored)) {
      return stored;
    }
    return 'dark'; // Default a dark mode
  }

  private applyTheme(theme: 'dark' | 'light'): void {
    const root = document.documentElement;
    const body = document.body;

    // Remover clases anteriores de html y body
    root.classList.remove('dark-theme', 'light-theme');
    body?.classList.remove('dark-theme', 'light-theme');

    // Agregar clase del tema actual a html y body
    root.classList.add(`${theme}-theme`);
    body?.classList.add(`${theme}-theme`);

    // También establecer data-theme attribute para máxima compatibilidad
    root.setAttribute('data-theme', theme);

    // Actualizar meta theme-color para mobile
    const metaThemeColor = document.querySelector('meta[name="theme-color"]');
    if (metaThemeColor) {
      metaThemeColor.setAttribute('content', theme === 'dark' ? '#0B1120' : '#f2f7fc');
    }

    console.log(`[ThemeService] Applied theme: ${theme}`);
  }
}
