import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { FormsModule } from '@angular/forms';
import { ReportesService } from '../../services/reportes.service';
import { ReporteHistorial } from '../../models/reportes.model';

@Component({
  selector: 'app-historial-list',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatChipsModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatSelectModule,
    FormsModule
  ],
  template: `
    <div class="historial-container">
      <div class="header">
        <h1>Historial de Reportes</h1>
        <div class="filters">
          <mat-form-field appearance="outline">
            <mat-label>Filtrar por entidad</mat-label>
            <mat-select [(ngModel)]="filtroEntidad" (selectionChange)="cargarHistorial()">
              <mat-option value="">Todas</mat-option>
              @for (entidad of entidades(); track entidad) {
                <mat-option [value]="entidad">{{ entidad }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
        </div>
      </div>

      <mat-card>
        @if (loading()) {
          <div class="loading">
            <mat-spinner diameter="40"></mat-spinner>
          </div>
        } @else {
          <table mat-table [dataSource]="historial()" class="historial-table">
            <ng-container matColumnDef="id">
              <th mat-header-cell *matHeaderCellDef>ID</th>
              <td mat-cell *matCellDef="let item">{{ item.id }}</td>
            </ng-container>

            <ng-container matColumnDef="entidad">
              <th mat-header-cell *matHeaderCellDef>Entidad</th>
              <td mat-cell *matCellDef="let item">{{ item.entidad || '-' }}</td>
            </ng-container>

            <ng-container matColumnDef="tipoReporte">
              <th mat-header-cell *matHeaderCellDef>Tipo</th>
              <td mat-cell *matCellDef="let item">{{ item.tipoReporte }}</td>
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

            <ng-container matColumnDef="tamanio">
              <th mat-header-cell *matHeaderCellDef>Tamano</th>
              <td mat-cell *matCellDef="let item">
                {{ item.tamanioBytes ? formatBytes(item.tamanioBytes) : '-' }}
              </td>
            </ng-container>

            <ng-container matColumnDef="duracion">
              <th mat-header-cell *matHeaderCellDef>Duracion</th>
              <td mat-cell *matCellDef="let item">
                {{ item.duracionMs ? (item.duracionMs / 1000).toFixed(2) + 's' : '-' }}
              </td>
            </ng-container>

            <ng-container matColumnDef="estado">
              <th mat-header-cell *matHeaderCellDef>Estado</th>
              <td mat-cell *matCellDef="let item">
                <mat-chip [class]="'estado-' + item.estado.toLowerCase()">
                  @if (item.estado === 'Completado') {
                    <mat-icon>check_circle</mat-icon>
                  } @else if (item.estado === 'Error') {
                    <mat-icon>error</mat-icon>
                  } @else {
                    <mat-icon>hourglass_empty</mat-icon>
                  }
                  {{ item.estado }}
                </mat-chip>
              </td>
            </ng-container>

            <ng-container matColumnDef="fecha">
              <th mat-header-cell *matHeaderCellDef>Fecha</th>
              <td mat-cell *matCellDef="let item">
                {{ item.fechaGeneracion | date:'dd/MM/yyyy HH:mm:ss' }}
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"
                [class.error-row]="row.estado === 'Error'"></tr>
          </table>

          <mat-paginator
            [length]="totalItems()"
            [pageSize]="pageSize"
            [pageSizeOptions]="[25, 50, 100]"
            (page)="onPageChange($event)">
          </mat-paginator>
        }
      </mat-card>
    </div>
  `,
  styles: [`
    .historial-container {
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

      .filters {
        mat-form-field {
          width: 200px;
        }
      }
    }

    .historial-table {
      width: 100%;
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: 60px;
    }

    .error-row {
      background-color: var(--ceskia-accent-danger-glow);
    }

    .estado-completado {
      background-color: var(--ceskia-accent-success-glow) !important;
      color: var(--ceskia-accent-success) !important;

      mat-icon {
        font-size: 16px;
        width: 16px;
        height: 16px;
        margin-right: 4px;
      }
    }

    .estado-error {
      background-color: var(--ceskia-accent-danger-glow) !important;
      color: var(--ceskia-accent-danger) !important;

      mat-icon {
        font-size: 16px;
        width: 16px;
        height: 16px;
        margin-right: 4px;
      }
    }

    .estado-enproceso {
      background-color: var(--ceskia-accent-warning-glow) !important;
      color: var(--ceskia-accent-warning) !important;

      mat-icon {
        font-size: 16px;
        width: 16px;
        height: 16px;
        margin-right: 4px;
      }
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
export class HistorialListComponent implements OnInit {
  private readonly reportesService = inject(ReportesService);

  historial = signal<ReporteHistorial[]>([]);
  entidades = signal<string[]>([]);
  loading = signal(true);
  totalItems = signal(0);

  filtroEntidad = '';
  pageSize = 50;
  currentPage = 0;

  displayedColumns = ['id', 'entidad', 'tipoReporte', 'formato', 'registros', 'tamanio', 'duracion', 'estado', 'fecha'];

  ngOnInit(): void {
    this.cargarHistorial();
  }

  cargarHistorial(): void {
    this.loading.set(true);

    this.reportesService.getHistorial(this.pageSize, this.filtroEntidad || undefined).subscribe({
      next: (data) => {
        this.historial.set(data);
        this.totalItems.set(data.length);

        // Extraer entidades unicas para el filtro
        const entidadesUnicas = [...new Set(data.filter(h => h.entidad).map(h => h.entidad!))];
        this.entidades.set(entidadesUnicas);

        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageSize = event.pageSize;
    this.currentPage = event.pageIndex;
    this.cargarHistorial();
  }

  formatBytes(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
}
