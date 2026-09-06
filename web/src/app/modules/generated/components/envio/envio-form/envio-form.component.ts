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

import { Envio } from '../../../models/envio.model';
import { EnvioService } from '../../../services/envio.service';
import { Cliente } from '../../../models/cliente.model';
import { ClienteService } from '../../../services/cliente.service';
import { Agencia } from '../../../models/agencia.model';
import { AgenciaService } from '../../../services/agencia.service';

@Component({
  selector: 'app-envio-form',
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
  templateUrl: './envio-form.component.html',
  styleUrl: './envio-form.component.scss'
})
export class EnvioFormComponent extends GeneratedFormBase<Envio> {
  protected readonly entidad = 'envio';
  protected readonly service = inject(EnvioService);
  private readonly clienteService = inject(ClienteService);
  private readonly agenciaService = inject(AgenciaService);

  // Opciones para selects de FK
  clientes = signal<Cliente[]>([]);
  agencias = signal<Agencia[]>([]);

  protected construirForm(): FormGroup {
    return this.fb.group({
      codigoRastreo: [this.item?.codigoRastreo || '', [Validators.required]],
      fechaRecibido: [this.item?.fechaRecibido || new Date(), [Validators.required]],
      fechaFactura: [this.item?.fechaFactura || new Date(), []],
      fechaEnvio: [this.item?.fechaEnvio || new Date(), []],
      fechaEntrega: [this.item?.fechaEntrega || new Date(), []],
      motivoAnulacion: [this.item?.motivoAnulacion || '', []],
      clienteId: [this.item?.clienteId || 0, [Validators.required]],
      agenciaId: [this.item?.agenciaId || 0, [Validators.required]],
    });
  }

  protected override cargarOpciones(): void {
    this.loadClientes();
    this.loadAgencias();
  }

  private loadClientes(): void {
    this.clienteService.getAll().subscribe({
      next: (data: Cliente[]) => this.clientes.set(data),
      error: (err: unknown) => console.error('[EnvioForm] Error cargando clientes:', err)
    });
  }

  private loadAgencias(): void {
    this.agenciaService.getAll().subscribe({
      next: (data: Agencia[]) => this.agencias.set(data),
      error: (err: unknown) => console.error('[EnvioForm] Error cargando agencias:', err)
    });
  }
}
