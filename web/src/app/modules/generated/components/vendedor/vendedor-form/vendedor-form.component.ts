import { Component, inject } from '@angular/core';
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

import { Vendedor } from '../../../models/vendedor.model';
import { VendedorService } from '../../../services/vendedor.service';

@Component({
  selector: 'app-vendedor-form',
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
  templateUrl: './vendedor-form.component.html',
  styleUrl: './vendedor-form.component.scss'
})
export class VendedorFormComponent extends GeneratedFormBase<Vendedor> {
  protected readonly entidad = 'vendedor';
  protected readonly service = inject(VendedorService);

  protected construirForm(): FormGroup {
    return this.fb.group({
      nombre: [this.item?.nombre || '', [Validators.required]],
      zona: [this.item?.zona || ''],
      comisionPorcentaje: [this.item?.comisionPorcentaje ?? 0, [Validators.required, Validators.min(0), Validators.max(100)]],
      telefono: [this.item?.telefono || ''],
      email: [this.item?.email || '', [Validators.email]],
      usuario: [this.item?.usuario || ''],
      activo: [this.item?.activo ?? true],
    });
  }
}
