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

import { Pedido } from '../../../models/pedido.model';
import { PedidoService } from '../../../services/pedido.service';
import { Cliente } from '../../../models/cliente.model';
import { ClienteService } from '../../../services/cliente.service';
import { Vendedor } from '../../../models/vendedor.model';
import { VendedorService } from '../../../services/vendedor.service';
import { Deposito } from '../../../models/deposito.model';
import { DepositoService } from '../../../services/deposito.service';
import { Agencia } from '../../../models/agencia.model';
import { AgenciaService } from '../../../services/agencia.service';

/**
 * Alta y edición de la CABECERA del pedido. Número, estado, total y comisión no están:
 * los pone el backend (PedidoHooks y las líneas). La agencia es opcional acá porque se
 * puede elegir después, pero sin ella el pedido no se puede despachar.
 */
@Component({
  selector: 'app-pedido-form',
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
  templateUrl: './pedido-form.component.html',
  styleUrl: './pedido-form.component.scss'
})
export class PedidoFormComponent extends GeneratedFormBase<Pedido> {
  protected readonly entidad = 'pedido';
  protected readonly service = inject(PedidoService);
  private readonly clienteService = inject(ClienteService);
  private readonly vendedorService = inject(VendedorService);
  private readonly depositoService = inject(DepositoService);
  private readonly agenciaService = inject(AgenciaService);

  // Opciones para selects de FK
  clientes = signal<Cliente[]>([]);
  vendedores = signal<Vendedor[]>([]);
  depositos = signal<Deposito[]>([]);
  agencias = signal<Agencia[]>([]);

  protected construirForm(): FormGroup {
    return this.fb.group({
      clienteId: [this.item?.clienteId || 0, [Validators.required]],
      vendedorId: [this.item?.vendedorId || 0, [Validators.required]],
      depositoId: [this.item?.depositoId || 0, [Validators.required]],
      agenciaId: [this.item?.agenciaId ?? null, []],
      fecha: [this.item?.fecha || new Date(), [Validators.required]],
      observaciones: [this.item?.observaciones || '', []],
    });
  }

  protected override cargarOpciones(): void {
    this.clienteService.getAll().subscribe({
      next: (data: Cliente[]) => {
        this.clientes.set(data);
        this.preseleccionarVendedorDelCliente(data);
      },
      error: (err: unknown) => console.error('[PedidoForm] Error cargando clientes:', err)
    });
    this.vendedorService.getAll().subscribe({
      next: (data: Vendedor[]) => this.vendedores.set(data.filter(v => v.activo !== false)),
      error: (err: unknown) => console.error('[PedidoForm] Error cargando vendedores:', err)
    });
    this.depositoService.getAll().subscribe({
      next: (data: Deposito[]) => this.depositos.set(data),
      error: (err: unknown) => console.error('[PedidoForm] Error cargando depósitos:', err)
    });
    this.agenciaService.getAll().subscribe({
      next: (data: Agencia[]) => this.agencias.set(data),
      error: (err: unknown) => console.error('[PedidoForm] Error cargando agencias:', err)
    });
  }

  /** Al elegir cliente en un alta, el vendedor asignado se propone solo (se puede cambiar). */
  onClienteChange(clienteId: number): void {
    if (this.isEditing) { return; }
    const cliente = this.clientes().find(c => c.id === clienteId) as any;
    if (cliente?.vendedorId) { this.form.get('vendedorId')?.setValue(cliente.vendedorId); }
  }

  private preseleccionarVendedorDelCliente(clientes: Cliente[]): void {
    const clienteId = this.form.get('clienteId')?.value;
    if (this.isEditing || !clienteId) { return; }
    const cliente = clientes.find(c => c.id === clienteId) as any;
    if (cliente?.vendedorId) { this.form.get('vendedorId')?.setValue(cliente.vendedorId); }
  }
}
