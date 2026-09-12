import { Injectable, inject, signal } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

const CLAVE = 'motos.modo-movil';

/**
 * App del vendedor en `/m` (plan Etapa F4, 2026-09-12). Mientras el "modo móvil" está activo,
 * cualquier navegación al sitio completo (`/cliente/2`, `/pedidos/nuevo`, …) se redirige a la
 * misma ruta bajo `/m`: las escenas artesanales navegan con rutas absolutas y así no hace falta
 * tocarlas para que se queden dentro del shell del celular. Se activa al entrar a `/m` y se
 * apaga con "Sitio completo". Es presentación, no seguridad (ver Etapa G del plan).
 */
@Injectable({ providedIn: 'root' })
export class ModoMovilService {
  /**
   * Marco de celular para Teams (plan Etapa F6): la página `/celular` carga el sitio en dos
   * iframes, uno con `/m?modo=marco` (teléfono) y otro con `/?modo=escritorio` (tablero).
   * Comparten localStorage, así que en esos casos el modo se fija por la URL de arranque y
   * NO se persiste: el teléfono queda en móvil, el tablero en escritorio, y la ventana
   * principal no se entera.
   */
  private readonly modoUrl = ModoMovilService.leerModoUrl();
  private readonly persistir = this.modoUrl == null;
  private readonly _activo = signal<boolean>(this.modoUrl != null ? this.modoUrl === 'marco' : this.leer());
  readonly activo = this._activo.asReadonly();

  entrar(): void { if (this.modoUrl === 'escritorio') return; this._activo.set(true); if (this.persistir) this.guardar('1'); }
  salir(): void { this._activo.set(false); if (this.persistir) this.guardar(null); }

  private static leerModoUrl(): 'marco' | 'escritorio' | null {
    try {
      const m = new URLSearchParams(window.location.search).get('modo');
      return m === 'marco' || m === 'escritorio' ? m : null;
    } catch { return null; }
  }

  private leer(): boolean {
    try { return localStorage.getItem(CLAVE) === '1'; } catch { return false; }
  }

  private guardar(v: string | null): void {
    try {
      if (v == null) { localStorage.removeItem(CLAVE); } else { localStorage.setItem(CLAVE, v); }
    } catch { /* sin storage: queda en memoria */ }
  }
}

/** En el layout de escritorio: con el modo móvil activo, la misma URL bajo `/m`. */
export const modoMovilGuard: CanActivateFn = (_route, state) => {
  const modo = inject(ModoMovilService);
  const router = inject(Router);
  if (!modo.activo()) { return true; }
  const url = state.url === '/' ? '' : state.url;
  return router.parseUrl('/m' + url);
};
