import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ComisionesService } from './comisiones.service';
import { ComisionVendedor } from './comision.model';

/**
 * Liquidación de comisiones — pantalla artesanal de SOLO LECTURA (plan §4.4, Etapa B).
 * Una fila por vendedor y mes: pedidos entregados, total vendido y comisión, todo en USD.
 * No recalcula nada: muestra la comisión que cada pedido selló al entregarse, así lo que
 * se paga coincide con lo que el sistema registró en ese momento.
 * Reusa las clases de las listas generadas (_entity-list.scss) para verse igual.
 */
@Component({
  selector: 'app-comisiones',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatIconModule, MatButtonModule, MatTooltipModule, MatFormFieldModule, MatSelectModule,
    MatTableModule, MatProgressSpinnerModule, MatSnackBarModule,
  ],
  templateUrl: './comisiones.component.html',
  styleUrl: './comisiones.component.scss',
})
export class ComisionesComponent implements OnInit {
  private readonly service = inject(ComisionesService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  readonly isLoading = signal(false);
  readonly items = signal<ComisionVendedor[]>([]);
  readonly periodo = signal<string>(mesActual());

  readonly columnas = ['vendedorDisplay', 'zona', 'pedidos', 'totalUsd', 'comisionPorcentaje', 'comisionUsd'];
  readonly skeletonRows = Array(3).fill(0);
  fabMenuExpanded = false;

  /** Últimos 12 meses para el selector: el período es texto YYYY-MM, como la meta. */
  readonly periodos = computed(() => {
    const hoy = new Date();
    return Array.from({ length: 12 }, (_, i) => {
      const d = new Date(hoy.getFullYear(), hoy.getMonth() - i, 1);
      return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
    });
  });

  readonly conVentas = computed(() => this.items().filter(i => i.pedidos > 0));
  readonly totalPedidos = computed(() => this.items().reduce((s, i) => s + (i.pedidos ?? 0), 0));
  readonly totalVendido = computed(() => this.items().reduce((s, i) => s + (i.totalUsd ?? 0), 0));
  readonly totalComision = computed(() => this.items().reduce((s, i) => s + (i.comisionUsd ?? 0), 0));

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.service.comisiones(this.periodo()).subscribe({
      next: (data) => { this.items.set(data ?? []); this.isLoading.set(false); },
      error: (err: unknown) => {
        console.error('[Comisiones] Error cargando la liquidación:', err);
        this.items.set([]);
        this.isLoading.set(false);
        this.showMessage('Error al cargar la liquidación', 'error');
      },
    });
  }

  onPeriodoChange(valor: string): void {
    this.periodo.set(valor);
    this.loadData();
  }

  etiquetaPeriodo(p: string): string {
    const [anio, mes] = p.split('-');
    const meses = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
                   'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];
    return `${meses[parseInt(mes, 10) - 1]} ${anio}`;
  }

  usd(n: number): string {
    return 'US$ ' + new Intl.NumberFormat('es-UY', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(n ?? 0);
  }

  verVendedor(fila: ComisionVendedor): void {
    this.router.navigate(['/vendedor', fila.vendedorId]);
  }

  /** Los pedidos que componen la liquidación de esa fila. */
  verPedidos(fila: ComisionVendedor, event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/pedido'], { queryParams: { vendedorId: fila.vendedorId } });
  }

  exportAll(): void {
    const data = this.items();
    if (data.length === 0) {
      this.showMessage('No hay datos para exportar', 'error');
      return;
    }
    const headers = ['periodo', 'vendedorDisplay', 'zona', 'pedidos', 'totalUsd', 'comisionPorcentaje', 'comisionUsd'];
    const rows = data.map(item => headers.map(h => (item as any)[h]?.toString() ?? ''));
    const csv = [headers, ...rows].map(row => row.map(c => '"' + c + '"').join(',')).join('\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = `comisiones_${this.periodo()}.csv`;
    link.click();
    this.showMessage('Archivo CSV exportado', 'success');
  }

  goBack(): void { this.router.navigate(['/']); }

  toggleFabMenu(): void { this.fabMenuExpanded = !this.fabMenuExpanded; }

  private showMessage(message: string, type: 'success' | 'error'): void {
    this.snackBar.open(message, 'Cerrar', {
      duration: 3000,
      panelClass: type === 'success' ? 'snackbar-success' : 'snackbar-error',
    });
  }
}

function mesActual(): string {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
}
