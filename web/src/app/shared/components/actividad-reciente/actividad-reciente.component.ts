import { Component, Input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { ActividadService, EventoNegocio } from '../../../core/services/actividad.service';

/**
 * "Recién pasó": el feed vivo del negocio. Cada línea es un evento de auditoría
 * traducido a lenguaje de negocio; click → la Ficha 360 del registro.
 */
@Component({
  selector: 'app-actividad-reciente',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    @if (visibles().length === 0) {
      <div class="ar-vacio">Todavía no pasó nada — cuando el equipo trabaje, lo vas a ver acá</div>
    } @else {
      <div class="ar-lista">
        @for (e of visibles(); track $index) {
          <div class="ar-evento" [class.clickeable]="e.id !== null" (click)="ir(e)">
            <span class="ar-dot"></span>
            <span class="ar-frase"><strong>{{ e.quien }}</strong> {{ e.frase }}</span>
            <span class="ar-cuando">{{ hace(e.fecha) }}</span>
          </div>
        }
      </div>
    }
  `,
  styles: [`
    .ar-vacio { color: var(--ceskia-text-muted); font-size: var(--ceskia-text-sm); padding: var(--ceskia-space-4) 0; }
    .ar-lista { display: flex; flex-direction: column; gap: var(--ceskia-space-2); }
    .ar-evento { display: flex; align-items: baseline; gap: var(--ceskia-space-3); padding: var(--ceskia-space-1) 0; border-radius: var(--ceskia-radius-sm); }
    .ar-evento.clickeable { cursor: pointer; }
    .ar-evento.clickeable:hover { background: var(--ceskia-hover); }
    .ar-dot { width: 7px; height: 7px; border-radius: 50%; background: var(--ceskia-accent-info); flex-shrink: 0; align-self: center; }
    .ar-frase { flex: 1; font-size: var(--ceskia-text-sm); min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; strong { font-weight: var(--ceskia-font-semibold); } }
    .ar-cuando { font-size: var(--ceskia-text-xs); color: var(--ceskia-text-muted); white-space: nowrap; }
  `]
})
export class ActividadRecienteComponent implements OnInit {
  private readonly router = inject(Router);
  readonly actividad = inject(ActividadService);

  @Input() limite = 12;

  ngOnInit(): void {
    this.actividad.cargar();
    // Ver el feed ES ponerse al día: apaga el punto de novedades
    setTimeout(() => this.actividad.marcarVisto(), 1500);
  }

  visibles(): EventoNegocio[] {
    return this.actividad.eventos().slice(0, this.limite);
  }

  ir(e: EventoNegocio): void {
    if (e.id !== null) { this.router.navigate([e.ruta, e.id]); }
  }

  hace(fecha: string): string {
    const ms = Date.now() - new Date(fecha).getTime();
    const min = Math.floor(ms / 60000);
    if (min < 1) { return 'recién'; }
    if (min < 60) { return `hace ${min} min`; }
    const h = Math.floor(min / 60);
    if (h < 24) { return `hace ${h} h`; }
    const d = Math.floor(h / 24);
    if (d === 1) { return 'ayer'; }
    if (d < 7) { return `hace ${d} días`; }
    return new Date(fecha).toLocaleDateString('es-UY');
  }
}
