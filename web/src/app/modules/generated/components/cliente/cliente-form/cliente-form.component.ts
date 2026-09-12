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

import { Cliente } from '../../../models/cliente.model';
import { ClienteService } from '../../../services/cliente.service';
import { Vendedor } from '../../../models/vendedor.model';
import { VendedorService } from '../../../services/vendedor.service';

@Component({
  selector: 'app-cliente-form',
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
  templateUrl: './cliente-form.component.html',
  styleUrl: './cliente-form.component.scss'
})
export class ClienteFormComponent extends GeneratedFormBase<Cliente> {
  protected readonly entidad = 'cliente';
  protected readonly service = inject(ClienteService);
  private readonly vendedorService = inject(VendedorService);

  // Enum Tipo (valores persistidos en minúsculas; etiqueta visible por `etiquetasTipo`)
  readonly tipos = ['tienda', 'distribuidor', 'online', 'particular'];
  readonly etiquetasTipo: Record<string, string> =
    { tienda: 'Tienda', distribuidor: 'Distribuidor', online: 'Online', particular: 'Particular' };

  // Opciones para selects de FK
  vendedores = signal<Vendedor[]>([]);

  // Bloque plegable "Ubicación" (lat/long): abierto al editar si ya hay coordenadas.
  readonly ubicacion = signal<boolean>(this.item?.latitud != null || this.item?.longitud != null);

  protected construirForm(): FormGroup {
    return this.fb.group({
      nombre: [this.item?.nombre || '', [Validators.required]],
      telefono: [this.item?.telefono || '', []],
      direccionEntrega: [this.item?.direccionEntrega || '', []],
      // Etapa A (plan §3.2): datos comerciales, todos opcionales.
      tipo: [this.item?.tipo ?? null],
      ciudad: [this.item?.ciudad || ''],
      contacto: [this.item?.contacto || ''],
      email: [this.item?.email || '', [Validators.email]],
      vendedorId: [this.item?.vendedorId ?? null],
      notas: [this.item?.notas || ''],
      latitud: [this.item?.latitud ?? null, [Validators.min(-90), Validators.max(90)]],
      longitud: [this.item?.longitud ?? null, [Validators.min(-180), Validators.max(180)]],
    });
  }

  protected override cargarOpciones(): void {
    this.vendedorService.getAll().subscribe({
      next: (data: Vendedor[]) => this.vendedores.set(data.filter(v => v.activo !== false || v.id === this.item?.vendedorId)),
      error: (err: unknown) => console.error('[ClienteForm] Error cargando vendedores:', err)
    });
  }
}
