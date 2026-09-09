import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { provideNativeDateAdapter } from '@angular/material/core';
import { GeneratedFormBase } from '../../../../../core/components/generated-form.base';

import { MovimientoStock } from '../../../models/movimientostock.model';
import { MovimientoStockService } from '../../../services/movimientostock.service';
import { Variante } from '../../../models/variante.model';
import { VarianteService } from '../../../services/variante.service';
import { Deposito } from '../../../models/deposito.model';
import { DepositoService } from '../../../services/deposito.service';

@Component({
  selector: 'app-movimientostock-form',
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
    MatProgressSpinnerModule,
    MatDatepickerModule
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './movimientostock-form.component.html',
  styleUrl: './movimientostock-form.component.scss'
})
export class MovimientoStockFormComponent extends GeneratedFormBase<MovimientoStock> {
  protected readonly entidad = 'movimientostock';
  protected readonly service = inject(MovimientoStockService);
  private readonly varianteService = inject(VarianteService);
  private readonly depositoService = inject(DepositoService);

  variantes = signal<Variante[]>([]);
  depositos = signal<Deposito[]>([]);

  readonly tipos = ['entrada', 'salida', 'ajuste', 'transferencia'];

  protected construirForm(): FormGroup {
    return this.fb.group({
      varianteId: [this.item?.varianteId || 0, [Validators.required]],
      depositoId: [this.item?.depositoId || 0, [Validators.required]],
      tipo: [this.item?.tipo || 'entrada', [Validators.required]],
      depositoDestinoId: [this.item?.depositoDestinoId ?? null],
      // Etapa 0: el ajuste admite negativo (faltante de inventario); el resto lo valida el dominio.
      cantidad: [this.item?.cantidad ?? 1, [Validators.required, (c: AbstractControl) => Number(c.value) === 0 ? { distintoDeCero: true } : null]],
      costoUnitario: [this.item?.costoUnitario ?? null],
      motivo: [this.item?.motivo || ''],
      documentoOrigen: [this.item?.documentoOrigen || ''],
      fecha: [this.item?.fecha || new Date()],
      usuario: [this.item?.usuario || ''],
    });
  }

  protected override cargarOpciones(): void {
    this.varianteService.getAll().subscribe({
      next: (data: Variante[]) => this.variantes.set(data),
      error: (err: unknown) => console.error('[MovimientoStockForm] Error cargando variantes:', err)
    });
    this.depositoService.getAll().subscribe({
      next: (data: Deposito[]) => this.depositos.set(data),
      error: (err: unknown) => console.error('[MovimientoStockForm] Error cargando depósitos:', err)
    });
  }

  /** El depósito destino solo aplica a transferencias. */
  get esTransferencia(): boolean {
    return this.form?.get('tipo')?.value === 'transferencia';
  }

  etiquetaVariante(v: Variante): string {
    return v.sku + (v.productoDisplay ? ' · ' + v.productoDisplay : '');
  }
}
