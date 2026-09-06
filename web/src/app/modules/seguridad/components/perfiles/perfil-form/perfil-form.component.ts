import { Component, Inject, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { SeguridadService } from '../../../services/seguridad.service';
import { Perfil } from '../../../models/seguridad.model';

export interface PerfilFormData {
  perfil: Perfil | null;
}

@Component({
  selector: 'app-perfil-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './perfil-form.component.html',
  styleUrl: './perfil-form.component.scss'
})
export class PerfilFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(SeguridadService);

  readonly isLoading = signal(false);
  readonly isEdit: boolean;
  readonly form: FormGroup;
  readonly totalFields = 2;

  constructor(
    public dialogRef: MatDialogRef<PerfilFormComponent>,
    @Inject(MAT_DIALOG_DATA) public data: PerfilFormData
  ) {
    this.isEdit = !!data.perfil;

    this.form = this.fb.group({
      nombre: [data.perfil?.nombre || '', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      descripcion: [data.perfil?.descripcion || '', [Validators.maxLength(500)]]
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

  getErrorMessage(field: string): string {
    const control = this.form.get(field);
    if (!control) return '';
    if (control.hasError('required')) return 'Este campo es requerido';
    if (control.hasError('minlength')) return `Minimo ${control.getError('minlength').requiredLength} caracteres`;
    if (control.hasError('maxlength')) return `Maximo ${control.getError('maxlength').requiredLength} caracteres`;
    return '';
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);

    const formData = {
      nombre: this.form.value.nombre.trim(),
      descripcion: this.form.value.descripcion?.trim() || ''
    };

    if (this.isEdit) {
      this.service.modificarPerfil(this.data.perfil!.id, formData).subscribe({
        next: () => {
          this.isLoading.set(false);
          this.dialogRef.close(true);
        },
        error: (err: Error) => {
          console.error('[PerfilForm] Error:', err);
          this.isLoading.set(false);
        }
      });
    } else {
      this.service.crearPerfil(formData).subscribe({
        next: () => {
          this.isLoading.set(false);
          this.dialogRef.close(true);
        },
        error: (err: Error) => {
          console.error('[PerfilForm] Error:', err);
          this.isLoading.set(false);
        }
      });
    }
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
