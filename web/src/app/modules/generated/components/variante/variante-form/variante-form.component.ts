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
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { provideNativeDateAdapter } from '@angular/material/core';
import { GeneratedFormBase } from '../../../../../core/components/generated-form.base';

import { Variante } from '../../../models/variante.model';
import { VarianteService } from '../../../services/variante.service';
import { Producto } from '../../../models/producto.model';
import { ProductoService } from '../../../services/producto.service';
import { Talla } from '../../../models/talla.model';
import { TallaService } from '../../../services/talla.service';
import { Color } from '../../../models/color.model';
import { ColorService } from '../../../services/color.service';

@Component({
  selector: 'app-variante-form',
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
    MatDatepickerModule,
    MatSlideToggleModule
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './variante-form.component.html',
  styleUrl: './variante-form.component.scss'
})
export class VarianteFormComponent extends GeneratedFormBase<Variante> {
  protected readonly entidad = 'variante';
  protected readonly service = inject(VarianteService);
  private readonly productoService = inject(ProductoService);
  private readonly tallaService = inject(TallaService);
  private readonly colorService = inject(ColorService);

  // Opciones para selects de FK
  productos = signal<Producto[]>([]);
  tallas = signal<Talla[]>([]);
  colores = signal<Color[]>([]);

  protected construirForm(): FormGroup {
    // Al crear desde la ficha de un producto, el FK lo fija GeneratedFormBase por contextoFk.
    return this.fb.group({
      productoId: [this.item?.productoId || 0, [Validators.required]],
      tallaId: [this.item?.tallaId ?? null],
      colorId: [this.item?.colorId ?? null],
      sku: [this.item?.sku || '', [Validators.required]],
      codigoBarras: [this.item?.codigoBarras || ''],
      costoEstandar: [this.item?.costoEstandar ?? 0, [Validators.min(0)]],
      precioLista: [this.item?.precioLista ?? 0, [Validators.min(0)]],
      activo: [this.item?.activo ?? true],
    });
  }

  protected override cargarOpciones(): void {
    this.productoService.getAll().subscribe({
      next: (data: Producto[]) => this.productos.set(data),
      error: (err: unknown) => console.error('[VarianteForm] Error cargando productos:', err)
    });
    this.tallaService.getAll().subscribe({
      next: (data: Talla[]) => this.tallas.set(data),
      error: (err: unknown) => console.error('[VarianteForm] Error cargando tallas:', err)
    });
    this.colorService.getAll().subscribe({
      next: (data: Color[]) => this.colores.set(data),
      error: (err: unknown) => console.error('[VarianteForm] Error cargando colores:', err)
    });
  }
}
