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

import { Parametrosla } from '../../../models/parametrosla.model';
import { ParametroslaService } from '../../../services/parametrosla.service';

@Component({
  selector: 'app-parametrosla-form',
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
  templateUrl: './parametrosla-form.component.html',
  styleUrl: './parametrosla-form.component.scss'
})
export class ParametroslaFormComponent extends GeneratedFormBase<Parametrosla> {
  protected readonly entidad = 'parametrosla';
  protected readonly service = inject(ParametroslaService);

  // Opciones para selects de FK

  protected construirForm(): FormGroup {
    return this.fb.group({
      rangoAlertaUmbralAdvertenciaDias: [this.item?.rangoAlertaUmbralAdvertenciaDias || 0, [Validators.required]],
      rangoAlertaLimiteDias: [this.item?.rangoAlertaLimiteDias || 0, [Validators.required]],
    });
  }
}
