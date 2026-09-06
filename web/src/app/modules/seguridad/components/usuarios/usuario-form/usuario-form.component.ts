import { Component, Inject, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { SeguridadService } from '../../../services/seguridad.service';
import { Usuario, Perfil } from '../../../models/seguridad.model';

export interface UsuarioFormData {
  usuario: Usuario | null;
}

@Component({
  selector: 'app-usuario-form',
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
    MatSelectModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './usuario-form.component.html',
  styleUrl: './usuario-form.component.scss'
})
export class UsuarioFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(SeguridadService);

  readonly isLoading = signal(false);
  readonly isLoadingPerfiles = signal(true);
  readonly perfiles = signal<Perfil[]>([]);
  readonly isEdit: boolean;
  readonly form: FormGroup;
  readonly totalFields: number;

  constructor(
    public dialogRef: MatDialogRef<UsuarioFormComponent>,
    @Inject(MAT_DIALOG_DATA) public data: UsuarioFormData
  ) {
    this.isEdit = !!data.usuario;
    this.totalFields = this.isEdit ? 4 : 5;

    this.form = this.fb.group({
      nombreCompleto: [data.usuario?.nombreCompleto || '', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      email: [data.usuario?.email || '', [Validators.required, Validators.email]],
      userName: [data.usuario?.userName || '', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
      perfilId: [data.usuario?.perfilId || null, [Validators.required]],
      password: ['', this.isEdit ? [] : [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit(): void {
    this.loadPerfiles();
  }

  private loadPerfiles(): void {
    this.isLoadingPerfiles.set(true);
    this.service.getPerfiles().subscribe({
      next: (data) => {
        this.perfiles.set(data.filter(p => p.activo));
        this.isLoadingPerfiles.set(false);
      },
      error: (err) => {
        console.error('[UsuarioForm] Error cargando perfiles:', err);
        this.isLoadingPerfiles.set(false);
      }
    });
  }

  get validFieldsCount(): number {
    let count = 0;
    const controls = this.form.controls;
    Object.keys(controls).forEach(key => {
      if (this.isEdit && key === 'password') {
        return; // en edición la contrasena no se muestra ni cuenta
      }
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
    if (control.hasError('email')) return 'Email invalido';
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

    const formData: any = {
      nombreCompleto: this.form.value.nombreCompleto.trim(),
      email: this.form.value.email.trim(),
      userName: this.form.value.userName.trim(),
      perfilId: this.form.value.perfilId
    };

    if (!this.isEdit && this.form.value.password) {
      formData.password = this.form.value.password;
    }

    if (this.isEdit) {
      formData.id = this.data.usuario!.id;
    }

    if (this.isEdit) {
      this.service.modificarUsuario(formData).subscribe({
        next: () => {
          this.isLoading.set(false);
          this.dialogRef.close(true);
        },
        error: (err: Error) => {
          console.error('[UsuarioForm] Error:', err);
          this.isLoading.set(false);
        }
      });
    } else {
      this.service.crearUsuario(formData).subscribe({
        next: () => {
          this.isLoading.set(false);
          this.dialogRef.close(true);
        },
        error: (err: Error) => {
          console.error('[UsuarioForm] Error:', err);
          this.isLoading.set(false);
        }
      });
    }
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
