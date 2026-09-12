import { Injectable, computed, signal } from '@angular/core';

/**
 * Rol de VISTA (plan "escenas, no tablas", 2026-09-12): Gerencia / Vendedor / Depósito.
 * Es presentación, no seguridad: cambia qué grupos del menú se ven y cuál es la pantalla
 * de inicio, para contar en la demo "esto mismo lo ve el vendedor en su celular".
 * Se guarda en localStorage; los permisos reales siguen siendo los de Seguridad.
 */
export type RolVista = 'gerencia' | 'vendedor' | 'deposito';

export interface RolVistaInfo {
  clave: RolVista;
  label: string;
  icon: string;
  /** Ruta de inicio del rol (sin barra inicial; '' = Hoy). */
  inicio: string;
  inicioLabel: string;
  /** Paths del menú visibles; null = todos. */
  paths: string[] | null;
  /** Si ve la sección "Sistema" (seguridad, configuración, auditoría, reportes). */
  sistema: boolean;
}

export const ROLES_VISTA: RolVistaInfo[] = [
  { clave: 'gerencia', label: 'Gerencia', icon: 'insights', inicio: '', inicioLabel: 'Hoy', paths: null, sistema: true },
  { clave: 'vendedor', label: 'Vendedor', icon: 'two_wheeler', inicio: 'agenda', inicioLabel: 'Mi día',
    paths: ['catalogo', 'existencias', 'agenda', 'pedidos/nuevo', 'pedido', 'cliente', 'actividad', 'comisiones'], sistema: false },
  { clave: 'deposito', label: 'Depósito', icon: 'warehouse', inicio: 'pedido', inicioLabel: 'Despacho',
    paths: ['existencias', 'catalogo', 'pedido', 'envio', 'producto', 'variante', 'deposito', 'movimientostock', 'agencia'], sistema: false },
];

const CLAVE = 'motos.rol-vista';

@Injectable({ providedIn: 'root' })
export class RolVistaService {
  private readonly _rol = signal<RolVista>(this.leer());

  readonly rol = this._rol.asReadonly();
  readonly info = computed<RolVistaInfo>(() => ROLES_VISTA.find(r => r.clave === this._rol()) ?? ROLES_VISTA[0]);
  readonly roles = ROLES_VISTA;

  cambiar(rol: RolVista): void {
    this._rol.set(rol);
    try { localStorage.setItem(CLAVE, rol); } catch { /* sin storage: queda en memoria */ }
  }

  private leer(): RolVista {
    try {
      const v = localStorage.getItem(CLAVE);
      return ROLES_VISTA.some(r => r.clave === v) ? (v as RolVista) : 'gerencia';
    } catch {
      return 'gerencia';
    }
  }
}
