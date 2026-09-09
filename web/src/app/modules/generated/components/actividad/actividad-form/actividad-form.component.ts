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

import { Actividad } from '../../../models/actividad.model';
import { ACTIVIDAD_DESCRIPTOR } from '../../../models/actividad.descriptor';
import { ActividadService } from '../../../services/actividad.service';
import { Cliente } from '../../../models/cliente.model';
import { ClienteService } from '../../../services/cliente.service';
import { Vendedor } from '../../../models/vendedor.model';
import { VendedorService } from '../../../services/vendedor.service';

@Component({
  selector: 'app-actividad-form',
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
  templateUrl: './actividad-form.component.html',
  styleUrl: './actividad-form.component.scss'
})
export class ActividadFormComponent extends GeneratedFormBase<Actividad> {
  protected readonly entidad = 'actividad';
  protected readonly service = inject(ActividadService);
  private readonly clienteService = inject(ClienteService);
  private readonly vendedorService = inject(VendedorService);

  // Enums del descriptor (valores persistidos en minúsculas; etiqueta visible por `etiquetas`)
  readonly tipos = ['visita', 'llamada', 'whatsapp', 'email'];
  readonly resultados = ['pedido', 'sin_pedido', 'reprogramar', 'sin_contacto'];
  private readonly etiquetasResultado: Record<string, string> =
    ACTIVIDAD_DESCRIPTOR.campos.find(c => c.nombre === 'resultado')?.etiquetas ?? {};

  // Opciones para selects de FK
  clientes = signal<Cliente[]>([]);
  vendedores = signal<Vendedor[]>([]);

  // Registro rápido (§4.2): 4 campos visibles; el resto plegado. Al editar se abre todo.
  readonly masDatos = signal<boolean>(this.isEditing);

  etiquetaResultado(valor: string): string {
    return this.etiquetasResultado[valor] ?? valor;
  }

  protected construirForm(): FormGroup {
    return this.fb.group({
      clienteId: [this.item?.clienteId || null, [Validators.required]],
      tipo: [this.item?.tipo || 'visita', [Validators.required]],
      resultado: [this.item?.resultado || 'sin_pedido', [Validators.required]],
      notas: [this.item?.notas || ''],
      vendedorId: [this.item?.vendedorId || null, [Validators.required]],
      fecha: [this.item?.fecha || new Date(), [Validators.required]],
      proximaAccion: [this.item?.proximaAccion || null],
      // Oculto: lo completa el flujo de Pedido (Etapa B); acá solo viaja.
      pedidoId: [this.item?.pedidoId ?? null],
    });
  }

  protected override cargarOpciones(): void {
    this.loadClientes();
    this.loadVendedores();
    this.preseleccionarVendedorMio();
  }

  private loadClientes(): void {
    this.clienteService.getAll().subscribe({
      next: (data: Cliente[]) => this.clientes.set(data),
      error: (err: unknown) => console.error('[ActividadForm] Error cargando clientes:', err)
    });
  }

  private loadVendedores(): void {
    this.vendedorService.getAll().subscribe({
      next: (data: Vendedor[]) => this.vendedores.set(data),
      error: (err: unknown) => console.error('[ActividadForm] Error cargando vendedores:', err)
    });
  }

  // Alta sin vendedor dado: si el usuario logueado es un vendedor, queda "lo mío" preseleccionado.
  private preseleccionarVendedorMio(): void {
    if (this.isEditing || this.form.get('vendedorId')?.value) { return; }
    this.vendedorService.mio().subscribe({
      next: (v: Vendedor) => {
        if (v?.id && !this.form.get('vendedorId')?.value) { this.form.get('vendedorId')?.setValue(v.id); }
      },
      error: () => { /* el usuario no es vendedor: se elige a mano */ }
    });
  }
}
