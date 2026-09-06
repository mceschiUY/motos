import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuditLog, getActionLabel, getActionIcon, getActionColor } from '../../models/auditoria.model';

@Component({
  selector: 'app-audit-log-detail',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
    MatTooltipModule
  ],
  template: `
    <div class="dialog-container">
      <!-- Header -->
      <div class="dialog-header">
        <div class="header-icon" [class]="getActionColor(log.action)">
          <mat-icon>{{ getActionIcon(log.action) }}</mat-icon>
        </div>
        <div class="header-text">
          <h2>{{ getActionLabel(log.action) }} - {{ log.entityType }}</h2>
          <span class="timestamp">{{ formatDate(log.timestamp) }}</span>
        </div>
        <button mat-icon-button (click)="close()" class="close-btn">
          <mat-icon>close</mat-icon>
        </button>
      </div>

      <!-- Content -->
      <mat-dialog-content>
        <!-- Info Cards -->
        <div class="info-grid">
          <div class="info-card">
            <span class="info-label">Usuario</span>
            <span class="info-value">
              <mat-icon>person</mat-icon>
              {{ log.userName || 'Anonimo' }}
            </span>
          </div>

          <div class="info-card">
            <span class="info-label">Entidad</span>
            <span class="info-value entity">
              {{ log.entityType }}
              @if (log.entityId) {
                <span class="entity-id">#{{ log.entityId }}</span>
              }
            </span>
          </div>

          <div class="info-card">
            <span class="info-label">Resultado</span>
            <span class="info-value">
              @if (log.success) {
                <span class="status success">
                  <mat-icon>check_circle</mat-icon>
                  Exitoso
                </span>
              } @else {
                <span class="status error">
                  <mat-icon>error</mat-icon>
                  Fallido
                </span>
              }
            </span>
          </div>

          <div class="info-card">
            <span class="info-label">Duracion</span>
            <span class="info-value mono">{{ formatDuration(log.durationMs) }}</span>
          </div>

          <div class="info-card">
            <span class="info-label">IP</span>
            <span class="info-value mono">{{ log.ipAddress || '-' }}</span>
          </div>

          <div class="info-card">
            <span class="info-label">Request</span>
            <span class="info-value mono small">{{ log.requestPath || '-' }}</span>
          </div>
        </div>

        <!-- Error Message -->
        @if (log.errorMessage) {
          <div class="error-box">
            <mat-icon>error</mat-icon>
            <span>{{ log.errorMessage }}</span>
          </div>
        }

        <!-- Tabs para valores -->
        @if (log.oldValues || log.newValues) {
          <mat-tab-group class="values-tabs">
            @if (log.newValues) {
              <mat-tab label="Valores Nuevos">
                <div class="json-container">
                  <pre class="json-content">{{ formatJson(log.newValues) }}</pre>
                </div>
              </mat-tab>
            }
            @if (log.oldValues) {
              <mat-tab label="Valores Anteriores">
                <div class="json-container">
                  <pre class="json-content">{{ formatJson(log.oldValues) }}</pre>
                </div>
              </mat-tab>
            }
            @if (log.oldValues && log.newValues) {
              <mat-tab label="Comparacion">
                <div class="diff-container">
                  <div class="diff-column old">
                    <h4>Antes</h4>
                    <pre>{{ formatJson(log.oldValues) }}</pre>
                  </div>
                  <div class="diff-column new">
                    <h4>Despues</h4>
                    <pre>{{ formatJson(log.newValues) }}</pre>
                  </div>
                </div>
              </mat-tab>
            }
          </mat-tab-group>
        }

        <!-- User Agent -->
        @if (log.userAgent) {
          <div class="user-agent-section">
            <span class="section-label">User Agent</span>
            <span class="user-agent-value">{{ log.userAgent }}</span>
          </div>
        }
      </mat-dialog-content>

      <!-- Actions -->
      <mat-dialog-actions align="end">
        <button mat-button (click)="close()">Cerrar</button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .dialog-container {
      background: var(--ceskia-elevated);
      color: var(--ceskia-text-primary);
    }

    .dialog-header {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 20px 24px;
      border-bottom: 1px solid var(--ceskia-border-default);
      position: relative;

      /* Ícono tonal: fondo glow (~10%) + ícono del acento, sin gradientes */
      .header-icon {
        width: 48px;
        height: 48px;
        border-radius: var(--ceskia-radius-xl);
        display: flex;
        align-items: center;
        justify-content: center;

        mat-icon {
          font-size: 24px;
          width: 24px;
          height: 24px;
        }

        &.accent-green { background: var(--ceskia-accent-success-glow); mat-icon { color: var(--ceskia-accent-success); } }
        &.accent-blue { background: var(--ceskia-accent-primary-glow); mat-icon { color: var(--ceskia-accent-primary); } }
        &.accent-red { background: var(--ceskia-accent-danger-glow); mat-icon { color: var(--ceskia-accent-danger); } }
        &.accent-purple { background: var(--ceskia-accent-primary-glow); mat-icon { color: var(--ceskia-accent-primary); } }
      }

      .header-text {
        flex: 1;

        h2 {
          margin: 0;
          font-size: var(--ceskia-text-lg);
          font-weight: var(--ceskia-font-semibold);
        }

        .timestamp {
          font-size: var(--ceskia-text-sm);
          color: var(--ceskia-text-secondary);
        }
      }

      .close-btn {
        position: absolute;
        top: 12px;
        right: 12px;
      }
    }

    mat-dialog-content {
      padding: 24px !important;
      max-height: 60vh;
    }

    .info-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 16px;
      margin-bottom: 24px;

      .info-card {
        background: var(--ceskia-base);
        border: 1px solid var(--ceskia-border-subtle);
        border-radius: var(--ceskia-radius-lg);
        padding: 12px;

        .info-label {
          display: block;
          font-size: 11px;
          text-transform: uppercase;
          color: var(--ceskia-text-tertiary);
          margin-bottom: 4px;
        }

        .info-value {
          display: flex;
          align-items: center;
          gap: 6px;
          font-size: var(--ceskia-text-base);

          mat-icon {
            font-size: 16px;
            width: 16px;
            height: 16px;
            color: var(--ceskia-text-secondary);
          }

          &.mono {
            font-family: var(--ceskia-font-mono);
            font-size: var(--ceskia-text-sm);
          }

          &.small {
            font-size: 11px;
            word-break: break-all;
          }

          &.entity {
            color: var(--ceskia-accent-primary);

            .entity-id {
              color: var(--ceskia-text-secondary);
              font-family: var(--ceskia-font-mono);
              font-size: var(--ceskia-text-xs);
            }
          }

          .status {
            display: inline-flex;
            align-items: center;
            gap: 4px;
            padding: 2px 8px;
            border-radius: var(--ceskia-radius-xl);
            font-size: var(--ceskia-text-xs);

            &.success {
              background: var(--ceskia-accent-success-glow);
              color: var(--ceskia-accent-success);
            }

            &.error {
              background: var(--ceskia-accent-danger-glow);
              color: var(--ceskia-accent-danger);
            }
          }
        }
      }
    }

    .error-box {
      display: flex;
      align-items: flex-start;
      gap: 12px;
      padding: 16px;
      background: var(--ceskia-accent-danger-glow);
      border: 1px solid var(--ceskia-accent-danger);
      border-radius: var(--ceskia-radius-lg);
      margin-bottom: 24px;

      mat-icon {
        color: var(--ceskia-accent-danger);
        flex-shrink: 0;
      }

      span {
        font-size: var(--ceskia-text-base);
        color: var(--ceskia-accent-danger);
        word-break: break-word;
      }
    }

    .values-tabs {
      margin-bottom: 24px;

      ::ng-deep {
        .mat-mdc-tab-header {
          background: var(--ceskia-base);
          border-radius: var(--ceskia-radius-lg) var(--ceskia-radius-lg) 0 0;
        }

        .mat-mdc-tab-body-wrapper {
          background: var(--ceskia-base);
          border: 1px solid var(--ceskia-border-default);
          border-top: none;
          border-radius: 0 0 var(--ceskia-radius-lg) var(--ceskia-radius-lg);
        }
      }
    }

    .json-container {
      padding: 16px;
    }

    .json-content {
      margin: 0;
      padding: 16px;
      background: var(--ceskia-surface);
      border-radius: var(--ceskia-radius-lg);
      font-family: var(--ceskia-font-mono);
      font-size: var(--ceskia-text-xs);
      color: var(--ceskia-text-primary);
      overflow-x: auto;
      white-space: pre-wrap;
      word-break: break-word;
    }

    .diff-container {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
      padding: 16px;

      .diff-column {
        h4 {
          margin: 0 0 8px 0;
          font-size: var(--ceskia-text-xs);
          text-transform: uppercase;
          color: var(--ceskia-text-tertiary);
        }

        pre {
          margin: 0;
          padding: 12px;
          background: var(--ceskia-surface);
          border-radius: var(--ceskia-radius-lg);
          font-family: var(--ceskia-font-mono);
          font-size: 11px;
          overflow-x: auto;
          white-space: pre-wrap;
          word-break: break-word;
        }

        &.old pre {
          border-left: 3px solid var(--ceskia-accent-danger);
        }

        &.new pre {
          border-left: 3px solid var(--ceskia-accent-success);
        }
      }
    }

    .user-agent-section {
      .section-label {
        display: block;
        font-size: 11px;
        text-transform: uppercase;
        color: var(--ceskia-text-tertiary);
        margin-bottom: 8px;
      }

      .user-agent-value {
        display: block;
        font-size: var(--ceskia-text-xs);
        color: var(--ceskia-text-secondary);
        font-family: var(--ceskia-font-mono);
        word-break: break-all;
      }
    }

    mat-dialog-actions {
      padding: 16px 24px;
      border-top: 1px solid var(--ceskia-border-default);
    }
  `]
})
export class AuditLogDetailComponent {
  private readonly dialogRef = inject(MatDialogRef<AuditLogDetailComponent>);
  readonly data = inject<{ log: AuditLog }>(MAT_DIALOG_DATA);

  get log(): AuditLog {
    return this.data.log;
  }

  getActionLabel = getActionLabel;
  getActionIcon = getActionIcon;
  getActionColor = getActionColor;

  formatDate(date: Date | string): string {
    const d = new Date(date);
    return d.toLocaleDateString('es-AR', {
      weekday: 'long',
      day: '2-digit',
      month: 'long',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit'
    });
  }

  formatDuration(ms: number | null): string {
    if (ms === null) return '-';
    if (ms < 1000) return `${ms}ms`;
    return `${(ms / 1000).toFixed(2)}s`;
  }

  formatJson(jsonString: string | null): string {
    if (!jsonString) return '';
    try {
      const parsed = JSON.parse(jsonString);
      return JSON.stringify(parsed, null, 2);
    } catch {
      return jsonString;
    }
  }

  close(): void {
    this.dialogRef.close();
  }
}
