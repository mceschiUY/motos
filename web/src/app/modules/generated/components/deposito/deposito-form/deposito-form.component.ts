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

import { Deposito } from '../../../models/deposito.model';
import { DepositoService } from '../../../services/deposito.service';

@Component({
  selector: 'app-deposito-form',
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
  templateUrl: './deposito-form.component.html',
  styleUrl: './deposito-form.component.scss'
})
export class DepositoFormComponent extends GeneratedFormBase<Deposito> {
  protected readonly entidad = 'deposito';
  protected readonly service = inject(DepositoService);

  protected construirForm(): FormGroup {
    return this.fb.group({
      codigo: [this.item?.codigo || '', [Validators.required]],
      nombre: [this.item?.nombre || '', [Validators.required]],
      direccion: [this.item?.direccion || ''],
      activo: [this.item?.activo ?? true],
    });
  }
}
