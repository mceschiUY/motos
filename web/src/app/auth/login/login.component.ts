import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../core/services/auth.service';

const GRID_CHARS = '01アイウエオ∑∆Ω∇⟨⟩λμπσ';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})

export class LoginComponent {
  credentials = { usuario: '', password: '' };
  rememberMe = false;

  hidePassword = signal(true);
  loading = signal(false);
  error = signal<string | null>(null);

  readonly gridChars: string[] = Array.from({ length: 300 }, () =>
    GRID_CHARS[Math.floor(Math.random() * GRID_CHARS.length)]
  );

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  onLogin(): void {
    // El bypass de dev (pablo/pablo) vive en la API, que emite un JWT REAL solo en
    // Development. El atajo viejo del front creaba sesión SIN token → con [Authorize]
    // activo todo daba 401. Ahora todo login pasa por el camino real.
    this.loading.set(true);
    this.error.set(null);

    this.authService.login(this.credentials).subscribe({
      next: () => {
        const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.error?.message || 'Error al iniciar sesion. Verifica tus credenciales.');
      }
    });
  }
}
