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
import { MatCheckboxModule } from '@angular/material/checkbox';
import { provideNativeDateAdapter } from '@angular/material/core';
import { GeneratedFormBase } from '../../../../../core/components/generated-form.base';

import { Talla } from '../../../models/talla.model';
import { TallaService } from '../../../services/talla.service';

@Component({
  selector: 'app-talla-form',
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
    MatCheckboxModule
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './talla-form.component.html',
  styleUrl: './talla-form.component.scss'
})
export class TallaFormComponent extends GeneratedFormBase<Talla> {
  protected readonly entidad = 'talla';
  protected readonly service = inject(TallaService);

  protected construirForm(): FormGroup {
    return this.fb.group({
      nombre: [this.item?.nombre || '', [Validators.required]],
      tipo: [this.item?.tipo || 'alfabetica', [Validators.required]],
      orden: [this.item?.orden ?? 0],
      activo: [this.item?.activo ?? true],
    });
  }
}
