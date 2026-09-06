import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LoadingService } from '../../services/loading.service';

/**
 * Componente de Loading Overlay Global
 * Se muestra automaticamente cuando hay peticiones HTTP activas.
 * Debe agregarse en app.component.html
 */
@Component({
  selector: 'app-loading-overlay',
  standalone: true,
  imports: [CommonModule, MatProgressSpinnerModule],
  template: `
    @if (loadingService.showLoading()) {
      <div class="loading-overlay">
        <div class="loading-content">
          <mat-spinner diameter="48"></mat-spinner>
          <span class="loading-text">Cargando...</span>
        </div>
      </div>
    }
  `,
  styles: [`
    .loading-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(6, 10, 18, 0.6);
      backdrop-filter: blur(2px);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 9999;
      animation: fadeIn 0.15s ease-out;
    }

    .loading-content {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 16px;
      padding: 32px;
      background: var(--ceskia-elevated);
      border: 1px solid var(--ceskia-border-default);
      border-radius: var(--ceskia-radius-xl);
      box-shadow: var(--ceskia-shadow-lg);
    }

    .loading-text {
      color: var(--ceskia-text-secondary);
      font-size: 14px;
      font-weight: 500;
    }

    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }
  `]
})
export class LoadingOverlayComponent {
  readonly loadingService = inject(LoadingService);
}
