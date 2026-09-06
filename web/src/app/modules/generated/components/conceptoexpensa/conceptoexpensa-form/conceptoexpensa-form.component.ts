import { Component, Inject, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

import { Conceptoexpensa } from '../../../models/conceptoexpensa.model';
import { ConceptoexpensaService } from '../../../services/conceptoexpensa.service';

@Component({
  selector: 'app-conceptoexpensa-form',
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
    MatSnackBarModule,
    MatSlideToggleModule
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './conceptoexpensa-form.component.html',
  styleUrl: './conceptoexpensa-form.component.scss'
})
export class ConceptoexpensaFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<ConceptoexpensaFormComponent>);
  private readonly service = inject(ConceptoexpensaService);
  private readonly snackBar = inject(MatSnackBar);

  form!: FormGroup;
  isEditing = false;
  isSaving = false;
  readonly totalFields = 4;

  // Opciones para selects de FK

  item: Conceptoexpensa | null = null;

  constructor(@Inject(MAT_DIALOG_DATA) public data: { item?: Conceptoexpensa; mode?: string } | null) {
    this.item = data?.item || null;
    this.isEditing = !!this.item;
  }

  ngOnInit(): void {
    this.initForm();
  }


  private initForm(): void {
    this.form = this.fb.group({
      nombre: [this.item?.nombre || '', [Validators.required]],
      descripcion: [this.item?.descripcion || '', [Validators.required]],
      tipo: [this.item?.tipo || '', [Validators.required]],
      activo: [this.item?.activo || false, [Validators.required]],
    });
  }

  get validFieldsCount(): number {
    let count = 0;
    const controls = this.form.controls;
    Object.keys(controls).forEach(key => {
      if (controls[key].valid && controls[key].value !== null && controls[key].value !== '') {
        count++;
      }
    });
    return count;
  }

  get formProgress(): number {
    return (this.validFieldsCount / this.totalFields) * 100;
  }

  isFieldValid(field: string): boolean {
    const control = this.form.get(field);
    return control ? control.valid && control.value !== null && control.value !== '' : false;
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onSave(): void {
    if (this.form.invalid) {
      Object.keys(this.form.controls).forEach(key => {
        this.form.get(key)?.markAsTouched();
      });
      return;
    }

    this.isSaving = true;
    const formData = this.form.value;

    const operation = this.isEditing && this.item
      ? this.service.update(this.item.id, { ...formData, id: this.item.id })
      : this.service.create(formData);

    operation.subscribe({
      next: () => {
        this.isSaving = false;
        this.dialogRef.close(true);
      },
      error: (err: unknown) => {
        console.error('[ConceptoexpensaForm] Error guardando:', err);
        this.isSaving = false;
        this.snackBar.open('Error al guardar. Intenta nuevamente.', 'Cerrar', {
          duration: 5000,
          panelClass: ['notification-error']
        });
      }
    });
  }

  getErrorMessage(field: string): string {
    const control = this.form.get(field);
    if (!control) return '';
    if (control.hasError('required')) return 'Este campo es requerido';
    return '';
  }
}
