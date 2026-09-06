import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../core/services/auth.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule],
  template: `
    <div class="home-container">
      <mat-card class="welcome-card">
        <mat-card-header>
          <mat-icon mat-card-avatar class="welcome-icon">waving_hand</mat-icon>
          <mat-card-title>Bienvenido, {{ userName }}</mat-card-title>
          <mat-card-subtitle>{{ userProfile }}</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <p>Esta es la pagina de inicio de SiteMotos.</p>
          <p>Utilice el menu lateral para navegar a los diferentes modulos.</p>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .home-container {
      padding: 2rem;
    }

    .welcome-card {
      max-width: 600px;
    }

    .welcome-icon {
      font-size: 40px;
      width: 40px;
      height: 40px;
      background: var(--ceskia-accent-primary);
      color: #ffffff;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    mat-card-content {
      padding-top: 1rem;
    }

    mat-card-content p {
      margin: 0.5rem 0;
      color: var(--ceskia-text-secondary);
    }
  `]
})
export class HomeComponent {
  private authService = inject(AuthService);

  get userName(): string {
    const user = this.authService.currentUser();
    return user?.nombreCompleto || user?.nombreUsuario || 'Usuario';
  }

  get userProfile(): string {
    const user = this.authService.currentUser();
    return user?.perfilNombre || '';
  }
}
