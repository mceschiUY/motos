import { Component, HostListener, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { environment } from '../../../../environments/environment';
import { BUSQUEDA_ENTIDADES, BusquedaEntidad } from '../../../modules/generated/generated-search.registry';

interface GrupoResultados {
  entidad: BusquedaEntidad;
  items: any[];
}

/**
 * Búsqueda global (Ctrl+K): un solo buscador para todas las entidades del producto.
 * QUÉ se busca lo define el manifiesto generado (generated-search.registry.ts) —
 * este componente es el mismo para todos los productos.
 */
@Component({
  selector: 'app-global-search',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  template: `
    @if (abierto()) {
      <div class="gs-backdrop" (click)="cerrar()">
        <div class="gs-panel" (click)="$event.stopPropagation()">
          <div class="gs-input-row">
            <mat-icon>search</mat-icon>
            <input
              id="gs-input"
              type="text"
              placeholder="Buscar en todo el sistema…"
              [ngModel]="texto()"
              (ngModelChange)="onTexto($event)"
              (keydown.enter)="irAlPrimero()"
              autocomplete="off" />
            <span class="gs-kbd">Esc</span>
          </div>

          @if (buscando()) {
            <div class="gs-estado">Buscando…</div>
          } @else if (texto().trim().length >= 2 && grupos().length === 0) {
            <div class="gs-estado">Sin resultados para "{{ texto() }}"</div>
          } @else if (texto().trim().length < 2) {
            <div class="gs-estado gs-ayuda">Escribí al menos 2 letras — busca en {{ entidades.length }} tipos de registro</div>
          }

          <div class="gs-resultados">
            @for (g of grupos(); track g.entidad.api) {
              <div class="gs-grupo">
                <div class="gs-grupo-titulo">
                  <mat-icon>{{ g.entidad.icon }}</mat-icon>
                  {{ g.entidad.label }}
                  <span class="gs-count">{{ g.items.length }}</span>
                </div>
                @for (item of g.items; track item.id) {
                  <div class="gs-item" (click)="ir(g, item)">
                    <span class="gs-item-titulo">{{ tituloDe(g, item) }}</span>
                    <mat-icon class="gs-ir">chevron_right</mat-icon>
                  </div>
                }
              </div>
            }
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .gs-backdrop { position: fixed; inset: 0; background: rgba(6, 10, 18, .6); backdrop-filter: blur(2px); z-index: 1000; display: flex; justify-content: center; padding-top: 12vh; }
    .gs-panel { width: 640px; max-width: 92vw; max-height: 64vh; display: flex; flex-direction: column; background: var(--ceskia-elevated); border: 1px solid var(--ceskia-border-default); border-radius: var(--ceskia-radius-lg); box-shadow: var(--ceskia-shadow-xl); overflow: hidden; }
    .gs-input-row { display: flex; align-items: center; gap: var(--ceskia-space-3); padding: var(--ceskia-space-4) var(--ceskia-space-5); border-bottom: 1px solid var(--ceskia-border-subtle); mat-icon { color: var(--ceskia-text-tertiary); } }
    .gs-input-row input { flex: 1; background: transparent; border: none; outline: none; color: var(--ceskia-text-primary); font-size: var(--ceskia-text-lg); }
    .gs-kbd { font-size: var(--ceskia-text-xs); color: var(--ceskia-text-muted); border: 1px solid var(--ceskia-border-default); border-radius: var(--ceskia-radius-sm); padding: 2px 6px; }
    .gs-estado { padding: var(--ceskia-space-5); color: var(--ceskia-text-tertiary); font-size: var(--ceskia-text-sm); text-align: center; }
    .gs-ayuda { color: var(--ceskia-text-muted); }
    .gs-resultados { overflow-y: auto; }
    .gs-grupo { padding: var(--ceskia-space-2) 0; border-bottom: 1px solid var(--ceskia-border-subtle); }
    .gs-grupo:last-child { border-bottom: none; }
    .gs-grupo-titulo { display: flex; align-items: center; gap: var(--ceskia-space-2); padding: var(--ceskia-space-2) var(--ceskia-space-5); font-size: var(--ceskia-text-xs); text-transform: uppercase; letter-spacing: .06em; color: var(--ceskia-text-tertiary); mat-icon { font-size: 15px; width: 15px; height: 15px; } }
    .gs-count { background: var(--ceskia-surface); border-radius: var(--ceskia-radius-full); padding: 1px 7px; }
    .gs-item { display: flex; align-items: center; padding: var(--ceskia-space-2) var(--ceskia-space-5); cursor: pointer; }
    .gs-item:hover { background: var(--ceskia-hover); }
    .gs-item-titulo { flex: 1; font-size: var(--ceskia-text-sm); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .gs-ir { font-size: 16px; width: 16px; height: 16px; color: var(--ceskia-text-muted); }
  `]
})
export class GlobalSearchComponent {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  readonly entidades = BUSQUEDA_ENTIDADES;
  readonly abierto = signal(false);
  readonly texto = signal('');
  readonly buscando = signal(false);
  readonly grupos = signal<GrupoResultados[]>([]);

  private debounce: ReturnType<typeof setTimeout> | null = null;
  private corridaActual = 0;

  @HostListener('document:keydown', ['$event'])
  atajos(e: KeyboardEvent): void {
    if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
      e.preventDefault();
      this.abierto() ? this.cerrar() : this.abrir();
    }
    if (e.key === 'Escape' && this.abierto()) {
      this.cerrar();
    }
  }

  abrir(): void {
    this.abierto.set(true);
    setTimeout(() => document.getElementById('gs-input')?.focus(), 60);
  }

  cerrar(): void {
    this.abierto.set(false);
    this.texto.set('');
    this.grupos.set([]);
    this.buscando.set(false);
  }

  onTexto(v: string): void {
    this.texto.set(v);
    if (this.debounce) { clearTimeout(this.debounce); }
    this.debounce = setTimeout(() => this.buscar(), 250);
  }

  private buscar(): void {
    const q = this.texto().trim();
    if (q.length < 2) { this.grupos.set([]); this.buscando.set(false); return; }

    const corrida = ++this.corridaActual;   // descarta respuestas de búsquedas viejas
    this.buscando.set(true);
    this.grupos.set([]);
    let pendientes = this.entidades.length;
    if (pendientes === 0) { this.buscando.set(false); return; }

    const resultados: GrupoResultados[] = [];
    for (const e of this.entidades) {
      this.http.get<any[]>(`${environment.apiUrl}/${e.api}/buscar`, { params: { q } }).subscribe({
        next: (items) => {
          if (corrida !== this.corridaActual) { return; }
          if (items && items.length > 0) { resultados.push({ entidad: e, items }); }
          if (--pendientes === 0) { this.terminar(resultados, corrida); }
        },
        error: () => {
          if (corrida !== this.corridaActual) { return; }
          if (--pendientes === 0) { this.terminar(resultados, corrida); }
        }
      });
    }
  }

  private terminar(resultados: GrupoResultados[], corrida: number): void {
    if (corrida !== this.corridaActual) { return; }
    // orden estable: el del manifiesto
    resultados.sort((a, b) => this.entidades.indexOf(a.entidad) - this.entidades.indexOf(b.entidad));
    this.grupos.set(resultados);
    this.buscando.set(false);
  }

  tituloDe(g: GrupoResultados, item: any): string {
    return item[g.entidad.campoTitulo] || (g.entidad.label + ' #' + item.id);
  }

  ir(g: GrupoResultados, item: any): void {
    this.cerrar();
    this.router.navigate([g.entidad.ruta, item.id]);
  }

  irAlPrimero(): void {
    const g = this.grupos()[0];
    if (g && g.items.length > 0) { this.ir(g, g.items[0]); }
  }
}
