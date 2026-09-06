import { Component, Input, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

/**
 * Componente reutilizable para exportar datos de cualquier entidad
 *
 * Uso:
 * <app-export-button
 *   entidad="Documentos"
 *   [filtros]="filtrosActuales"
 *   [columnas]="['Id', 'Nombre', 'Fecha']">
 * </app-export-button>
 */
@Component({
  selector: 'app-export-button',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  template: `
    @if (exporting()) {
      <button mat-button disabled>
        <mat-spinner diameter="20"></mat-spinner>
        Exportando...
      </button>
    } @else {
      <button mat-button [matMenuTriggerFor]="exportMenu" [disabled]="disabled">
        <mat-icon>download</mat-icon>
        {{ label }}
      </button>
      <mat-menu #exportMenu="matMenu">
        <button mat-menu-item (click)="exportar('excel')">
          <mat-icon class="excel-icon">table_chart</mat-icon>
          <span>Exportar a Excel</span>
        </button>
        <button mat-menu-item (click)="exportar('csv')">
          <mat-icon class="csv-icon">description</mat-icon>
          <span>Exportar a CSV</span>
        </button>
      </mat-menu>
    }
  `,
  styles: [`
    :host {
      display: inline-block;
    }

    .excel-icon {
      color: var(--ceskia-accent-success);
    }

    .csv-icon {
      color: var(--ceskia-accent-info);
    }

    mat-spinner {
      display: inline-block;
      margin-right: 8px;
    }
  `]
})
export class ExportButtonComponent {
  private readonly http = inject(HttpClient);
  private readonly snackBar = inject(MatSnackBar);

  /** Nombre de la entidad/tabla a exportar */
  @Input({ required: true }) entidad!: string;

  /** Filtros a aplicar en la exportacion */
  @Input() filtros?: { [key: string]: any };

  /** Columnas especificas a incluir (null = todas) */
  @Input() columnas?: string[];

  /** Headers personalizados para las columnas */
  @Input() columnasHeaders?: { [key: string]: string };

  /** Titulo del reporte */
  @Input() titulo?: string;

  /** Maximo de registros */
  @Input() maxRegistros = 10000;

  /** Columna para ordenar */
  @Input() ordenarPor?: string;

  /** Orden descendente */
  @Input() ordenDesc = false;

  /** Texto del boton */
  @Input() label = 'Exportar';

  /** Deshabilitar boton */
  @Input() disabled = false;

  exporting = signal(false);

  exportar(formato: 'excel' | 'csv'): void {
    this.exporting.set(true);

    const request = {
      entidad: this.entidad,
      formato,
      titulo: this.titulo || `Reporte de ${this.entidad}`,
      columnas: this.columnas,
      columnasHeaders: this.columnasHeaders,
      filtros: this.filtros,
      ordenarPor: this.ordenarPor,
      ordenDesc: this.ordenDesc,
      maxRegistros: this.maxRegistros
    };

    this.http.post(`${environment.apiUrl}/reportes/exportar`, request, {
      responseType: 'blob'
    }).subscribe({
      next: (blob) => {
        this.exporting.set(false);

        // Descargar archivo
        const extension = formato === 'csv' ? 'csv' : 'xlsx';
        const filename = `${this.entidad}_${new Date().toISOString().slice(0, 10)}.${extension}`;

        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = filename;
        link.click();
        window.URL.revokeObjectURL(url);

        this.snackBar.open(`Archivo ${filename} descargado`, 'OK', { duration: 3000 });
      },
      error: (err) => {
        this.exporting.set(false);
        console.error('Error exportando:', err);
        this.snackBar.open(
          err.error?.message || 'Error al exportar datos',
          'Cerrar',
          { duration: 5000 }
        );
      }
    });
  }
}
