import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ReportesService } from '../../services/reportes.service';
import { ReporteProgramado } from '../../models/reportes.model';
import { confirmar } from '../../../../shared/confirm/confirmar';

@Component({
  selector: 'app-programados-list',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatSlideToggleModule,
    MatProgressSpinnerModule,
    MatMenuModule,
    MatSnackBarModule
  ],
  template: `
    <div class="programados-container">
      <div class="header">
        <h1>Reportes Programados</h1>
        <button mat-raised-button color="primary">
          <mat-icon>add</mat-icon>
          Nuevo Reporte Programado
        </button>
      </div>

      <mat-card>
        @if (loading()) {
          <div class="loading">
            <mat-spinner diameter="40"></mat-spinner>
          </div>
        } @else if (reportes().length === 0) {
          <div class="empty-state">
            <mat-icon>schedule</mat-icon>
            <h3>No hay reportes programados</h3>
            <p>Crea un reporte programado para generar exportaciones automaticamente</p>
          </div>
        } @else {
          <table mat-table [dataSource]="reportes()" class="programados-table">
            <ng-container matColumnDef="nombre">
              <th mat-header-cell *matHeaderCellDef>Nombre</th>
              <td mat-cell *matCellDef="let item">
                <strong>{{ item.nombre }}</strong>
              </td>
            </ng-container>

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

            <ng-container matColumnDef="cronExpression">
              <th mat-header-cell *matHeaderCellDef>Programacion</th>
              <td mat-cell *matCellDef="let item">
                <code>{{ item.cronExpression || 'Manual' }}</code>
              </td>
            </ng-container>

            <ng-container matColumnDef="ultimaEjecucion">
              <th mat-header-cell *matHeaderCellDef>Ultima Ejecucion</th>
              <td mat-cell *matCellDef="let item">
                {{ item.ultimaEjecucion ? (item.ultimaEjecucion | date:'dd/MM/yyyy HH:mm') : 'Nunca' }}
              </td>
            </ng-container>

            <ng-container matColumnDef="proximaEjecucion">
              <th mat-header-cell *matHeaderCellDef>Proxima Ejecucion</th>
              <td mat-cell *matCellDef="let item">
                {{ item.proximaEjecucion ? (item.proximaEjecucion | date:'dd/MM/yyyy HH:mm') : '-' }}
              </td>
            </ng-container>

            <ng-container matColumnDef="activo">
              <th mat-header-cell *matHeaderCellDef>Activo</th>
              <td mat-cell *matCellDef="let item">
                <mat-slide-toggle
                  [checked]="item.activo"
                  (change)="toggleActivo(item)">
                </mat-slide-toggle>
              </td>
            </ng-container>

            <ng-container matColumnDef="acciones">
              <th mat-header-cell *matHeaderCellDef></th>
              <td mat-cell *matCellDef="let item">
                <button mat-icon-button [matMenuTriggerFor]="menu">
                  <mat-icon>more_vert</mat-icon>
                </button>
                <mat-menu #menu="matMenu">
                  <button mat-menu-item>
                    <mat-icon>edit</mat-icon>
                    <span>Editar</span>
                  </button>
                  <button mat-menu-item>
                    <mat-icon>play_arrow</mat-icon>
                    <span>Ejecutar Ahora</span>
                  </button>
                  <button mat-menu-item>
                    <mat-icon>history</mat-icon>
                    <span>Ver Historial</span>
                  </button>
                  <button mat-menu-item (click)="eliminar(item)" class="delete-option">
                    <mat-icon color="warn">delete</mat-icon>
                    <span>Eliminar</span>
                  </button>
                </mat-menu>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"
                [class.inactive-row]="!row.activo"></tr>
          </table>
        }
      </mat-card>
    </div>
  `,
  styles: [`
    .programados-container {
      padding: 24px;
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;

      h1 {
        margin: 0;
      }
    }

    .programados-table {
      width: 100%;
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: 60px;
    }

    .empty-state {
      text-align: center;
      padding: 60px 24px;

      mat-icon {
        font-size: 64px;
        width: 64px;
        height: 64px;
        color: var(--ceskia-text-muted);
      }

      h3 {
        margin: 16px 0 8px;
        color: var(--ceskia-text-secondary);
      }

      p {
        color: var(--ceskia-text-tertiary);
        margin: 0;
      }
    }

    .inactive-row {
      opacity: 0.6;
      background-color: var(--ceskia-hover);
    }

    code {
      background-color: var(--ceskia-hover);
      padding: 2px 8px;
      border-radius: var(--ceskia-radius-md);
      font-size: 12px;
    }

    .formato-excel {
      background-color: var(--ceskia-accent-primary-glow) !important;
      color: var(--ceskia-accent-primary) !important;
    }

    .formato-csv {
      background-color: var(--ceskia-accent-info-glow) !important;
      color: var(--ceskia-accent-info) !important;
    }

    .delete-option {
      color: var(--ceskia-accent-danger);
    }
  `]
})
export class ProgramadosListComponent implements OnInit {
  private readonly reportesService = inject(ReportesService);
  private readonly snackBar = inject(MatSnackBar);

  reportes = signal<ReporteProgramado[]>([]);
  loading = signal(true);

  displayedColumns = ['nombre', 'entidad', 'formato', 'cronExpression', 'ultimaEjecucion', 'proximaEjecucion', 'activo', 'acciones'];

  ngOnInit(): void {
    this.cargarReportes();
  }

  cargarReportes(): void {
    this.loading.set(true);

    this.reportesService.getProgramados().subscribe({
      next: (data) => {
        this.reportes.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.snackBar.open('Error cargando reportes programados', 'Cerrar', { duration: 3000 });
      }
    });
  }

  toggleActivo(reporte: ReporteProgramado): void {
    const action = reporte.activo
      ? this.reportesService.desactivarProgramado(reporte.id)
      : this.reportesService.activarProgramado(reporte.id);

    action.subscribe({
      next: () => {
        this.cargarReportes();
        this.snackBar.open(
          reporte.activo ? 'Reporte desactivado' : 'Reporte activado',
          'OK',
          { duration: 2000 }
        );
      },
      error: () => {
        this.snackBar.open('Error actualizando reporte', 'Cerrar', { duration: 3000 });
      }
    });
  }

  async eliminar(reporte: ReporteProgramado): Promise<void> {
    if (!(await confirmar(`Eliminar el reporte "${reporte.nombre}"?`))) {
      return;
    }

    this.reportesService.eliminarProgramado(reporte.id).subscribe({
      next: () => {
        this.cargarReportes();
        this.snackBar.open('Reporte eliminado', 'OK', { duration: 2000 });
      },
      error: () => {
        this.snackBar.open('Error eliminando reporte', 'Cerrar', { duration: 3000 });
      }
    });
  }
}
