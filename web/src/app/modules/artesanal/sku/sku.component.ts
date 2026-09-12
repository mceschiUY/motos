import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { usd } from '../comun/usd.pipe';
import { EtiquetaPipe } from '../comun/etiquetas';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MovimientoStockFormComponent } from '../../generated/components/movimientostock/movimientostock-form/movimientostock-form.component';
import { VarianteFormComponent } from '../../generated/components/variante/variante-form/variante-form.component';
import { VarianteService } from '../../generated/services/variante.service';
import { Variante } from '../../generated/models/variante.model';
import { PrecioVariante } from '../catalogo/catalogo.model';
import { SkuService } from './sku.service';
import { FichaSku, FichaSkuHermano, FichaSkuMovimiento, SemaforoStock } from './sku.model';

/** Una fila de la matriz de hermanos: un color con una celda por talla (null si no existe el SKU). */
interface FilaHermanos {
  colorId: number | null;
  colorDisplay: string;
  colorHex: string | null;
  celdas: (FichaSkuHermano | null)[];
}

/**
 * Ficha de SKU — escena artesanal (pisa `/variante/:id`, la ficha generada de Variante).
 * Cuenta la historia de un SKU: qué es (producto, talla, color), cuánto vale (precio, costo,
 * margen), dónde está el stock (por depósito), cómo se movió (Kardex con saldo corrido) y
 * cuándo se acaba (cobertura al ritmo de los últimos 30 días).
 *
 * Solo LEE por `/api/artesanal/sku/{id}`; las escrituras van por los forms generados abiertos
 * en diálogo (MovimientoStockForm con el SKU fijado por contexto, VarianteForm para editar).
 * `?movimientoId=N` (viene del redirect de `/movimientostock/:id`) resalta esa fila del Kardex.
 */
@Component({
  selector: 'app-sku',
  standalone: true,
  imports: [EtiquetaPipe, 
    CommonModule,
    MatIconModule, MatButtonModule, MatTooltipModule, MatProgressSpinnerModule,
    MatSnackBarModule, MatDialogModule,
  ],
  templateUrl: './sku.component.html',
  styleUrl: './sku.component.scss',
})
export class SkuComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly service = inject(SkuService);
  private readonly varianteService = inject(VarianteService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly isLoading = signal(true);
  readonly ficha = signal<FichaSku | null>(null);
  readonly foto = signal<string | null>(null);
  readonly precios = signal<PrecioVariante[]>([]);
  /** Movimiento a resaltar en el Kardex (`?movimientoId=N`). */
  readonly movimientoId = signal<number | null>(null);
  id = 0;

  readonly semaforo = computed<SemaforoStock>(() => this.ficha()?.semaforoStock ?? 'ok');
  readonly stockEnAlerta = computed(() => this.semaforo() !== 'ok');

  readonly etiquetaSemaforo = computed(() => ({
    ok: 'En stock', bajo: 'Stock bajo', sin_stock: 'Sin stock', negativo: 'Saldo negativo',
  } as Record<SemaforoStock, string>)[this.semaforo()]);

  readonly iconoSemaforo = computed(() => ({
    ok: 'inventory_2', bajo: 'report', sin_stock: 'remove_shopping_cart', negativo: 'warning',
  } as Record<SemaforoStock, string>)[this.semaforo()]);

  /** "Se acaba en N días" o "sin ventas en 30 días". */
  readonly textoCobertura = computed(() => {
    const f = this.ficha();
    if (!f) { return ''; }
    if (f.coberturaDias == null) { return 'Sin ventas en 30 días'; }
    if (f.stockTotal <= 0) { return 'Ya se acabó'; }
    return `Se acaba en ${this.formatoNumero(f.coberturaDias, 0)} días`;
  });

  readonly coberturaCritica = computed(() => {
    const f = this.ficha();
    return f?.coberturaDias != null && f.coberturaDias < 15;
  });

  // ─── Hermanos: matriz color × talla (Etapa H.8) ───
  readonly hermanos = computed<FichaSkuHermano[]>(() => this.ficha()?.hermanos ?? []);
  readonly actual = computed(() => this.hermanos().find(h => h.esActual) ?? null);
  readonly otrosHermanos = computed(() => this.hermanos().filter(h => !h.esActual));

  /** Tallas del producto en orden, como columnas. */
  readonly tallas = computed(() => {
    const vistas = new Map<string, { id: number | null; nombre: string; orden: number }>();
    for (const h of this.hermanos()) {
      const clave = String(h.tallaId ?? 'unica');
      if (!vistas.has(clave)) { vistas.set(clave, { id: h.tallaId, nombre: h.tallaDisplay || 'Única', orden: h.tallaOrden }); }
    }
    return [...vistas.values()].sort((a, b) => a.orden - b.orden || a.nombre.localeCompare(b.nombre));
  });

  /** Colores del producto como filas, cada una con una celda por talla. */
  readonly filasHermanos = computed<FilaHermanos[]>(() => {
    const filas = new Map<string, FilaHermanos>();
    const tallas = this.tallas();
    for (const h of this.hermanos()) {
      const clave = String(h.colorId ?? 'unico');
      let fila = filas.get(clave);
      if (!fila) {
        fila = { colorId: h.colorId, colorDisplay: h.colorDisplay || 'Color único', colorHex: h.colorHex, celdas: tallas.map(() => null) };
        filas.set(clave, fila);
      }
      const col = tallas.findIndex(t => t.id === h.tallaId);
      if (col >= 0) { fila.celdas[col] = h; }
    }
    return [...filas.values()].sort((a, b) => a.colorDisplay.localeCompare(b.colorDisplay));
  });

  readonly unidadesHermanos = computed(() => this.hermanos().reduce((s, h) => s + (h.saldo ?? 0), 0));
  readonly hermanosConStock = computed(() => this.otrosHermanos().filter(h => h.activo && h.saldo - h.comprometido > 0));

  /** Cuando este SKU está flojo, las alternativas del mismo producto con stock neto, las de la misma talla primero. */
  readonly alternativas = computed(() => {
    const f = this.ficha();
    const yo = this.actual();
    if (!f || !yo || f.disponibleNeto > 0 && this.semaforo() === 'ok') { return []; }
    return [...this.hermanosConStock()]
      .sort((a, b) => Number(b.tallaId === yo.tallaId) - Number(a.tallaId === yo.tallaId)
        || Number(b.colorId === yo.colorId) - Number(a.colorId === yo.colorId)
        || (b.saldo - b.comprometido) - (a.saldo - a.comprometido))
      .slice(0, 4);
  });

  irAlHermano(h: FichaSkuHermano | null): void {
    if (!h || h.esActual) { return; }
    this.router.navigate(['/variante', h.varianteId]);
  }

  nombreHermano(h: FichaSkuHermano): string {
    return `${h.tallaDisplay || 'única'} ${h.colorDisplay || ''}`.trim();
  }

  tooltipHermano(h: FichaSkuHermano): string {
    const neto = h.saldo - h.comprometido;
    const base = `${h.sku} · ${this.nombreHermano(h)} · ${this.formatoNumero(h.saldo)} en stock`;
    const comp = h.comprometido > 0 ? ` · ${this.formatoNumero(h.comprometido)} comprometidas · ${this.formatoNumero(neto)} neto` : '';
    return base + comp + (h.activo ? '' : ' · inactivo') + (h.esActual ? ' · este SKU' : ' · abrir');
  }

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(qp => {
      const mid = Number(qp.get('movimientoId') ?? 0);
      this.movimientoId.set(mid > 0 ? mid : null);
      if (this.ficha()) { this.scrollAlMovimiento(); }
    });
    this.route.paramMap.subscribe(pm => {
      this.id = Number(pm.get('id') ?? 0);
      this.cargar();
    });
  }

  cargar(): void {
    if (!this.id) { this.ficha.set(null); this.isLoading.set(false); return; }
    this.isLoading.set(true);
    this.service.ficha(this.id).subscribe({
      next: (f: FichaSku) => {
        this.ficha.set(f);
        this.isLoading.set(false);
        this.foto.set(null);
        if (f?.imagenPrincipalId != null) {
          this.service.portada(f.imagenPrincipalId).subscribe(src => this.foto.set(src));
        }
        this.scrollAlMovimiento();
      },
      error: (e: unknown) => { console.error('[Sku] ficha:', e); this.ficha.set(null); this.isLoading.set(false); },
    });
    this.service.historialPrecios(this.id).subscribe({
      next: (p: PrecioVariante[]) => this.precios.set(p ?? []),
      error: () => this.precios.set([]),
    });
  }

  /** Lleva la vista a la fila resaltada del Kardex, una vez que Angular la pintó. */
  private scrollAlMovimiento(): void {
    const mid = this.movimientoId();
    if (mid == null) { return; }
    setTimeout(() => {
      document.getElementById('mov-' + mid)?.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }, 80);
  }

  // ─── Navegación ───
  volver(): void {
    const pid = this.ficha()?.productoId;
    if (pid) { this.router.navigate(['/catalogo', pid]); } else { this.router.navigate(['/catalogo']); }
  }
  verPedido(pedidoId: number): void { this.router.navigate(['/pedido', pedidoId]); }
  agregarAlPedido(): void { this.router.navigate(['/pedidos/nuevo'], { queryParams: { varianteId: this.id } }); }

  // ─── Acciones (escriben por los forms generados) ───
  registrarMovimiento(): void {
    const ref = this.dialog.open(MovimientoStockFormComponent, {
      width: '600px', maxWidth: '95vw', panelClass: 'crm-dialog', autoFocus: true,
      data: { item: null, mode: 'create', contextoFk: { campo: 'varianteId', valor: this.id } },
    });
    ref.afterClosed().subscribe(res => {
      if (res) { this.showMessage('Movimiento registrado', 'success'); this.cargar(); }
    });
  }

  editarDatos(): void {
    this.varianteService.getById(this.id).subscribe({
      next: (item: Variante | undefined) => {
        if (!item) { this.showMessage('No se pudo leer el SKU', 'error'); return; }
        const ref = this.dialog.open(VarianteFormComponent, {
          width: '600px', maxWidth: '95vw', panelClass: 'crm-dialog',
          data: { item, mode: 'edit' },
        });
        ref.afterClosed().subscribe(ok => {
          if (ok) { this.showMessage('Datos del SKU guardados', 'success'); this.cargar(); }
        });
      },
      error: () => this.showMessage('No se pudo leer el SKU', 'error'),
    });
  }

  // ─── Presentación ───
  /** Cantidad con signo para la línea de tiempo: +5 / −3 / 5 → (transferencia). */
  cantidadConSigno(m: FichaSkuMovimiento): string {
    const n = this.formatoNumero(Math.abs(m.cantidad), 0);
    if (m.tipo === 'transferencia' && m.depositoDestinoId != null) { return n; }
    if (m.delta > 0) { return '+' + n; }
    if (m.delta < 0) { return '−' + n; }
    return n;
  }

  claseDelta(m: FichaSkuMovimiento): string {
    if (m.tipo === 'transferencia' && m.depositoDestinoId != null) { return 'mov-neutro'; }
    return m.delta > 0 ? 'mov-positivo' : m.delta < 0 ? 'mov-negativo' : 'mov-neutro';
  }

  etiquetaCampoPrecio(campo: string): string {
    return campo === 'costo' ? 'Costo estándar' : 'Precio de lista';
  }

  formatoNumero(n: number | null | undefined, decimales = 0): string {
    return new Intl.NumberFormat('es-UY', { minimumFractionDigits: decimales, maximumFractionDigits: decimales }).format(n ?? 0);
  }

  formatoUsd(n: number | null | undefined): string {
    return usd(n);
  }

  trackByMovimiento(_: number, m: FichaSkuMovimiento): number { return m.movimientoId; }

  private showMessage(message: string, type: 'success' | 'error'): void {
    this.snackBar.open(message, 'Cerrar', { duration: 3000, panelClass: type === 'success' ? 'snackbar-success' : 'snackbar-error' });
  }
}
