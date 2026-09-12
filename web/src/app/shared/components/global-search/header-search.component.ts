import { Component, ElementRef, HostListener, inject, signal, viewChild } from '@angular/core';
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
 * Búsqueda global INLINE del cabezal (pedido Pablo 2026-09-06): se tipea directo
 * en la barra y los resultados se despliegan debajo — reemplaza al overlay modal
 * de global-search.component (misma lógica de fan-out a /{entidad}/buscar, otro
 * envase). Ctrl+K ahora enfoca el input; Esc lo limpia y cierra.
 */
@Component({
  selector: 'app-header-search',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  template: `
    <div class="hs-root">
      <div class="hs-input-row">
        <mat-icon>search</mat-icon>
        <input #campo
          type="text"
          placeholder="Buscar en todo el sitio…"
          [ngModel]="texto()"
          (ngModelChange)="onTexto($event)"
          (focus)="abierto.set(true)"
          (keydown.enter)="irAlPrimero()"
          (keydown.escape)="limpiar()"
          autocomplete="off" />
        @if (texto()) {
          <button class="hs-clear" (click)="limpiar()" aria-label="Limpiar búsqueda"><mat-icon>close</mat-icon></button>
        } @else {
          <span class="hs-kbd">Ctrl K</span>
        }
      </div>

      @if (abierto() && texto().trim().length >= 2) {
        <div class="hs-panel">
          @if (buscando()) {
            <div class="hs-estado">Buscando…</div>
          } @else if (grupos().length === 0) {
            <div class="hs-estado">Sin resultados para «{{ texto() }}»</div>
          }
          @for (g of grupos(); track g.entidad.api) {
            <div class="hs-grupo">
              <div class="hs-grupo-titulo">
                <mat-icon>{{ g.entidad.icon }}</mat-icon>
                {{ g.entidad.label }}
                <span class="hs-count">{{ g.items.length }}</span>
              </div>
              @for (item of g.items; track item.id) {
                <div class="hs-item" (click)="ir(g, item)">
                  <span class="hs-item-titulo">{{ tituloDe(g, item) }}</span>
                  <mat-icon class="hs-ir">chevron_right</mat-icon>
                </div>
              }
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    :host { display: block; min-width: 0; }
    .hs-root { position: relative; width: 100%; }

    .hs-input-row {
      display: flex; align-items: center; gap: var(--ceskia-space-2);
      height: 36px; padding: 0 12px 0 10px;
      background: var(--ceskia-surface);
      border: 1px solid var(--ceskia-border-default);
      border-radius: var(--ceskia-radius-full);
      transition: var(--ceskia-transition-fast);
      > mat-icon { font-size: 18px; width: 18px; height: 18px; color: var(--ceskia-text-tertiary); flex-shrink: 0; }
    }
    .hs-root:focus-within .hs-input-row { border-color: var(--ceskia-accent-primary); background: var(--ceskia-base); }

    .hs-input-row input {
      flex: 1; min-width: 0; background: transparent; border: none; outline: none;
      color: var(--ceskia-text-primary); font-family: inherit; font-size: var(--ceskia-text-sm);
    }
    .hs-input-row input::placeholder { color: var(--ceskia-text-tertiary); }

    .hs-kbd { flex-shrink: 0; font-size: 0.65rem; color: var(--ceskia-text-muted); border: 1px solid var(--ceskia-border-default); border-radius: var(--ceskia-radius-sm); padding: 1px 5px; }
    .hs-clear { flex-shrink: 0; display: flex; background: transparent; border: none; cursor: pointer; color: var(--ceskia-text-tertiary); padding: 0;
      mat-icon { font-size: 16px; width: 16px; height: 16px; }
      &:hover { color: var(--ceskia-text-primary); } }

    .hs-panel {
      position: absolute; top: calc(100% + 8px); left: 0; right: 0; z-index: 1002;
      max-height: 62vh; overflow-y: auto;
      background: var(--ceskia-elevated);
      border: 1px solid var(--ceskia-border-default);
      border-radius: var(--ceskia-radius-lg);
      box-shadow: var(--ceskia-shadow-xl);
    }
    .hs-estado { padding: var(--ceskia-space-4); color: var(--ceskia-text-tertiary); font-size: var(--ceskia-text-sm); text-align: center; }
    .hs-grupo { padding: var(--ceskia-space-2) 0; border-bottom: 1px solid var(--ceskia-border-subtle); }
    .hs-grupo:last-child { border-bottom: none; }
    .hs-grupo-titulo { display: flex; align-items: center; gap: var(--ceskia-space-2); padding: var(--ceskia-space-2) var(--ceskia-space-4); font-size: var(--ceskia-text-xs); text-transform: uppercase; letter-spacing: .06em; color: var(--ceskia-text-tertiary);
      mat-icon { font-size: 15px; width: 15px; height: 15px; } }
    .hs-count { background: var(--ceskia-surface); border-radius: var(--ceskia-radius-full); padding: 1px 7px; }
    .hs-item { display: flex; align-items: center; padding: var(--ceskia-space-2) var(--ceskia-space-4); cursor: pointer; }
    .hs-item:hover { background: var(--ceskia-hover); }
    .hs-item-titulo { flex: 1; font-size: var(--ceskia-text-sm); color: var(--ceskia-text-primary); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .hs-ir { font-size: 16px; width: 16px; height: 16px; color: var(--ceskia-text-muted); }
  `]
})
export class HeaderSearchComponent {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly host = inject(ElementRef<HTMLElement>);

  private readonly campo = viewChild<ElementRef<HTMLInputElement>>('campo');

  // Sin las entidades marcadas `oculta` (hijas o de configuración): no son escena del relato.
  readonly entidades = BUSQUEDA_ENTIDADES.filter(e => !e.oculta);
  readonly abierto = signal(false);
  readonly texto = signal('');
  readonly buscando = signal(false);
  readonly grupos = signal<GrupoResultados[]>([]);

  private debounce: ReturnType<typeof setTimeout> | null = null;
  private corridaActual = 0;

  /** Ctrl+K enfoca la barra (antes abría la modal). */
  @HostListener('document:keydown', ['$event'])
  atajos(e: KeyboardEvent): void {
    if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
      e.preventDefault();
      this.campo()?.nativeElement.focus();
    }
  }

  /** Click fuera del componente cierra el desplegable. */
  @HostListener('document:click', ['$event'])
  clickAfuera(e: MouseEvent): void {
    if (!this.host.nativeElement.contains(e.target as Node)) {
      this.abierto.set(false);
    }
  }

  onTexto(v: string): void {
    this.texto.set(v);
    this.abierto.set(true);
    if (this.debounce) { clearTimeout(this.debounce); }
    this.debounce = setTimeout(() => this.buscar(), 250);
  }

  limpiar(): void {
    this.texto.set('');
    this.grupos.set([]);
    this.buscando.set(false);
    this.abierto.set(false);
  }

  private buscar(): void {
    const q = this.texto().trim();
    if (q.length < 2) { this.grupos.set([]); this.buscando.set(false); return; }

    const corrida = ++this.corridaActual;   // descarta respuestas de búsquedas viejas
    this.buscando.set(true);
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
    resultados.sort((a, b) => this.entidades.indexOf(a.entidad) - this.entidades.indexOf(b.entidad));
    this.grupos.set(resultados);
    this.buscando.set(false);
  }

  tituloDe(g: GrupoResultados, item: any): string {
    return item[g.entidad.campoTitulo] || (g.entidad.label + ' #' + item.id);
  }

  ir(g: GrupoResultados, item: any): void {
    this.limpiar();
    this.router.navigate([g.entidad.ruta, item.id]);
  }

  irAlPrimero(): void {
    const g = this.grupos()[0];
    if (g && g.items.length > 0) { this.ir(g, g.items[0]); }
  }
}
