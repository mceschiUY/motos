import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { RouterModule } from '@angular/router';
import { ReportesService } from '../../services/reportes.service';
import { ReporteHistorial, ReporteEstadisticas } from '../../models/reportes.model';
import { ExportDialogComponent } from '../export-dialog/export-dialog.component';

@Component({
  selector: 'app-reportes-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    RouterModule
  ],
  template: `
    <div class="reportes-dashboard">
      <div class="header">
        <h1>Reportes y Exportaciones</h1>
        <button mat-raised-button color="primary" (click)="abrirExportDialog()">
          <mat-icon>download</mat-icon>
          Nueva Exportacion
        </button>
      </div>

      <!-- Estadisticas -->
      <div class="stats-cards">
        @if (estadisticas()) {
          <mat-card class="stat-card">
            <mat-card-content>
              <div class="stat-value">{{ estadisticas()!.totalGenerados }}</div>
              <div class="stat-label">Total Generados</div>
            </mat-card-content>
          </mat-card>

          <mat-card class="stat-card success">
            <mat-card-content>
              <div class="stat-value">{{ estadisticas()!.exitosos }}</div>
              <div class="stat-label">Exitosos</div>
            </mat-card-content>
          </mat-card>

          <mat-card class="stat-card error">
            <mat-card-content>
              <div class="stat-value">{{ estadisticas()!.fallidos }}</div>
              <div class="stat-label">Fallidos</div>
            </mat-card-content>
          </mat-card>

          <mat-card class="stat-card">
            <mat-card-content>
              <div class="stat-value">{{ formatBytes(estadisticas()!.totalBytes) }}</div>
              <div class="stat-label">Datos Exportados</div>
            </mat-card-content>
          </mat-card>
        } @else {
          <mat-spinner diameter="40"></mat-spinner>
        }
      </div>

      <!-- Ultimos reportes -->
      <mat-card class="historial-card">
        <mat-card-header>
          <mat-card-title>Ultimos Reportes</mat-card-title>
          <a mat-button color="primary" routerLink="historial">Ver todos</a>
        </mat-card-header>
        <mat-card-content>
          @if (loading()) {
            <div class="loading">
              <mat-spinner diameter="40"></mat-spinner>
            </div>
          } @else {
            <table mat-table [dataSource]="historial()" class="historial-table">
              <ng-container matColumnDef="entidad">
                <th mat-header-cell *matHeaderCellDef>Entidad</th>
                <td mat-cell *matCellDef="let item">{{ item.entidad || '-' }}</td>
              </ng-container>

              <ng-container matColumnDef="formato">
                <th mat-header-cell *matHeaderCellDef>Formato</th>
                <td mat-cell *matCellDef="let item">
                  <mat-chip [class]="'formato-' + item.formato">
                    {{ item.formato.toUpperCase() }}
                  </mat-chip>
                </td>
              </ng-container>

              <ng-container matColumnDef="registros">
                <th mat-header-cell *matHeaderCellDef>Registros</th>
                <td mat-cell *matCellDef="let item">{{ item.registros | number }}</td>
              </ng-container>

              <ng-container matColumnDef="estado">
                <th mat-header-cell *matHeaderCellDef>Estado</th>
                <td mat-cell *matCellDef="let item">
                  <mat-chip [class]="'estado-' + item.estado.toLowerCase()">
                    {{ item.estado }}
                  </mat-chip>
                </td>
              </ng-container>

              <ng-container matColumnDef="fecha">
                <th mat-header-cell *matHeaderCellDef>Fecha</th>
                <td mat-cell *matCellDef="let item">
                  {{ item.fechaGeneracion | date:'dd/MM/yyyy HH:mm' }}
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
            </table>
          }
        </mat-card-content>
      </mat-card>

      <!-- Accesos rapidos -->
      <div class="quick-actions">
        <mat-card class="action-card" routerLink="historial">
          <mat-icon>history</mat-icon>
          <span>Historial Completo</span>
        </mat-card>

        <mat-card class="action-card" routerLink="programados">
          <mat-icon>schedule</mat-icon>
          <span>Reportes Programados</span>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .reportes-dashboard {
      padding: 24px;
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;

      h1 {
        margin: 0;
        font-size: 24px;
        font-weight: 500;
      }
    }

    .stats-cards {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 16px;
      margin-bottom: 24px;
    }

    .stat-card {
      text-align: center;

      .stat-value {
        font-size: 32px;
        font-weight: 600;
        color: var(--ceskia-accent-primary);
      }

      .stat-label {
        color: var(--ceskia-text-secondary);
        margin-top: 4px;
      }

      &.success .stat-value {
        color: var(--ceskia-accent-success);
      }

      &.error .stat-value {
        color: var(--ceskia-accent-danger);
      }
    }

    .historial-card {
      margin-bottom: 24px;

      mat-card-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
      }
    }

    .historial-table {
      width: 100%;
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: 40px;
    }

    .quick-actions {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 16px;
    }

    .action-card {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 24px;
      cursor: pointer;
      transition: transform 0.2s, box-shadow 0.2s;

      &:hover {
        transform: translateY(-2px);
        box-shadow: var(--ceskia-shadow-md);
      }

      mat-icon {
        font-size: 48px;
        width: 48px;
        height: 48px;
        color: var(--ceskia-accent-primary);
        margin-bottom: 12px;
      }

      span {
        font-weight: 500;
      }
    }

    .estado-completado {
      background-color: var(--ceskia-accent-success-glow) !important;
      color: var(--ceskia-accent-success) !important;
    }

    .estado-error {
      background-color: var(--ceskia-accent-danger-glow) !important;
      color: var(--ceskia-accent-danger) !important;
    }

    .estado-enproceso {
      background-color: var(--ceskia-accent-warning-glow) !important;
      color: var(--ceskia-accent-warning) !important;
    }

    .formato-excel {
      background-color: var(--ceskia-accent-primary-glow) !important;
      color: var(--ceskia-accent-primary) !important;
    }

    .formato-csv {
      background-color: var(--ceskia-accent-info-glow) !important;
      color: var(--ceskia-accent-info) !important;
    }
  `]
})
export class ReportesDashboardComponent implements OnInit {
  private readonly reportesService = inject(ReportesService);
  private readonly dialog = inject(MatDialog);

  historial = signal<ReporteHistorial[]>([]);
  estadisticas = signal<ReporteEstadisticas | null>(null);
  loading = signal(true);

  displayedColumns = ['entidad', 'formato', 'registros', 'estado', 'fecha'];

  ngOnInit(): void {
    this.cargarDatos();
  }

  cargarDatos(): void {
    this.loading.set(true);

    // Cargar historial reciente
    this.reportesService.getHistorial(10).subscribe({
      next: (data) => {
        this.historial.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });

    // Cargar estadisticas
    const hace30Dias = new Date();
    hace30Dias.setDate(hace30Dias.getDate() - 30);

    this.reportesService.getEstadisticas(hace30Dias, new Date()).subscribe({
      next: (data) => this.estadisticas.set(data)
    });
  }

  abrirExportDialog(): void {
    const dialogRef = this.dialog.open(ExportDialogComponent, {
      width: '600px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.cargarDatos();
      }
    });
  }

  formatBytes(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
}
