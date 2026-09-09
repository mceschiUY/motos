import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { GeneratedFormBase } from '../../../../../core/components/generated-form.base';

import { PedidoLinea } from '../../../models/pedidolinea.model';
import { PedidoLineaService } from '../../../services/pedidolinea.service';
import { Pedido } from '../../../models/pedido.model';
import { PedidoService } from '../../../services/pedido.service';
import { Variante } from '../../../models/variante.model';
import { VarianteService } from '../../../services/variante.service';

/**
 * Línea de pedido. El precio se puede dejar vacío: el backend copia el de lista de la
 * variante (plan §3.5, el pedido congela el precio del momento). Solo se listan los
 * pedidos editables — sobre uno despachado el backend rechaza igual, pero mejor no
 * ofrecerlo.
 */
@Component({
  selector: 'app-pedidolinea-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './pedidolinea-form.component.html',
  styleUrl: './pedidolinea-form.component.scss'
})
export class PedidoLineaFormComponent extends GeneratedFormBase<PedidoLinea> {
  protected readonly entidad = 'pedidolinea';
  protected readonly service = inject(PedidoLineaService);
  private readonly pedidoService = inject(PedidoService);
  private readonly varianteService = inject(VarianteService);

  // Opciones para selects de FK
  pedidos = signal<Pedido[]>([]);
  variantes = signal<Variante[]>([]);

  private static readonly EDITABLES = ['borrador', 'confirmado'];

  protected construirForm(): FormGroup {
    return this.fb.group({
      pedidoId: [this.item?.pedidoId || null, [Validators.required]],
      varianteId: [this.item?.varianteId || null, [Validators.required]],
      cantidad: [this.item?.cantidad ?? 1, [Validators.required, Validators.min(0.01)]],
      precioUnitarioUsd: [this.item?.precioUnitarioUsd ?? null, []],
    });
  }

  protected override cargarOpciones(): void {
    this.pedidoService.getAll().subscribe({
      next: (data: Pedido[]) => this.pedidos.set(
        data.filter(p => PedidoLineaFormComponent.EDITABLES.includes(p.estado) || p.id === this.item?.pedidoId)),
      error: (err: unknown) => console.error('[PedidoLineaForm] Error cargando pedidos:', err)
    });
    this.varianteService.getAll().subscribe({
      next: (data: Variante[]) => this.variantes.set(data.filter(v => v.activo !== false)),
      error: (err: unknown) => console.error('[PedidoLineaForm] Error cargando variantes:', err)
    });
  }

  /** Al elegir el SKU se propone su precio de lista (se puede pisar). */
  onVarianteChange(varianteId: number): void {
    const v = this.variantes().find(x => x.id === varianteId) as any;
    if (v?.precioLista != null && !this.form.get('precioUnitarioUsd')?.value) {
      this.form.get('precioUnitarioUsd')?.setValue(v.precioLista);
    }
  }

  etiquetaVariante(v: any): string {
    const partes = [v?.sku, v?.productoDisplay].filter(Boolean);
    return partes.length ? partes.join(' · ') : `#${v?.id}`;
  }
}
