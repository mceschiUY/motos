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
import { provideNativeDateAdapter } from '@angular/material/core';
import { GeneratedFormBase } from '../../../../../core/components/generated-form.base';

import { Meta } from '../../../models/meta.model';
import { MetaService } from '../../../services/meta.service';
import { Vendedor } from '../../../models/vendedor.model';
import { VendedorService } from '../../../services/vendedor.service';

/** Período mensual: AAAA-MM (mes 01..12). */
export const PERIODO_REGEX = /^\d{4}-(0[1-9]|1[0-2])$/;

@Component({
  selector: 'app-meta-form',
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
  templateUrl: './meta-form.component.html',
  styleUrl: './meta-form.component.scss'
})
export class MetaFormComponent extends GeneratedFormBase<Meta> {
  protected readonly entidad = 'meta';
  protected readonly service = inject(MetaService);
  private readonly vendedorService = inject(VendedorService);

  // Opciones para selects de FK
  vendedores = signal<Vendedor[]>([]);

  protected construirForm(): FormGroup {
    return this.fb.group({
      vendedorId: [this.item?.vendedorId || null, [Validators.required]],
      periodo: [this.item?.periodo || this.mesActual(), [Validators.required, Validators.pattern(PERIODO_REGEX)]],
      objetivoUsd: [this.item?.objetivoUsd ?? null, [Validators.required, Validators.min(1)]],
    });
  }

  protected override cargarOpciones(): void {
    this.vendedorService.getAll().subscribe({
      next: (data: Vendedor[]) => this.vendedores.set(data),
      error: (err: unknown) => console.error('[MetaForm] Error cargando vendedores:', err)
    });
  }

  private mesActual(): string {
    const d = new Date();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
  }

  // El core no traduce `pattern`: el mensaje del período vive acá.
  errorPeriodo(): string {
    const c = this.form.get('periodo');
    if (c?.hasError('pattern')) { return 'Formato AAAA-MM (ej: 2026-09)'; }
    return this.getErrorMessage('periodo');
  }
}
