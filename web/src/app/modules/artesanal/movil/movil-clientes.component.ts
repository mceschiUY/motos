import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { EtiquetaPipe } from '../comun/etiquetas';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { VendedorService } from '../../generated/services/vendedor.service';
import { ClienteService } from '../../generated/services/cliente.service';
import { VendedorPanelService } from '../vendedor-panel/vendedor-panel.service';

interface ClienteMovil {
  id: number;
  nombre: string;
  tipo: string | null;
  ciudad: string | null;
  diasSinVisita: number | null;
  semaforo: 'verde' | 'amarillo' | 'rojo' | null;
}

/**
 * Mis clientes, para el celular (plan Etapa F4): tarjetas con semáforo y búsqueda. Si el usuario
 * logueado es vendedor (`GET api/Vendedor/mio`) usa su panel (clientes asignados con semáforo);
 * si no, la lista completa de clientes sin semáforo.
 */
@Component({
  selector: 'app-movil-clientes',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatProgressSpinnerModule, EtiquetaPipe],
  template: `
    <div class="mc">
      <h1 class="mc-titulo">Mis clientes <span class="mc-cant">{{ filtrados().length }}</span></h1>
      <label class="mc-buscar">
        <mat-icon>search</mat-icon>
        <input type="search" placeholder="Nombre o ciudad" [ngModel]="q()" (ngModelChange)="q.set($event)">
      </label>
      @if (cargando()) {
        <div class="mc-cargando"><mat-spinner diameter="28"></mat-spinner></div>
      } @else if (filtrados().length === 0) {
        <p class="mc-vacio"><mat-icon>person_off</mat-icon> Sin clientes para mostrar.</p>
      } @else {
        <div class="mc-lista">
          @for (c of filtrados(); track c.id) {
            <button class="mc-card" [attr.data-semaforo]="c.semaforo" (click)="abrir(c)">
              <span class="mc-punto"></span>
              <span class="mc-texto">
                <span class="mc-nombre">{{ c.nombre }}</span>
                <span class="mc-detalle">{{ c.ciudad || 'Sin ciudad' }}@if (c.tipo) { · {{ c.tipo | etiqueta }}}</span>
              </span>
              <span class="mc-dias">
                @if (c.diasSinVisita == null) { nunca } @else if (c.diasSinVisita === 0) { hoy } @else { hace {{ c.diasSinVisita }} d }
              </span>
              <mat-icon class="mc-flecha">chevron_right</mat-icon>
            </button>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .mc-titulo { font-size: var(--ceskia-text-xl); margin: 4px 0 12px; display: flex; align-items: baseline; gap: 8px; }
    .mc-cant { font-size: var(--ceskia-text-sm); color: var(--ceskia-text-tertiary); font-weight: 400; }
    .mc-buscar {
      display: flex; align-items: center; gap: 8px; padding: 8px 12px; margin-bottom: 12px;
      background: var(--ceskia-surface); border: 1px solid var(--ceskia-border-default); border-radius: var(--ceskia-radius-lg);
    }
    .mc-buscar mat-icon { color: var(--ceskia-text-tertiary); }
    .mc-buscar input { flex: 1; min-width: 0; border: none; background: transparent; color: var(--ceskia-text-primary); font-size: 16px; outline: none; }
    .mc-lista { display: flex; flex-direction: column; gap: 8px; }
    .mc-card {
      display: grid; grid-template-columns: 12px minmax(0, 1fr) auto 24px; align-items: center; gap: 10px;
      width: 100%; text-align: left; padding: 12px; border: 1px solid var(--ceskia-border-subtle); border-radius: var(--ceskia-radius-lg);
      background: var(--ceskia-elevated); color: var(--ceskia-text-primary); cursor: pointer;
    }
    .mc-punto { width: 10px; height: 10px; border-radius: 50%; background: var(--ceskia-border-strong); }
    .mc-card[data-semaforo='verde'] .mc-punto { background: var(--ceskia-accent-success); }
    .mc-card[data-semaforo='amarillo'] .mc-punto { background: var(--ceskia-accent-warning); }
    .mc-card[data-semaforo='rojo'] .mc-punto { background: var(--ceskia-accent-danger); }
    .mc-texto { display: flex; flex-direction: column; min-width: 0; }
    .mc-nombre { font-weight: var(--ceskia-font-semibold); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .mc-detalle { font-size: var(--ceskia-text-xs); color: var(--ceskia-text-tertiary); text-transform: capitalize; }
    .mc-dias { font-size: var(--ceskia-text-xs); color: var(--ceskia-text-tertiary); white-space: nowrap; font-variant-numeric: tabular-nums; }
    .mc-flecha { color: var(--ceskia-text-muted); }
    .mc-cargando { display: flex; justify-content: center; padding: 32px; }
    .mc-vacio { display: flex; align-items: center; gap: 8px; color: var(--ceskia-text-tertiary); }
  `]
})
export class MovilClientesComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly vendedores = inject(VendedorService);
  private readonly clientes = inject(ClienteService);
  private readonly panel = inject(VendedorPanelService);

  readonly cargando = signal(true);
  readonly q = signal('');
  readonly lista = signal<ClienteMovil[]>([]);
  readonly filtrados = computed(() => {
    const t = this.q().trim().toLowerCase();
    const l = this.lista();
    return t ? l.filter(c => c.nombre.toLowerCase().includes(t) || (c.ciudad ?? '').toLowerCase().includes(t)) : l;
  });

  ngOnInit(): void {
    this.vendedores.mio().subscribe({
      next: v => this.panel.panel(v.id).subscribe({
        next: p => {
          this.lista.set(p.clientes.map(c => ({
            id: c.id, nombre: c.nombre ?? '', tipo: c.tipo, ciudad: c.ciudad,
            diasSinVisita: c.diasSinVisita, semaforo: c.semaforo as ClienteMovil['semaforo'],
          })));
          this.cargando.set(false);
        },
        error: () => this.todos(),
      }),
      error: () => this.todos(),
    });
  }

  private todos(): void {
    this.clientes.getAll().subscribe({
      next: cs => {
        this.lista.set(cs.map(c => ({ id: c.id, nombre: c.nombre, tipo: c.tipo ?? null, ciudad: c.ciudad ?? null, diasSinVisita: null, semaforo: null })));
        this.cargando.set(false);
      },
      error: () => { this.lista.set([]); this.cargando.set(false); },
    });
  }

  abrir(c: ClienteMovil): void { this.router.navigate(['/m/cliente', c.id]); }
}
