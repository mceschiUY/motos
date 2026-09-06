import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { RouterModule } from '@angular/router';
import { AlertItem } from '../../models/dashboard.model';

@Component({
  selector: 'app-alerts-panel',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, RouterModule],
  template: `
    <div class="alerts-container">
      @for (alert of alerts; track alert.id) {
        <div class="alert-item" [class]="'alert-' + alert.tipo">
          <div class="alert-icon">
            <mat-icon>
              @switch (alert.tipo) {
                @case ('warning') { warning }
                @case ('error') { error }
                @case ('info') { info }
              }
            </mat-icon>
          </div>
          <div class="alert-content">
            <h4 class="alert-title">{{ alert.titulo }}</h4>
            <p class="alert-message">{{ alert.mensaje }}</p>
          </div>
          @if (alert.accion) {
            <button mat-stroked-button [routerLink]="alert.accion.route" class="alert-action">
              {{ alert.accion.label }}
            </button>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .alerts-container {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .alert-item {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 16px 20px;
      border-radius: 12px;
      border: 1px solid;

      &.alert-warning {
        background: var(--ceskia-accent-warning-glow);
        border-color: var(--ceskia-accent-warning);

        .alert-icon mat-icon { color: var(--ceskia-accent-warning); }
      }

      &.alert-error {
        background: var(--ceskia-accent-danger-glow);
        border-color: var(--ceskia-accent-danger);

        .alert-icon mat-icon { color: var(--ceskia-accent-danger); }
      }

      &.alert-info {
        background: var(--ceskia-accent-info-glow);
        border-color: var(--ceskia-accent-info);

        .alert-icon mat-icon { color: var(--ceskia-accent-info); }
      }
    }

    .alert-icon {
      width: 40px;
      height: 40px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;

      mat-icon {
        font-size: 24px;
        width: 24px;
        height: 24px;
      }
    }

    .alert-content {
      flex: 1;
      min-width: 0;
    }

    .alert-title {
      font-size: 14px;
      font-weight: 600;
      color: var(--ceskia-text-primary);
      margin: 0 0 2px 0;
    }

    .alert-message {
      font-size: 13px;
      color: var(--ceskia-text-secondary);
      margin: 0;
    }

    .alert-action {
      flex-shrink: 0;
    }
  `]
})
export class AlertsPanelComponent {
  @Input() alerts: AlertItem[] = [];
}
