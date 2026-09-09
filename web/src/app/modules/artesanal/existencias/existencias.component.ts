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
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { VarianteService } from '../../generated/services/variante.service';
import { DepositoService } from '../../generated/services/deposito.service';
import { Variante } from '../../generated/models/variante.model';
import { Deposito } from '../../generated/models/deposito.model';
import { ExistenciasService } from './existencias.service';
import { Existencia, claveExistencia } from './existencia.model';

type DensityMode = 'comfortable' | 'compact';
const DENSITY_KEY = 'existencias-density-v1';

/**
 * Existencias — pantalla artesanal de SOLO LECTURA (Fase 1, bloque Stock, ítem 4 de
 * doc/modelo-stock.md). Saldo por SKU/depósito calculado por el backend sumando el
 * Kardex. Sin alta/edición/baja: el stock no se edita, se mueve (MovimientoStock).
 * Reusa las clases de las listas generadas (_entity-list.scss) para verse igual.
 */
@Component({
  selector: 'app-existencias',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatIconModule, MatButtonModule, MatTooltipModule, MatFormFieldModule, MatSelectModule,
    MatTableModule, MatSortModule, MatPaginatorModule, MatProgressSpinnerModule, MatSnackBarModule,
  ],
  templateUrl: './existencias.component.html',
  styleUrl: './existencias.component.scss',
})
export class ExistenciasComponent implements OnInit {
  private readonly service = inject(ExistenciasService);
  private readonly varianteService = inject(VarianteService);
  private readonly depositoService = inject(DepositoService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  readonly isLoading = signal(false);
  readonly items = signal<Existencia[]>([]);
  readonly searchTerm = signal('');
  readonly densityMode = signal<DensityMode>('comfortable');

  // Filtros server-side (el endpoint acepta varianteId / depositoId opcionales)
  readonly varianteId = signal<number | null>(null);
  readonly depositoId = signal<number | null>(null);
  readonly variantes = signal<Variante[]>([]);
  readonly depositos = signal<Deposito[]>([]);

  private readonly orden = signal<Sort | null>(null);
  private readonly pagina = signal(0);
  readonly tamPagina = signal(25);

  fabMenuExpanded = false;
  readonly columnas = ['sku', 'productoDisplay', 'depositoDisplay', 'disponible'];
  readonly skeletonRows = Array(5).fill(0);
  readonly trackByClave = (_: number, e: Existencia) => claveExistencia(e);

  readonly filteredItems = computed(() => {
    const t = this.searchTerm().trim().toLowerCase();
    if (!t) return this.items();
    return this.items().filter(i =>
      [i.sku, i.productoDisplay, i.depositoDisplay].some(v => (v ?? '').toLowerCase().includes(t)));
  });

  readonly ordenados = computed(() => {
    const items = this.filteredItems();
    const orden = this.orden();
    if (!orden?.active || !orden.direction) return items;
    const dir = orden.direction === 'asc' ? 1 : -1;
    const key = orden.active as keyof Existencia;
    return [...items].sort((a, b) => {
      const va = a[key]; const vb = b[key];
      if (va == null) return 1;
      if (vb == null) return -1;
      if (typeof va === 'number' && typeof vb === 'number') return (va - vb) * dir;
      return String(va).localeCompare(String(vb), 'es') * dir;
    });
  });

  readonly pageItems = computed(() => {
    const inicio = this.pagina() * this.tamPagina();
    return this.ordenados().slice(inicio, inicio + this.tamPagina());
  });

  readonly totalCount = computed(() => this.filteredItems().length);
  readonly totalUnidades = computed(() => this.filteredItems().reduce((s, i) => s + (i.disponible ?? 0), 0));
  readonly skusConStock = computed(() =>
    new Set(this.filteredItems().filter(i => i.disponible > 0).map(i => i.varianteId)).size);
  readonly negativos = computed(() => this.filteredItems().filter(i => i.disponible < 0).length);

  ngOnInit(): void {
    try {
      const saved = localStorage.getItem(DENSITY_KEY);
      if (saved === 'compact' || saved === 'comfortable') this.densityMode.set(saved);
    } catch { /* sin storage */ }
    this.cargarOpciones();
    this.loadData();
  }

  private cargarOpciones(): void {
    this.varianteService.getAll().subscribe({
      next: (data: Variante[]) => this.variantes.set(data),
      error: (err: unknown) => console.error('[Existencias] Error cargando variantes:', err),
    });
    this.depositoService.getAll().subscribe({
      next: (data: Deposito[]) => this.depositos.set(data),
      error: (err: unknown) => console.error('[Existencias] Error cargando depósitos:', err),
    });
  }

  loadData(): void {
    this.isLoading.set(true);
    this.service.existencias(this.varianteId(), this.depositoId()).subscribe({
      next: (data) => { this.items.set(data ?? []); this.isLoading.set(false); },
      error: (err: unknown) => {
        console.error('[Existencias] Error cargando existencias:', err);
        this.items.set([]);
        this.isLoading.set(false);
        this.showMessage('Error al cargar existencias', 'error');
      },
    });
  }

  onFiltroChange(): void {
    this.pagina.set(0);
    this.loadData();
  }

  limpiarFiltros(): void {
    this.varianteId.set(null);
    this.depositoId.set(null);
    this.searchTerm.set('');
    this.onFiltroChange();
  }

  hayFiltros(): boolean {
    return this.varianteId() != null || this.depositoId() != null;
  }

  onSearchChange(event: Event): void {
    this.searchTerm.set((event.target as HTMLInputElement).value);
    this.pagina.set(0);
  }

  ordenar(sort: Sort): void { this.orden.set(sort); this.pagina.set(0); }

  paginar(e: PageEvent): void { this.pagina.set(e.pageIndex); this.tamPagina.set(e.pageSize); }

  toggleDensity(): void {
    this.densityMode.update((m: DensityMode) => m === 'comfortable' ? 'compact' : 'comfortable');
    try { localStorage.setItem(DENSITY_KEY, this.densityMode()); } catch { /* sin storage */ }
  }

  etiquetaVariante(v: Variante): string {
    return v.sku + (v.productoDisplay ? ' · ' + v.productoDisplay : '');
  }

  formato(n: number): string {
    return new Intl.NumberFormat('es-UY', { maximumFractionDigits: 2 }).format(n ?? 0);
  }

  verVariante(item: Existencia): void {
    this.router.navigate(['/variante', item.varianteId]);
  }

  /** Kardex del SKU filtrado (ruta hija generada movimientostock/by-variante/:varianteId). */
  verKardex(): void {
    const id = this.varianteId();
    if (id == null) return;
    this.router.navigate(['/movimientostock/by-variante', id]);
  }

  exportAll(): void {
    const data = this.ordenados();
    if (data.length === 0) {
      this.showMessage('No hay datos para exportar', 'error');
      return;
    }
    const headers = ['sku', 'productoDisplay', 'depositoDisplay', 'disponible'];
    const rows = data.map((item) => headers.map((h) => (item as any)[h]?.toString() || ''));
    const csvContent = [headers, ...rows].map(row => row.map(cell => '"' + cell + '"').join(',')).join('\n');
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = 'existencias_' + new Date().toISOString().split('T')[0] + '.csv';
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
