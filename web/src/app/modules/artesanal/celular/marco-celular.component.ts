import { Component, HostListener, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';

interface Destino { label: string; icon: string; ruta: string; }

/**
 * MARCO DE CELULAR para la demo por Teams (plan Etapa F6, 2026-09-12). Muestra la app del
 * vendedor (`/m`) dentro de un teléfono dibujado en CSS y, en "vista doble", al lado el
 * tablero "Hoy" de la gerencia: lo que el vendedor hace en el celular (crear un pedido) se ve
 * cambiar en el tablero. Todo en la misma máquina, sin puertos ni WiFi: son dos iframes del
 * mismo origen. `?modo=marco` / `?modo=escritorio` fijan el modo móvil de cada iframe sin
 * persistirlo (ver ModoMovilService). Esc o ✕ para volver al sitio.
 */
@Component({
  selector: 'app-marco-celular',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, MatTooltipModule],
  template: `
    <div class="mk" [class.doble]="doble()">
      <header class="mk-barra">
        <span class="mk-titulo"><mat-icon>smartphone</mat-icon> Marco de celular</span>
        <nav class="mk-destinos">
          @for (d of destinos; track d.ruta) {
            <button mat-button [class.activo]="destino() === d.ruta" (click)="ir(d.ruta)"><mat-icon>{{ d.icon }}</mat-icon>{{ d.label }}</button>
          }
        </nav>
        <span class="mk-espacio"></span>
        <button mat-stroked-button (click)="alternarDoble()" [matTooltip]="doble() ? 'Solo el teléfono' : 'Teléfono + tablero de gerencia'">
          <mat-icon>{{ doble() ? 'smartphone' : 'vertical_split' }}</mat-icon>{{ doble() ? 'Solo teléfono' : 'Vista doble' }}
        </button>
        <button mat-icon-button (click)="recargar()" matTooltip="Recargar el teléfono"><mat-icon>refresh</mat-icon></button>
        <button mat-icon-button (click)="salir()" matTooltip="Salir (Esc)"><mat-icon>close</mat-icon></button>
      </header>

      <div class="mk-escenario">
        <div class="mk-columna-tel">
          <div class="mk-telefono" [style.transform]="'scale(' + escala() + ')'">
            <div class="mk-muesca"></div>
            <iframe class="mk-pantalla" [src]="urlTelefono()" title="App del vendedor en el celular" allow="geolocation; clipboard-write"></iframe>
            <div class="mk-barra-inicio"></div>
          </div>
        </div>
        @if (doble()) {
          <div class="mk-columna-tablero">
            <div class="mk-tablero-cab"><mat-icon>insights</mat-icon> Gerencia · Hoy <small>lo que ve la oficina mientras el vendedor vende</small></div>
            <iframe class="mk-tablero" [src]="urlTablero" title="Centro de control de la gerencia"></iframe>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; }
    .mk { position: fixed; inset: 0; z-index: 999; display: flex; flex-direction: column; background: var(--ceskia-void); color: var(--ceskia-text-primary); }
    .mk-barra { display: flex; align-items: center; gap: 8px; padding: 8px 14px; background: var(--ceskia-surface); border-bottom: 1px solid var(--ceskia-border-subtle); }
    .mk-titulo { display: flex; align-items: center; gap: 6px; font-weight: 600; margin-right: 12px; mat-icon { color: var(--ceskia-accent-primary); } }
    .mk-destinos { display: flex; gap: 2px; button { color: var(--ceskia-text-secondary); mat-icon { margin-right: 4px; } &.activo { color: var(--ceskia-accent-primary); background: var(--ceskia-accent-primary-glow); } } }
    .mk-espacio { flex: 1; }
    .mk-escenario { flex: 1; display: grid; grid-template-columns: 1fr; min-height: 0; }
    .doble .mk-escenario { grid-template-columns: minmax(420px, 34%) 1fr; }
    .mk-columna-tel { display: flex; align-items: center; justify-content: center; padding: 16px; min-height: 0; overflow: hidden; }
    /* Teléfono: 390 × 844 (iPhone 14) en CSS, escalado para entrar en la pantalla. */
    .mk-telefono {
      position: relative; width: 390px; height: 844px; flex: none; transform-origin: center center;
      background: #0b0f19; border-radius: 52px; padding: 14px;
      box-shadow: 0 0 0 10px #1c2333, 0 0 0 12px #3a4457, 0 30px 80px rgba(0,0,0,.6);
    }
    .mk-muesca { position: absolute; top: 14px; left: 50%; transform: translateX(-50%); width: 120px; height: 34px; background: #0b0f19; border-radius: 0 0 20px 20px; z-index: 2; }
    .mk-pantalla { width: 100%; height: 100%; border: none; border-radius: 40px; background: var(--ceskia-base); }
    .mk-barra-inicio { position: absolute; bottom: 22px; left: 50%; transform: translateX(-50%); width: 130px; height: 5px; border-radius: 3px; background: rgba(255,255,255,.55); z-index: 2; pointer-events: none; }
    .mk-columna-tablero { display: flex; flex-direction: column; min-width: 0; min-height: 0; border-left: 1px solid var(--ceskia-border-subtle); }
    .mk-tablero-cab { display: flex; align-items: center; gap: 8px; padding: 8px 14px; font-weight: 600; background: var(--ceskia-surface); border-bottom: 1px solid var(--ceskia-border-subtle); mat-icon { color: var(--ceskia-accent-primary); } small { font-weight: 400; color: var(--ceskia-text-tertiary); margin-left: 6px; } }
    .mk-tablero { flex: 1; width: 100%; border: none; background: var(--ceskia-base); }
  `]
})
export class MarcoCelularComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly sanitizer = inject(DomSanitizer);

  readonly destinos: Destino[] = [
    { label: 'Mi día', icon: 'route', ruta: '/m/agenda' },
    { label: 'Clientes', icon: 'storefront', ruta: '/m/clientes' },
    { label: 'Catálogo', icon: 'inventory_2', ruta: '/m/catalogo' },
    { label: 'Vender', icon: 'add_shopping_cart', ruta: '/m/pedidos/nuevo' },
    { label: 'Pedidos', icon: 'receipt_long', ruta: '/m/pedidos' },
  ];

  readonly doble = signal(true);
  readonly destino = signal('/m/agenda');
  readonly escala = signal(1);
  private readonly version = signal(0);

  readonly urlTelefono = computed<SafeResourceUrl>(() =>
    this.sanitizer.bypassSecurityTrustResourceUrl(`${this.destino()}?modo=marco&v=${this.version()}`));
  readonly urlTablero: SafeResourceUrl = this.sanitizer.bypassSecurityTrustResourceUrl('/?modo=escritorio');

  ngOnInit(): void { this.ajustarEscala(); }

  @HostListener('window:resize')
  ajustarEscala(): void {
    // 844 px de alto más el marco tienen que entrar en la ventana; en vista doble también el ancho.
    const altoDisponible = window.innerHeight - 56 - 32;
    const anchoDisponible = (this.doble() ? window.innerWidth * 0.34 : window.innerWidth) - 48;
    this.escala.set(Math.min(1, altoDisponible / 880, anchoDisponible / 420));
  }

  ir(ruta: string): void { this.destino.set(ruta); }

  alternarDoble(): void { this.doble.update(v => !v); this.ajustarEscala(); }

  recargar(): void { this.version.update(v => v + 1); }

  @HostListener('document:keydown.escape')
  salir(): void { this.router.navigateByUrl('/'); }
}
