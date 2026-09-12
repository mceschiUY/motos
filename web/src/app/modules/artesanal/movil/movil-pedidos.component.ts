import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { EtiquetaPipe } from '../comun/etiquetas';
import { UsdPipe } from '../comun/usd.pipe';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Observable } from 'rxjs';
import { VendedorService } from '../../generated/services/vendedor.service';
import { PedidoService } from '../../generated/services/pedido.service';
import { Pedido } from '../../generated/models/pedido.model';

const ORDEN_ESTADOS = ['borrador', 'confirmado', 'preparado', 'despachado', 'entregado', 'anulado'];
const LABEL_ESTADO: Record<string, string> = {
  borrador: 'Borradores', confirmado: 'Confirmados', preparado: 'Preparados',
  despachado: 'Despachados', entregado: 'Entregados', anulado: 'Anulados',
};

/**
 * Mis pedidos, para el celular (plan Etapa F4): tarjetas agrupadas por estado, del más
 * reciente al más viejo. Del vendedor logueado si lo hay; si no, todos.
 */
@Component({
  selector: 'app-movil-pedidos',
  standalone: true,
  imports: [UsdPipe, EtiquetaPipe, CommonModule, MatIconModule, MatProgressSpinnerModule],
  template: `
    <div class="mp">
      <h1 class="mp-titulo">Mis pedidos <span class="mp-cant">{{ pedidos().length }}</span></h1>
      <div class="mp-filtros">
        <button class="mp-chip" [class.activo]="estado() === null" (click)="estado.set(null)">Abiertos</button>
        @for (e of estadosPresentes(); track e) {
          <button class="mp-chip" [class.activo]="estado() === e" (click)="estado.set(e)">{{ label(e) }}</button>
        }
      </div>
      @if (cargando()) {
        <div class="mp-cargando"><mat-spinner diameter="28"></mat-spinner></div>
      } @else if (visibles().length === 0) {
        <p class="mp-vacio"><mat-icon>receipt_long</mat-icon> No hay pedidos acá.</p>
      } @else {
        <div class="mp-lista">
          @for (p of visibles(); track p.id) {
            <button class="mp-card" (click)="abrir(p)">
              <span class="mp-fila">
                <span class="mp-numero">{{ p.numero }}</span>
                <span class="enum-pill" [attr.data-valor]="p.estado">{{ p.estado | etiqueta }}</span>
              </span>
              <span class="mp-cliente">{{ p.clienteDisplay || 'Cliente ' + p.clienteId }}</span>
              <span class="mp-fila mp-pie">
                <span class="mp-fecha">{{ p.fecha | date:'d MMM' }}</span>
                <span class="mp-total">{{ p.totalUsd | usd }}</span>
              </span>
            </button>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .mp-titulo { font-size: var(--ceskia-text-xl); margin: 4px 0 12px; display: flex; align-items: baseline; gap: 8px; }
    .mp-cant { font-size: var(--ceskia-text-sm); color: var(--ceskia-text-tertiary); font-weight: 400; }
    .mp-filtros { display: flex; gap: 6px; overflow-x: auto; padding-bottom: 8px; margin-bottom: 8px; scrollbar-width: none; }
    .mp-chip {
      flex: none; padding: 6px 12px; border-radius: var(--ceskia-radius-full); font-size: var(--ceskia-text-sm);
      border: 1px solid var(--ceskia-border-default); background: var(--ceskia-surface); color: var(--ceskia-text-secondary); cursor: pointer;
    }
    .mp-chip.activo { border-color: var(--ceskia-accent-primary); color: var(--ceskia-accent-primary); background: var(--ceskia-accent-primary-glow); }
    .mp-lista { display: flex; flex-direction: column; gap: 8px; }
    .mp-card {
      display: flex; flex-direction: column; gap: 4px; width: 100%; text-align: left; padding: 12px;
      border: 1px solid var(--ceskia-border-subtle); border-radius: var(--ceskia-radius-lg);
      background: var(--ceskia-elevated); color: var(--ceskia-text-primary); cursor: pointer;
    }
    .mp-fila { display: flex; align-items: center; justify-content: space-between; gap: 8px; }
    .mp-numero { font-family: var(--ceskia-font-mono); font-weight: var(--ceskia-font-semibold); }
    .mp-cliente { font-size: var(--ceskia-text-sm); }
    .mp-pie { font-size: var(--ceskia-text-xs); color: var(--ceskia-text-tertiary); }
    .mp-total { font-weight: var(--ceskia-font-semibold); color: var(--ceskia-text-primary); font-variant-numeric: tabular-nums; }
    .mp-cargando { display: flex; justify-content: center; padding: 32px; }
    .mp-vacio { display: flex; align-items: center; gap: 8px; color: var(--ceskia-text-tertiary); }
  `]
})
export class MovilPedidosComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly vendedores = inject(VendedorService);
  private readonly pedidosSvc = inject(PedidoService);

  readonly cargando = signal(true);
  readonly pedidos = signal<Pedido[]>([]);
  /** null = abiertos (todo lo que no está entregado ni anulado). */
  readonly estado = signal<string | null>(null);

  readonly estadosPresentes = computed(() =>
    ORDEN_ESTADOS.filter(e => this.pedidos().some(p => p.estado === e)));

  readonly visibles = computed(() => {
    const e = this.estado();
    const l = [...this.pedidos()].sort((a, b) => String(b.fecha).localeCompare(String(a.fecha)));
    return e ? l.filter(p => p.estado === e) : l.filter(p => p.estado !== 'entregado' && p.estado !== 'anulado');
  });

  ngOnInit(): void {
    this.vendedores.mio().subscribe({
      next: v => this.cargar(this.pedidosSvc.getByVendedorId(v.id)),
      error: () => this.cargar(this.pedidosSvc.getAll()),
    });
  }

  private cargar(fuente: Observable<Pedido[]>): void {
    fuente.subscribe({
      next: ps => { this.pedidos.set(ps ?? []); this.cargando.set(false); },
      error: () => { this.pedidos.set([]); this.cargando.set(false); },
    });
  }

  label(e: string): string { return LABEL_ESTADO[e] ?? e; }
  abrir(p: Pedido): void { this.router.navigate(['/m/pedido', p.id]); }
}
