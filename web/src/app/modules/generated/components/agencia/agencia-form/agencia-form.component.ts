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
import { provideNativeDateAdapter } from '@angular/material/core';
import { GeneratedFormBase } from '../../../../../core/components/generated-form.base';

import { Agencia } from '../../../models/agencia.model';
import { AgenciaService } from '../../../services/agencia.service';

@Component({
  selector: 'app-agencia-form',
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
  templateUrl: './agencia-form.component.html',
  styleUrl: './agencia-form.component.scss'
})
export class AgenciaFormComponent extends GeneratedFormBase<Agencia> {
  protected readonly entidad = 'agencia';
  protected readonly service = inject(AgenciaService);

  // Opciones para selects de FK

  protected construirForm(): FormGroup {
    return this.fb.group({
      nombre: [this.item?.nombre || '', [Validators.required]],
    });
  }
}
