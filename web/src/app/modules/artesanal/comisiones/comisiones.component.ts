import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { usd } from '../comun/usd.pipe';
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

type Semaforo = 'verde' | 'amarillo' | 'rojo' | 'sin-meta';

/** Variación contra el mes anterior, lista para pintar. */
interface Delta {
  /** 'sube' | 'baja' | 'igual' | 'nuevo' (antes no había nada) */
  tendencia: 'sube' | 'baja' | 'igual' | 'nuevo';
  /** Porcentaje absoluto (ej. 23 para +23 % o −23 %). null cuando no se puede comparar. */
  pct: number | null;
}

/**
 * Liquidación de comisiones — pantalla artesanal de SOLO LECTURA (plan §4.4, Etapa B).
 * Una fila por vendedor y mes: pedidos entregados, total vendido y comisión, todo en USD.
 * No recalcula nada: muestra la comisión que cada pedido selló al entregarse, así lo que
 * se paga coincide con lo que el sistema registró en ese momento.
 * Etapa H.6: cada fila trae además la meta del mes con su barra y semáforo (mismo criterio
 * que el panel del vendedor: contra lo esperado a la fecha), la comisión PROYECTADA si
 * entrega lo que tiene abierto, y el mes anterior con la variación.
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

  readonly esMesActual = computed(() => this.periodo() === mesActual());

  /** La proyección solo tiene sentido en el mes en curso: en uno cerrado lo abierto ya es otra historia. */
  readonly columnas = computed(() => this.esMesActual()
    ? ['vendedorDisplay', 'zona', 'pedidos', 'totalUsd', 'meta', 'comisionPorcentaje', 'comisionUsd', 'proyectada', 'anterior']
    : ['vendedorDisplay', 'zona', 'pedidos', 'totalUsd', 'meta', 'comisionPorcentaje', 'comisionUsd', 'anterior']);

  readonly conVentas = computed(() => this.items().filter(i => i.pedidos > 0));
  readonly totalPedidos = computed(() => this.suma(i => i.pedidos));
  readonly totalVendido = computed(() => this.suma(i => i.totalUsd));
  readonly totalComision = computed(() => this.suma(i => i.comisionUsd));
  readonly totalObjetivo = computed(() => this.suma(i => i.objetivoUsd));
  readonly totalPendiente = computed(() => this.suma(i => i.pendienteEntregaUsd));
  readonly totalPedidosPendientes = computed(() => this.suma(i => i.pedidosPendientes));
  readonly totalProyectada = computed(() => this.suma(i => i.comisionProyectadaUsd));
  readonly anteriorPedidos = computed(() => this.suma(i => i.anteriorPedidos));
  readonly anteriorVendido = computed(() => this.suma(i => i.anteriorTotalUsd));
  readonly anteriorComision = computed(() => this.suma(i => i.anteriorComisionUsd));
  readonly periodoAnterior = computed(() => this.items()[0]?.periodoAnterior ?? mesAnteriorDe(this.periodo()));

  /** Avance del equipo contra la suma de metas (0 si nadie tiene meta cargada). */
  readonly pctEquipo = computed(() => this.pct(this.totalVendido(), this.totalObjetivo()));
  readonly semaforoEquipo = computed<Semaforo>(() => this.semaforoDe(this.totalVendido(), this.totalObjetivo()));
  readonly deltaVendido = computed<Delta>(() => this.delta(this.totalVendido(), this.anteriorVendido()));
  readonly deltaComision = computed<Delta>(() => this.delta(this.totalComision(), this.anteriorComision()));

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

  /** "Ago 2026", para las columnas donde no entra el nombre completo. */
  etiquetaCorta(p: string): string {
    return this.etiquetaPeriodo(p).slice(0, 3) + ' ' + p.slice(0, 4);
  }

  usd(n: number): string {
    return usd(n);
  }

  // ─── Meta por fila (mismo criterio que el panel del vendedor) ─────────────
  pctMeta(fila: ComisionVendedor): number {
    return this.pct(fila.totalUsd, fila.objetivoUsd);
  }

  semaforoMeta(fila: ComisionVendedor): Semaforo {
    return this.semaforoDe(fila.totalUsd, fila.objetivoUsd);
  }

  /** Dónde cae hoy dentro del mes (0..100); en un mes cerrado, 100. */
  marcaHoyPct(): number {
    return Math.round(this.fraccionMes() * 100);
  }

  tooltipMeta(fila: ComisionVendedor): string {
    if (fila.objetivoUsd <= 0) return 'Sin meta cargada para este mes';
    const falta = Math.max(0, fila.objetivoUsd - fila.totalUsd);
    const base = `Meta ${usd(fila.objetivoUsd)} · vendido ${usd(fila.totalUsd)} (${this.pctMeta(fila)} %)`;
    if (!this.esMesActual()) return falta > 0 ? `${base} · faltaron ${usd(falta)}` : `${base} · cumplida`;
    const esperado = fila.objetivoUsd * this.fraccionMes();
    return `${base} · esperado a hoy ${usd(esperado)} · falta ${usd(falta)}`;
  }

  // ─── Mes anterior ────────────────────────────────────────────────────────
  deltaFila(fila: ComisionVendedor): Delta {
    return this.delta(fila.comisionUsd, fila.anteriorComisionUsd);
  }

  tooltipAnterior(fila: ComisionVendedor): string {
    const p = this.etiquetaPeriodo(fila.periodoAnterior);
    if (fila.anteriorPedidos === 0) return `${p}: sin entregas`;
    const meta = fila.anteriorObjetivoUsd > 0
      ? ` · meta ${usd(fila.anteriorObjetivoUsd)} (${this.pct(fila.anteriorTotalUsd, fila.anteriorObjetivoUsd)} %)`
      : '';
    return `${p}: ${fila.anteriorPedidos} entregados · vendido ${usd(fila.anteriorTotalUsd)}${meta}`;
  }

  tooltipProyectada(fila: ComisionVendedor): string {
    if (fila.pedidosPendientes === 0) return 'No tiene pedidos abiertos este mes: lo proyectado es lo sellado';
    return `${fila.pedidosPendientes} abiertos por ${usd(fila.pendienteEntregaUsd)} al ${fila.comisionPorcentaje} % de hoy`;
  }

  iconoDelta(d: Delta): string {
    return d.tendencia === 'sube' ? 'trending_up' : d.tendencia === 'baja' ? 'trending_down' : 'trending_flat';
  }

  textoDelta(d: Delta): string {
    if (d.tendencia === 'nuevo') return 'nuevo';
    if (d.pct === null) return '—';
    return (d.tendencia === 'sube' ? '+' : d.tendencia === 'baja' ? '−' : '') + d.pct + ' %';
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
    const headers = ['periodo', 'vendedorDisplay', 'zona', 'pedidos', 'totalUsd', 'objetivoUsd', 'comisionPorcentaje',
                     'comisionUsd', 'pedidosPendientes', 'pendienteEntregaUsd', 'comisionProyectadaUsd',
                     'periodoAnterior', 'anteriorPedidos', 'anteriorTotalUsd', 'anteriorComisionUsd', 'anteriorObjetivoUsd'];
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

  // ─── Helpers ─────────────────────────────────────────────────────────────
  private suma(f: (i: ComisionVendedor) => number): number {
    return this.items().reduce((s, i) => s + (f(i) ?? 0), 0);
  }

  private pct(valor: number, objetivo: number): number {
    return objetivo > 0 ? Math.round(valor / objetivo * 100) : 0;
  }

  /** Parte del mes transcurrida: en el mes actual día/díasDelMes; en uno cerrado, 1. */
  private fraccionMes(): number {
    if (!this.esMesActual()) return 1;
    const hoy = new Date();
    const diasMes = new Date(hoy.getFullYear(), hoy.getMonth() + 1, 0).getDate();
    return hoy.getDate() / diasMes;
  }

  private semaforoDe(vendido: number, objetivo: number): Semaforo {
    if (objetivo <= 0) return 'sin-meta';
    if (vendido >= objetivo) return 'verde';
    const esperado = objetivo * this.fraccionMes();
    if (esperado <= 0) return 'verde';
    const ratio = vendido / esperado;
    return ratio >= 0.8 ? 'verde' : ratio >= 0.5 ? 'amarillo' : 'rojo';
  }

  private delta(actual: number, anterior: number): Delta {
    if (anterior <= 0) return actual > 0 ? { tendencia: 'nuevo', pct: null } : { tendencia: 'igual', pct: null };
    const pct = Math.round((actual - anterior) / anterior * 100);
    return { tendencia: pct > 0 ? 'sube' : pct < 0 ? 'baja' : 'igual', pct: Math.abs(pct) };
  }

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

function mesAnteriorDe(periodo: string): string {
  const [y, m] = periodo.split('-').map(Number);
  const d = new Date(y, m - 2, 1);
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
}
