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

import { Observacion } from '../../../models/observacion.model';
import { ObservacionService } from '../../../services/observacion.service';
import { Envio } from '../../../models/envio.model';
import { EnvioService } from '../../../services/envio.service';

@Component({
  selector: 'app-observacion-form',
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
  templateUrl: './observacion-form.component.html',
  styleUrl: './observacion-form.component.scss'
})
export class ObservacionFormComponent extends GeneratedFormBase<Observacion> {
  protected readonly entidad = 'observacion';
  protected readonly service = inject(ObservacionService);
  private readonly envioService = inject(EnvioService);

  // Opciones para selects de FK
  envios = signal<Envio[]>([]);

  protected construirForm(): FormGroup {
    return this.fb.group({
      texto: [this.item?.texto || '', [Validators.required]],
      fechaHora: [this.item?.fechaHora || new Date(), [Validators.required]],
      usuario: [this.item?.usuario || '', [Validators.required]],
      envioId: [this.item?.envioId || 0, [Validators.required]],
    });
  }

  protected override cargarOpciones(): void {
    this.loadEnvios();
  }

  private loadEnvios(): void {
    this.envioService.getAll().subscribe({
      next: (data: Envio[]) => this.envios.set(data),
      error: (err: unknown) => console.error('[ObservacionForm] Error cargando envios:', err)
    });
  }
}
