import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ExistenciasService } from './existencias.service';
import { ExistenciaFila, Existencias, SemaforoStock } from './existencia.model';

/** Producto dentro de una categoría: sus SKU y los totales. */
export interface GrupoProducto {
  productoId: number;
  nombre: string;
  codigo: string | null;
  marca: string | null;
  categoria: string | null;
  skus: ExistenciaFila[];
  unidades: number;
  comprometido: number;
  alertas: number;
  peor: SemaforoStock;
}

/** Categoría raíz: sus productos y los totales. */
export interface GrupoCategoria {
  nombre: string;
  productos: GrupoProducto[];
  unidades: number;
  skus: number;
  alertas: number;
}

const ORDEN_SEMAFORO: Record<SemaforoStock, number> = { negativo: 0, sin_stock: 1, bajo: 2, ok: 3 };
const ABIERTOS_KEY = 'existencias-abiertos-v2';

/**
 * EXISTENCIAS — escena "¿qué se está acabando?" (plan Etapa D, 2026-09-12). Reemplaza a la
 * lista plana de 73 filas SKU × depósito: agrupa categoría → producto → SKU con subtotales,
 * semáforo por SKU (negativo, sin stock, bajo umbral, ok), cobertura en días y comprometido.
 * Solo lectura: los movimientos se registran desde la ficha del SKU. Export CSV de las filas
 * planas visibles. Datos por `GET /api/artesanal/existencias`.
 */
@Component({
  selector: 'app-existencias',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatIconModule, MatButtonModule, MatTooltipModule, MatFormFieldModule, MatSelectModule,
    MatProgressSpinnerModule, MatSnackBarModule,
  ],
  templateUrl: './existencias.component.html',
  styleUrl: './existencias.component.scss',
})
export class ExistenciasComponent implements OnInit {
  private readonly service = inject(ExistenciasService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  readonly cargando = signal(true);
  readonly datos = signal<Existencias | null>(null);
  readonly busqueda = signal('');
  readonly depositoId = signal<number | null>(null);
  readonly soloAlertas = signal(false);
  /** Categorías y productos abiertos (clave: nombre de categoría o `p:<productoId>`). */
  readonly abiertos = signal<Set<string>>(this.leerAbiertos());
  readonly fabMenuExpanded = signal(false);

  readonly depositos = computed(() => this.datos()?.depositos ?? []);
  readonly umbral = computed(() => this.datos()?.umbralStockBajo ?? 3);
  readonly depositoNombre = computed(() => this.depositos().find(d => d.id === this.depositoId())?.nombre ?? null);

  /** Filas planas que pasan búsqueda y "solo alertas". */
  readonly filas = computed<ExistenciaFila[]>(() => {
    const t = this.busqueda().trim().toLowerCase();
    const solo = this.soloAlertas();
    return (this.datos()?.filas ?? []).filter(f => {
      if (solo && f.semaforo === 'ok') return false;
      if (!t) return true;
      return [f.sku, f.productoNombre, f.productoCodigo, f.marca, f.categoria, f.talla, f.color]
        .some(x => (x ?? '').toLowerCase().includes(t));
    });
  });

  /** Categoría → producto → SKU, con subtotales; las categorías con más alertas primero. */
  readonly grupos = computed<GrupoCategoria[]>(() => {
    const porCategoria = new Map<string, Map<number, GrupoProducto>>();
    for (const f of this.filas()) {
      const cat = f.categoriaRaiz || f.categoria || 'Sin categoría';
      if (!porCategoria.has(cat)) porCategoria.set(cat, new Map());
      const productos = porCategoria.get(cat)!;
      if (!productos.has(f.productoId)) {
        productos.set(f.productoId, {
          productoId: f.productoId, nombre: f.productoNombre ?? '', codigo: f.productoCodigo, marca: f.marca,
          categoria: f.categoria, skus: [], unidades: 0, comprometido: 0, alertas: 0, peor: 'ok',
        });
      }
      const p = productos.get(f.productoId)!;
      p.skus.push(f);
      p.unidades += f.saldo;
      p.comprometido += f.comprometido;
      if (f.semaforo !== 'ok') p.alertas++;
      if (ORDEN_SEMAFORO[f.semaforo] < ORDEN_SEMAFORO[p.peor]) p.peor = f.semaforo;
    }
    return [...porCategoria.entries()].map(([nombre, productos]) => {
      const lista = [...productos.values()].sort((a, b) => b.alertas - a.alertas || a.nombre.localeCompare(b.nombre));
      return {
        nombre, productos: lista,
        unidades: lista.reduce((s, p) => s + p.unidades, 0),
        skus: lista.reduce((s, p) => s + p.skus.length, 0),
        alertas: lista.reduce((s, p) => s + p.alertas, 0),
      };
    }).sort((a, b) => b.alertas - a.alertas || b.unidades - a.unidades);
  });

  // KPI del cabezal sobre TODAS las filas (no las filtradas): el estado del depósito.
  readonly totalUnidades = computed(() => (this.datos()?.filas ?? []).reduce((s, f) => s + f.saldo, 0));
  readonly totalSkus = computed(() => (this.datos()?.filas ?? []).length);
  readonly conStock = computed(() => (this.datos()?.filas ?? []).filter(f => f.saldo > 0).length);
  readonly enAlerta = computed(() => (this.datos()?.filas ?? []).filter(f => f.semaforo !== 'ok').length);
  readonly negativos = computed(() => (this.datos()?.filas ?? []).filter(f => f.semaforo === 'negativo').length);
  readonly comprometidoTotal = computed(() => (this.datos()?.filas ?? []).reduce((s, f) => s + f.comprometido, 0));

  ngOnInit(): void { this.cargar(); }

  cargar(): void {
    this.cargando.set(true);
    this.service.agrupadas(this.depositoId()).subscribe({
      next: d => {
        this.datos.set(d);
        this.cargando.set(false);
        // Primera vez: todas las categorías abiertas, productos cerrados.
        if (this.abiertos().size === 0) {
          this.abiertos.set(new Set(this.grupos().map(g => g.nombre)));
        }
      },
      error: (e: unknown) => {
        console.error('[Existencias] agrupadas:', e);
        this.datos.set(null);
        this.cargando.set(false);
        this.mensaje('No se pudieron cargar las existencias', 'error');
      },
    });
  }

  onDepositoChange(id: number | null): void { this.depositoId.set(id); this.cargar(); }

  limpiar(): void { this.busqueda.set(''); this.soloAlertas.set(false); }

  estaAbierto(clave: string): boolean { return this.abiertos().has(clave); }

  alternar(clave: string): void {
    this.abiertos.update(s => {
      const n = new Set(s);
      if (n.has(clave)) n.delete(clave); else n.add(clave);
      return n;
    });
    this.guardarAbiertos();
  }

  expandirTodo(): void {
    const claves = new Set<string>();
    for (const g of this.grupos()) { claves.add(g.nombre); for (const p of g.productos) claves.add('p:' + p.productoId); }
    this.abiertos.set(claves);
    this.guardarAbiertos();
  }

  contraerTodo(): void { this.abiertos.set(new Set(this.grupos().map(g => g.nombre))); this.guardarAbiertos(); }

  /** Cobertura legible: "12 días", "sin ventas" o "—" (sin stock). */
  cobertura(f: ExistenciaFila): string {
    if (f.saldo <= 0) return '—';
    if (f.coberturaDias == null) return 'sin ventas 30 d';
    return f.coberturaDias >= 999 ? '> 999 días' : `${Math.round(f.coberturaDias)} días`;
  }

  etiquetaSemaforo(s: SemaforoStock): string {
    return s === 'negativo' ? 'negativo' : s === 'sin_stock' ? 'sin stock' : s === 'bajo' ? 'bajo' : 'ok';
  }

  verSku(f: ExistenciaFila): void { this.router.navigate(['/variante', f.varianteId]); }
  verProducto(p: GrupoProducto): void { this.router.navigate(['/catalogo', p.productoId]); }
  agregarAlPedido(f: ExistenciaFila, ev: Event): void {
    ev.stopPropagation();
    this.router.navigate(['/pedidos/nuevo'], { queryParams: { varianteId: f.varianteId } });
  }

  exportarCsv(): void {
    const filas = this.filas();
    if (filas.length === 0) { this.mensaje('No hay datos para exportar', 'error'); return; }
    const deps = this.depositos();
    const headers = ['Categoría', 'Producto', 'Marca', 'SKU', 'Talla', 'Color', 'Saldo', ...deps.map(d => d.nombre ?? ''), 'Comprometido', 'Vendidas 30d', 'Cobertura (días)', 'Semáforo'];
    const rows = filas.map(f => [
      f.categoriaRaiz ?? f.categoria ?? '', f.productoNombre ?? '', f.marca ?? '', f.sku ?? '', f.talla ?? '', f.color ?? '',
      String(f.saldo), ...deps.map(d => String(f.porDeposito.find(x => x.depositoId === d.id)?.saldo ?? 0)),
      String(f.comprometido), String(f.vendidas30d), f.coberturaDias == null ? '' : String(f.coberturaDias), this.etiquetaSemaforo(f.semaforo),
    ]);
    const csv = [headers, ...rows].map(r => r.map(c => '"' + String(c).replace(/"/g, '""') + '"').join(',')).join('\n');
    const blob = new Blob(['﻿' + csv], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = 'existencias_' + new Date().toISOString().split('T')[0] + '.csv';
    link.click();
    URL.revokeObjectURL(link.href);
    this.mensaje('Archivo CSV exportado', 'success');
  }

  toggleFabMenu(): void { this.fabMenuExpanded.update(v => !v); }
  goBack(): void { this.router.navigate(['/']); }

  private leerAbiertos(): Set<string> {
    try {
      const raw = localStorage.getItem(ABIERTOS_KEY);
      return raw ? new Set(JSON.parse(raw) as string[]) : new Set();
    } catch { return new Set(); }
  }

  private guardarAbiertos(): void {
    try { localStorage.setItem(ABIERTOS_KEY, JSON.stringify([...this.abiertos()])); } catch { /* sin storage */ }
  }

  private mensaje(texto: string, tipo: 'success' | 'error'): void {
    this.snackBar.open(texto, 'Cerrar', { duration: 3000, panelClass: tipo === 'error' ? 'snackbar-error' : 'snackbar-success' });
  }
}
