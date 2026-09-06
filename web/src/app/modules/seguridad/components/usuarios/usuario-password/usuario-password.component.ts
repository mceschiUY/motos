import { Component, Inject, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { SeguridadService } from '../../../services/seguridad.service';
import { Usuario } from '../../../models/seguridad.model';

export interface UsuarioPasswordData {
  usuario: Usuario;
}

@Component({
  selector: 'app-usuario-password',
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
  templateUrl: './usuario-password.component.html',
  styleUrl: './usuario-password.component.scss'
})
export class UsuarioPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(SeguridadService);

  readonly isLoading = signal(false);
  readonly showPassword = signal(false);
  readonly showConfirm = signal(false);
  readonly form: FormGroup;

  constructor(
    public dialogRef: MatDialogRef<UsuarioPasswordComponent>,
    @Inject(MAT_DIALOG_DATA) public data: UsuarioPasswordData
  ) {
    this.form = this.fb.group({
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  private passwordMatchValidator(control: AbstractControl): { [key: string]: boolean } | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');

    if (password && confirmPassword && password.value !== confirmPassword.value) {
      return { passwordMismatch: true };
    }
    return null;
  }

  togglePassword(): void {
    this.showPassword.set(!this.showPassword());
  }

  toggleConfirm(): void {
    this.showConfirm.set(!this.showConfirm());
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);

    this.service.cambiarPassword({ usuarioId: this.data.usuario.id, nuevaPassword: this.form.value.password }).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.dialogRef.close(true);
      },
      error: (err) => {
        console.error('[UsuarioPassword] Error:', err);
        this.isLoading.set(false);
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
