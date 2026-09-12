import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MovimientoStockService } from '../../generated/services/movimientostock.service';
import { MovimientoStock } from '../../generated/models/movimientostock.model';

/**
 * Reemplaza la ficha generada de MovimientoStock (`/movimientostock/:id`): una fila del Kardex
 * sin contexto no cuenta nada. Carga el movimiento y manda a la ficha del SKU con la fila
 * resaltada (`/variante/:varianteId?movimientoId=:id`). Si no existe, a Existencias.
 */
@Component({
  selector: 'app-movimiento-redirect',
  standalone: true,
  imports: [MatProgressSpinnerModule],
  template: `
    <div class="mov-redirect">
      <mat-spinner diameter="36"></mat-spinner>
      <span>Buscando el SKU del movimiento…</span>
    </div>
  `,
  styles: [`
    .mov-redirect {
      display: flex; align-items: center; justify-content: center; gap: var(--ceskia-space-3);
      padding: var(--ceskia-space-10); color: var(--ceskia-text-tertiary); font-size: var(--ceskia-text-sm);
    }
  `],
})
export class MovimientoRedirectComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly movimientos = inject(MovimientoStockService);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id') ?? 0);
    if (!id) { this.aExistencias(); return; }
    this.movimientos.getById(id).subscribe({
      next: (m: MovimientoStock | undefined) => {
        if (!m?.varianteId) { this.aExistencias(); return; }
        this.router.navigate(['/variante', m.varianteId], { queryParams: { movimientoId: id }, replaceUrl: true });
      },
      error: () => this.aExistencias(),
    });
  }

  private aExistencias(): void {
    this.router.navigate(['/existencias'], { replaceUrl: true });
  }
}
