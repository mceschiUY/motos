import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { EtiquetaPipe } from '../comun/etiquetas';
import { UsdPipe, usd } from '../comun/usd.pipe';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { SiteConfigService } from '../../../core/services/site-config.service';
import { ActividadRecienteComponent } from '../../../shared/components/actividad-reciente/actividad-reciente.component';
import { HoyService } from './hoy.service';
import { Accion, CentroControl, PipelineEstado, VendedorResumen } from './hoy.model';

const REFRESCO_MS = 60_000;

/**
 * HOY — el centro de control como FEED DE ACCIONES (home artesanal, plan "escenas, no
 * tablas" 2026-09-12). Reemplaza al dashboard genérico de totales (que sigue en
 * /inicio-generico). Cada tarjeta dice qué hacer y a dónde ir: el feed va a la escena
 * que resuelve la acción (envío, pedido, SKU, cliente, agenda, vendedor).
 * READ-ONLY sobre un único endpoint: GET api/artesanal/centro-control.
 */
@Component({
  selector: 'app-hoy',
  standalone: true,
  imports: [UsdPipe, EtiquetaPipe, CommonModule, RouterModule, MatIconModule, MatTooltipModule, ActividadRecienteComponent],
  templateUrl: './hoy.component.html',
  styleUrl: './hoy.component.scss',
})
export class HoyComponent implements OnInit, OnDestroy {
  private readonly service = inject(HoyService);
  private readonly router = inject(Router);
  readonly config = inject(SiteConfigService);

  readonly datos = signal<CentroControl | null>(null);
  readonly cargado = signal(false);
  readonly error = signal(false);
  readonly actualizado = signal<Date | null>(null);

  readonly fechaLarga = new Intl.DateTimeFormat('es-UY', { weekday: 'long', day: 'numeric', month: 'long' }).format(new Date());
  readonly skeletonRows = Array(5).fill(0);

  private refresco: ReturnType<typeof setInterval> | null = null;

  ngOnInit(): void {
    this.cargar();
    this.refresco = setInterval(() => this.cargar(), REFRESCO_MS);
  }

  ngOnDestroy(): void {
    if (this.refresco) { clearInterval(this.refresco); }
  }

  cargar(): void {
    this.service.centroControl().subscribe({
      next: (d) => {
        this.datos.set(d);
        this.error.set(false);
        this.cargado.set(true);
        this.actualizado.set(new Date());
      },
      error: (err: unknown) => {
        console.error('[Hoy] Error cargando el centro de control:', err);
        this.error.set(true);
        this.cargado.set(true);
      },
    });
  }

  // ═══════════ Cabecera ═══════════

  get saludo(): string {
    const h = new Date().getHours();
    if (h < 6) { return 'Buenas noches'; }
    if (h < 13) { return 'Buen día'; }
    if (h < 20) { return 'Buenas tardes'; }
    return 'Buenas noches';
  }

  readonly kpis = computed(() => this.datos()?.kpis ?? null);
  readonly acciones = computed<Accion[]>(() => this.datos()?.acciones ?? []);
  readonly vendedores = computed<VendedorResumen[]>(() => this.datos()?.vendedores ?? []);
  readonly topProductos = computed(() => this.datos()?.topProductos ?? []);
  readonly stockPorDeposito = computed(() => this.datos()?.stockPorDeposito ?? []);

  /** Texto del KPI de ventas: variación contra el mes anterior en lenguaje de negocio. */
  readonly variacionTexto = computed(() => {
    const k = this.kpis();
    if (!k) { return ''; }
    if (k.variacionMesPorcentaje === null) {
      return k.ventasMesAnteriorUsd === 0 ? 'sin ventas el mes pasado' : '';
    }
    const v = k.variacionMesPorcentaje;
    const abs = Math.abs(v).toLocaleString('es-UY', { maximumFractionDigits: 1 });
    if (v > 0) { return `+${abs} % vs mes anterior`; }
    if (v < 0) { return `−${abs} % vs mes anterior`; }
    return 'igual que el mes anterior';
  });

  readonly acentoVentas = computed(() => {
    const v = this.kpis()?.variacionMesPorcentaje ?? null;
    if (v === null) { return 'primary'; }
    return v < 0 ? 'warning' : 'success';
  });

  /** El KPI muestra lo COLOCADO; el tooltip aclara cuánto ya se entregó (base de comisiones). */
  readonly tooltipVentas = computed(() => {
    const k = this.kpis();
    if (!k) { return ''; }
    return `Pedidos del mes sin borradores ni anulados. Entregado: ${usd(k.ventasEntregadasMesUsd)} · mes anterior: ${usd(k.ventasMesAnteriorUsd)}`;
  });

  readonly acentoStock = computed(() => {
    const k = this.kpis();
    if (!k) { return 'primary'; }
    if (k.skuStockNegativo > 0) { return 'danger'; }
    return k.skuStockBajo > 0 ? 'warning' : 'success';
  });

  readonly subStock = computed(() => {
    const k = this.kpis();
    if (!k) { return ''; }
    if (k.skuStockNegativo > 0) {
      return `${k.skuStockNegativo} en negativo · umbral ${this.datos()?.umbralStockBajo ?? 3}`;
    }
    return k.skuStockBajo > 0 ? `por debajo de ${this.datos()?.umbralStockBajo ?? 3} unidades` : 'nada por reponer';
  });

  readonly subSla = computed(() => {
    const k = this.kpis();
    if (!k) { return ''; }
    if (k.enviosFueraSla === 0) { return 'todos los envíos en plazo'; }
    const v = k.enviosSlaVencidos;
    const a = k.enviosFueraSla - v;
    const partes: string[] = [];
    if (v > 0) { partes.push(`${v} ${v === 1 ? 'vencido' : 'vencidos'}`); }
    if (a > 0) { partes.push(`${a} por vencer`); }
    return partes.join(' · ');
  });

  // ═══════════ Pipeline ═══════════

  readonly pipeline = computed<PipelineEstado[]>(() => this.datos()?.pipeline ?? []);
  readonly pipelineTotal = computed(() => this.pipeline().reduce((s, p) => s + (p.totalUsd ?? 0), 0));
  readonly pipelineCantidad = computed(() => this.pipeline().reduce((s, p) => s + (p.cantidad ?? 0), 0));
  private readonly pipelineMax = computed(() => Math.max(1, ...this.pipeline().map(p => p.totalUsd ?? 0)));

  pctPipeline(p: PipelineEstado): number {
    return ((p.totalUsd ?? 0) / this.pipelineMax()) * 100;
  }

  /** Índice de color del estado (misma escala estado-N de las listas generadas). */
  estadoIndice(estado: string): number {
    return ['borrador', 'confirmado', 'preparado', 'despachado', 'entregado'].indexOf(estado);
  }

  // ═══════════ Equipo ═══════════

  /** ok si va al ritmo del mes, advertencia si está 20 puntos abajo, vencido si está a menos de la mitad del ritmo. */
  semaforoMeta(v: VendedorResumen): 'ok' | 'advertencia' | 'vencido' | 'sin-meta' {
    if (!v.objetivoUsd || v.objetivoUsd <= 0) { return 'sin-meta'; }
    if (v.avancePorcentaje >= 100) { return 'ok'; }
    const hoy = new Date();
    const diasMes = new Date(hoy.getFullYear(), hoy.getMonth() + 1, 0).getDate();
    const ritmo = (hoy.getDate() / diasMes) * 100;
    if (v.avancePorcentaje >= ritmo - 20) { return 'ok'; }
    if (v.avancePorcentaje >= ritmo / 2) { return 'advertencia'; }
    return 'vencido';
  }

  pctMeta(v: VendedorResumen): number {
    return Math.max(0, Math.min(100, v.avancePorcentaje ?? 0));
  }

  /** Dónde cae hoy dentro del mes (0..100): el tick "esperado a hoy" de la barra de meta. */
  ritmoMes(): number {
    const hoy = new Date();
    const diasMes = new Date(hoy.getFullYear(), hoy.getMonth() + 1, 0).getDate();
    return Math.round((hoy.getDate() / diasMes) * 100);
  }

  // ═══════════ Top productos / stock ═══════════

  private readonly topMax = computed(() => Math.max(1, ...this.topProductos().map(p => p.totalUsd ?? 0)));
  pctTop(totalUsd: number): number { return ((totalUsd ?? 0) / this.topMax()) * 100; }

  private readonly stockMax = computed(() => Math.max(1, ...this.stockPorDeposito().map(d => d.unidades ?? 0)));
  pctStock(unidades: number): number { return (Math.max(0, unidades ?? 0) / this.stockMax()) * 100; }
  readonly stockTotal = computed(() => this.stockPorDeposito().reduce((s, d) => s + (d.unidades ?? 0), 0));

  // ═══════════ Feed ═══════════

  ir(a: Accion): void {
    if (a.ruta) { this.router.navigateByUrl(a.ruta); }
  }

  trackAccion(i: number, a: Accion): string { return a.tipo + '|' + a.ruta; }

  /** Fecha corta del feed: "hoy", "ayer", "hace N días" o dd/MM. */
  cuando(fecha: string | null): string {
    if (!fecha) { return ''; }
    const d = new Date(fecha);
    const hoy = new Date();
    const diff = Math.floor((this.soloDia(hoy).getTime() - this.soloDia(d).getTime()) / 86_400_000);
    if (diff <= 0) { return 'hoy'; }
    if (diff === 1) { return 'ayer'; }
    if (diff < 30) { return `hace ${diff} días`; }
    return d.toLocaleDateString('es-UY', { day: '2-digit', month: '2-digit' });
  }

  private soloDia(d: Date): Date { return new Date(d.getFullYear(), d.getMonth(), d.getDate()); }

  formato(n: number): string {
    return new Intl.NumberFormat('es-UY', { maximumFractionDigits: 0 }).format(n ?? 0);
  }
}
