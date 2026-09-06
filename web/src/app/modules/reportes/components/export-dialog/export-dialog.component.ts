import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { ReportesService } from '../../services/reportes.service';
import { ColumnInfo, ExportRequest } from '../../models/reportes.model';

@Component({
  selector: 'app-export-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressBarModule,
    MatChipsModule
  ],
  template: `
    <h2 mat-dialog-title>Exportar Datos</h2>

    <mat-dialog-content>
      <form [formGroup]="form">
        <!-- Entidad -->
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Entidad/Tabla</mat-label>
          <input matInput formControlName="entidad" placeholder="Ej: Documentos, Clientes">
          <mat-hint>Nombre de la tabla a exportar</mat-hint>
        </mat-form-field>

        <!-- Formato -->
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Formato</mat-label>
          <mat-select formControlName="formato">
            <mat-option value="excel">Excel (.xlsx)</mat-option>
            <mat-option value="csv">CSV (.csv)</mat-option>
          </mat-select>
        </mat-form-field>

        <!-- Titulo -->
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Titulo del Reporte (opcional)</mat-label>
          <input matInput formControlName="titulo" placeholder="Ej: Reporte de Ventas Q1">
        </mat-form-field>

        <!-- Cargar columnas -->
        <div class="columnas-section">
          <button mat-stroked-button type="button" (click)="cargarColumnas()" [disabled]="!form.value.entidad">
            <mat-icon>refresh</mat-icon>
            Cargar Columnas
          </button>

          @if (loadingColumnas()) {
            <mat-progress-bar mode="indeterminate"></mat-progress-bar>
          }

          @if (columnas().length > 0) {
            <div class="columnas-list">
              <p>Selecciona las columnas a incluir:</p>
              @for (col of columnas(); track col.name) {
                <mat-checkbox
                  [checked]="columnasSeleccionadas().includes(col.name)"
                  (change)="toggleColumna(col.name)">
                  {{ col.name }} <small class="data-type">({{ col.dataType }})</small>
                </mat-checkbox>
              }
            </div>
          }
        </div>

        <!-- Max registros -->
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Maximo de registros</mat-label>
          <input matInput type="number" formControlName="maxRegistros">
          <mat-hint>0 = sin limite</mat-hint>
        </mat-form-field>
      </form>

      @if (exporting()) {
        <div class="exporting">
          <mat-progress-bar mode="indeterminate"></mat-progress-bar>
          <p>Generando reporte...</p>
        </div>
      }

      @if (error()) {
        <div class="error-message">
          {{ error() }}
        </div>
      }
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancelar</button>
      <button
        mat-raised-button
        color="primary"
        (click)="exportar()"
        [disabled]="!form.valid || exporting()">
        <mat-icon>download</mat-icon>
        Exportar
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }

    .columnas-section {
      margin-bottom: 24px;

      button {
        margin-bottom: 12px;
      }
    }

    .columnas-list {
      max-height: 200px;
      overflow-y: auto;
      border: 1px solid var(--ceskia-border-default);
      border-radius: var(--ceskia-radius-md);
      padding: 12px;
      margin-top: 8px;

      p {
        margin: 0 0 8px 0;
        font-weight: 500;
      }

      mat-checkbox {
        display: block;
        margin: 4px 0;
      }

      .data-type {
        color: var(--ceskia-text-tertiary);
      }
    }

    .exporting {
      text-align: center;
      padding: 16px;

      p {
        margin-top: 8px;
        color: var(--ceskia-text-secondary);
      }
    }

    .error-message {
      color: var(--ceskia-accent-danger);
      background-color: var(--ceskia-accent-danger-glow);
      padding: 12px;
      border-radius: var(--ceskia-radius-md);
      margin-top: 16px;
    }
  `]
})
export class ExportDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly reportesService = inject(ReportesService);
  private readonly dialogRef = inject(MatDialogRef<ExportDialogComponent>);

  form!: FormGroup;
  columnas = signal<ColumnInfo[]>([]);
  columnasSeleccionadas = signal<string[]>([]);
  loadingColumnas = signal(false);
  exporting = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.form = this.fb.group({
      entidad: ['', Validators.required],
      formato: ['excel', Validators.required],
      titulo: [''],
      maxRegistros: [10000]
    });
  }

  cargarColumnas(): void {
    const entidad = this.form.value.entidad;
    if (!entidad) return;

    this.loadingColumnas.set(true);
    this.error.set(null);

    this.reportesService.getColumnas(entidad).subscribe({
      next: (cols) => {
        this.columnas.set(cols);
        this.columnasSeleccionadas.set(cols.map(c => c.name));
        this.loadingColumnas.set(false);
      },
      error: (err) => {
        this.error.set('Error cargando columnas. Verifique el nombre de la entidad.');
        this.loadingColumnas.set(false);
      }
    });
  }

  toggleColumna(colName: string): void {
    const current = this.columnasSeleccionadas();
    if (current.includes(colName)) {
      this.columnasSeleccionadas.set(current.filter(c => c !== colName));
    } else {
      this.columnasSeleccionadas.set([...current, colName]);
    }
  }

  exportar(): void {
    if (!this.form.valid) return;

    this.exporting.set(true);
    this.error.set(null);

    const request: ExportRequest = {
      entidad: this.form.value.entidad,
      formato: this.form.value.formato,
      titulo: this.form.value.titulo || undefined,
      columnas: this.columnasSeleccionadas().length > 0 ? this.columnasSeleccionadas() : undefined,
      maxRegistros: this.form.value.maxRegistros || 10000
    };

    this.reportesService.exportar(request).subscribe({
      next: (blob) => {
        this.exporting.set(false);

        // Descargar archivo
        const extension = request.formato === 'csv' ? 'csv' : 'xlsx';
        const filename = `${request.entidad}_${new Date().toISOString().slice(0, 10)}.${extension}`;

        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = filename;
        link.click();
        window.URL.revokeObjectURL(url);

        this.dialogRef.close(true);
      },
      error: (err) => {
        this.exporting.set(false);
        this.error.set(err.error?.message || 'Error generando el reporte');
      }
    });
  }
}
