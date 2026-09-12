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

import { Producto } from '../../../models/producto.model';
import { ProductoService } from '../../../services/producto.service';
import { Marca } from '../../../models/marca.model';
import { MarcaService } from '../../../services/marca.service';
import { Categoria } from '../../../models/categoria.model';
import { CategoriaService } from '../../../services/categoria.service';

@Component({
  selector: 'app-producto-form',
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
  templateUrl: './producto-form.component.html',
  styleUrl: './producto-form.component.scss'
})
export class ProductoFormComponent extends GeneratedFormBase<Producto> {
  protected readonly entidad = 'producto';
  protected readonly service = inject(ProductoService);
  private readonly marcaService = inject(MarcaService);
  private readonly categoriaService = inject(CategoriaService);

  // Opciones para selects de FK
  marcas = signal<Marca[]>([]);
  categorias = signal<Categoria[]>([]);

  readonly generos = ['unisex', 'hombre', 'mujer', 'nino'];
  readonly tiposCasco = ['integral', 'modular', 'jet', 'cross', 'na'];
  readonly homologaciones = ['ece2206', 'dot', 'snell', 'na'];

  protected construirForm(): FormGroup {
    return this.fb.group({
      codigo: [this.item?.codigo || '', [Validators.required]],
      nombre: [this.item?.nombre || '', [Validators.required]],
      marcaId: [this.item?.marcaId || 0, [Validators.required]],
      categoriaId: [this.item?.categoriaId || 0, [Validators.required]],
      genero: [this.item?.genero || 'unisex', [Validators.required]],
      descripcion: [this.item?.descripcion || ''],
      // Etapa C — catálogo premium. imagenPrincipalId no tiene UI acá (la portada se elige
      // desde la galería de la ficha), pero viaja en el form para que editar no la borre.
      fichaTecnica: [this.item?.fichaTecnica || ''],
      destacado: [this.item?.destacado ?? false],
      novedad: [this.item?.novedad ?? false],
      imagenPrincipalId: [this.item?.imagenPrincipalId ?? null],
      temporada: [this.item?.temporada || ''],
      material: [this.item?.material || ''],
      pesoGramos: [this.item?.pesoGramos ?? null],
      tipoCasco: [this.item?.tipoCasco ?? null],
      homologacion: [this.item?.homologacion ?? null],
      homologacionVigente: [this.item?.homologacionVigente ?? false],
      fechaVencHomologacion: [this.item?.fechaVencHomologacion ?? null],
      activo: [this.item?.activo ?? true],
    });
  }

  /**
   * Etapa 0 (2026-09-07): los atributos de casco (tipo, homologación, vencimiento, vigencia)
   * solo se muestran si la categoría elegida es "Casco" o una hija directa (Integral, Modular…).
   * Comparación sin tildes ni mayúsculas. Al ocultar no se limpian los valores ya cargados.
   */
  esCasco(): boolean {
    const id = Number(this.form?.get('categoriaId')?.value);
    if (!id) return false;
    const cats = this.categorias();
    const cat = cats.find(c => c.id === id);
    if (!cat) return false;
    const norm = (s: string | null | undefined) => (s ?? '').normalize('NFD').replace(/\p{M}/gu, '').trim().toLowerCase();
    if (norm(cat.nombre) === 'casco') return true;
    const padre = cat.categoriaPadreId ? cats.find(c => c.id === cat.categoriaPadreId) : undefined;
    return norm(padre?.nombre) === 'casco';
  }

  protected override cargarOpciones(): void {
    this.marcaService.getAll().subscribe({
      next: (data: Marca[]) => this.marcas.set(data),
      error: (err: unknown) => console.error('[ProductoForm] Error cargando marcas:', err)
    });
    this.categoriaService.getAll().subscribe({
      next: (data: Categoria[]) => this.categorias.set(data),
      error: (err: unknown) => console.error('[ProductoForm] Error cargando categorias:', err)
    });
  }
}
