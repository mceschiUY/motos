import { Component, HostListener, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { UsdPipe } from '../modules/artesanal/comun/usd.pipe';
import { EtiquetaPipe } from '../modules/artesanal/comun/etiquetas';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { ActividadRecienteComponent } from '../shared/components/actividad-reciente/actividad-reciente.component';
import { SiteConfigService } from '../core/services/site-config.service';
import { HoyService } from '../modules/artesanal/hoy/hoy.service';
import { Accion, CentroControl, VendedorResumen } from '../modules/artesanal/hoy/hoy.model';

type SlideTipo = 'hoy' | 'equipo' | 'pedidos' | 'actividad';

/**
 * MODO PANTALLA: el negocio en la TV de la oficina (plan "escenas", Etapa D, 2026-09-12).
 * Rota entre los mismos datos del home "Hoy" (un solo endpoint `centro-control`):
 * KPI del día + qué hacer hoy, el equipo contra su meta, los pedidos del mes con el top de
 * productos, y la actividad reciente. Sin menú, letra grande, auto-refresh. Esc o ✕ para salir.
 */
@Component({
  selector: 'app-pantalla',
  standalone: true,
  imports: [UsdPipe, EtiquetaPipe, CommonModule, MatIconModule, ActividadRecienteComponent],
  template: `
    <div class="tv">
      <header class="tv-head">
        <span class="tv-marca">{{ config.sitioNombre() }}</span>
        <span class="tv-fecha">{{ fechaLarga }}</span>
        <span class="tv-reloj tnum">{{ hora() }}</span>
        <button class="tv-salir" (click)="salir()" title="Salir (Esc)"><mat-icon>close</mat-icon></button>
      </header>

      @switch (slideActual()) {
        @case ('hoy') {
          <div class="tv-slide">
            <section class="tv-kpis">
              <div class="tv-kpi" [attr.data-acento]="(kpis()?.pedidosADespachar ?? 0) > 0 ? 'warning' : 'success'">
                <mat-icon>local_shipping</mat-icon>
                <span class="tv-cifra tnum">{{ datos() ? kpis()!.pedidosADespachar : '—' }}</span>
                <span class="tv-label">Pedidos a despachar</span>
              </div>
              <div class="tv-kpi" data-acento="primary">
                <mat-icon>route</mat-icon>
                <span class="tv-cifra tnum">{{ datos() ? kpis()!.visitasHoy : '—' }}</span>
                <span class="tv-label">Visitas hoy</span>
              </div>
              <div class="tv-kpi" [attr.data-acento]="(kpis()?.enviosSlaVencidos ?? 0) > 0 ? 'danger' : (kpis()?.enviosFueraSla ?? 0) > 0 ? 'warning' : 'success'">
                <mat-icon>{{ (kpis()?.enviosFueraSla ?? 0) > 0 ? 'timer' : 'verified' }}</mat-icon>
                <span class="tv-cifra tnum">{{ datos() ? kpis()!.enviosFueraSla : '—' }}</span>
                <span class="tv-label">Envíos fuera de SLA</span>
              </div>
              <div class="tv-kpi" data-acento="success">
                <mat-icon>payments</mat-icon>
                <span class="tv-cifra tnum">{{ datos() ? (kpis()!.ventasMesUsd | usd:0) : '—' }}</span>
                <span class="tv-label">Ventas del mes</span>
              </div>
              <div class="tv-kpi" [attr.data-acento]="(kpis()?.skuStockNegativo ?? 0) > 0 ? 'danger' : (kpis()?.skuStockBajo ?? 0) > 0 ? 'warning' : 'success'">
                <mat-icon>inventory_2</mat-icon>
                <span class="tv-cifra tnum">{{ datos() ? (kpis()!.skuStockBajo + kpis()!.skuStockNegativo) : '—' }}</span>
                <span class="tv-label">SKU con stock bajo</span>
              </div>
            </section>

            <h2 class="tv-subtitulo"><mat-icon>checklist</mat-icon> Qué hacer hoy</h2>
            @if (datos() && acciones().length === 0) {
              <p class="tv-vacio"><mat-icon>verified</mat-icon> Todo en orden.</p>
            }
            <div class="tv-feed">
              @for (a of acciones(); track a.ruta + a.titulo) {
                <div class="tv-accion" [attr.data-acento]="a.acento">
                  <span class="tv-accion-icono"><mat-icon>{{ a.icono }}</mat-icon></span>
                  <span class="tv-accion-texto">
                    <span class="tv-accion-titulo">{{ a.titulo }}</span>
                    <span class="tv-accion-detalle">{{ a.detalle }}</span>
                  </span>
                </div>
              }
            </div>
          </div>
        }
        @case ('equipo') {
          <div class="tv-slide">
            <h1 class="tv-titulo"><mat-icon class="tv-titulo-icon">groups</mat-icon> El equipo contra la meta</h1>
            <div class="tv-equipo">
              @for (v of vendedores(); track v.id) {
                <div class="tv-vend" [attr.data-acento]="semaforoMeta(v)">
                  <div class="tv-vend-cab">
                    <span class="tv-vend-nombre">{{ v.nombre }}</span>
                    <span class="tv-vend-zona">{{ v.zona || '' }}</span>
                    <span class="tv-vend-pct tnum">{{ v.objetivoUsd > 0 ? entero(v.avancePorcentaje) + ' %' : 'sin meta' }}</span>
                  </div>
                  <div class="tv-barra"><div class="tv-fill" [style.width.%]="pctMeta(v)"></div></div>
                  <div class="tv-vend-pie tnum">
                    <span>vendió <strong>{{ v.vendidoMesUsd | usd:0 }}</strong>@if (v.objetivoUsd > 0) { de {{ v.objetivoUsd | usd:0 }}}</span>
                    <span>comisión <strong>{{ v.comisionMesUsd | usd }}</strong></span>
                    <span>{{ v.visitasMes }} {{ v.visitasMes === 1 ? 'visita' : 'visitas' }}</span>
                  </div>
                </div>
              }
              @if (datos() && vendedores().length === 0) { <p class="tv-vacio">No hay vendedores activos.</p> }
            </div>
          </div>
        }
        @case ('pedidos') {
          <div class="tv-slide tv-dos">
            <section>
              <h1 class="tv-titulo"><mat-icon class="tv-titulo-icon">receipt_long</mat-icon> Pedidos del mes</h1>
              <p class="tv-total tnum">{{ pipelineTotal() | usd:0 }} <small>{{ pipelineCantidad() }} {{ pipelineCantidad() === 1 ? 'pedido' : 'pedidos' }}</small></p>
              <div class="tv-estados">
                @for (p of pipeline(); track p.estado) {
                  <div class="tv-estado">
                    <span class="tv-estado-nombre">{{ p.estado | etiqueta }}</span>
                    <div class="tv-barra"><div class="tv-fill" [style.width.%]="pctPipeline(p.cantidad)"></div></div>
                    <span class="tv-estado-num tnum">{{ p.cantidad }}</span>
                  </div>
                }
              </div>
            </section>
            <section>
              <h1 class="tv-titulo"><mat-icon class="tv-titulo-icon">star</mat-icon> Top productos</h1>
              <div class="tv-top">
                @for (p of topProductos(); track p.productoId; let i = $index) {
                  <div class="tv-top-fila">
                    <span class="tv-top-pos tnum">{{ i + 1 }}</span>
                    <span class="tv-top-nombre">{{ p.nombre }} <small>{{ p.marca || '' }} · {{ entero(p.unidadesMes) }} u.</small></span>
                    <span class="tv-top-monto tnum">{{ p.totalUsd | usd:0 }}</span>
                  </div>
                }
                @if (datos() && topProductos().length === 0) { <p class="tv-vacio">Todavía no se vendió nada este mes.</p> }
              </div>
            </section>
          </div>
        }
        @case ('actividad') {
          <div class="tv-slide">
            <h1 class="tv-titulo"><mat-icon class="tv-titulo-icon">history</mat-icon> Recién pasó</h1>
            <div class="tv-actividad">
              <app-actividad-reciente [limite]="8"></app-actividad-reciente>
            </div>
          </div>
        }
      }

      <div class="tv-progreso">
        @for (s of slides; track s; let i = $index) {
          <span class="tv-punto" [class.activo]="i === indice()" (click)="indice.set(i)"></span>
        }
      </div>
    </div>
  `,
  styles: [`
    .tv { position: fixed; inset: 0; background: var(--ceskia-base); color: var(--ceskia-text-primary); z-index: 999; display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 9vh 5vw 6vh; }
    .tnum { font-variant-numeric: tabular-nums; }
    .tv-head { position: absolute; top: 0; left: 0; right: 0; display: flex; align-items: center; gap: 24px; padding: 18px 5vw; color: var(--ceskia-text-tertiary); font-size: 1.05rem; }
    .tv-marca { font-weight: 600; color: var(--ceskia-text-secondary); }
    .tv-fecha { text-transform: capitalize; }
    .tv-reloj { margin-left: auto; font-size: 1.4rem; font-weight: 600; color: var(--ceskia-text-primary); }
    .tv-salir { background: transparent; border: none; color: var(--ceskia-text-muted); cursor: pointer; opacity: .4; }
    .tv-salir:hover { opacity: 1; }
    .tv-slide { width: 100%; max-width: 1400px; animation: tv-in .6s ease; }
    @keyframes tv-in { from { opacity: 0; transform: translateY(14px); } to { opacity: 1; transform: none; } }
    .tv-titulo { font-size: 2.4rem; font-weight: 600; letter-spacing: -.02em; margin: 0 0 4vh 0; display: flex; align-items: center; gap: 16px; }
    .tv-titulo-icon { font-size: 2.2rem; width: 2.2rem; height: 2.2rem; color: var(--ceskia-accent-primary); }
    .tv-subtitulo { font-size: 1.5rem; font-weight: 600; margin: 4vh 0 2vh; display: flex; align-items: center; gap: 12px; color: var(--ceskia-text-secondary); mat-icon { color: var(--ceskia-accent-primary); } }

    /* KPI con acento */
    .tv-kpis { display: grid; grid-template-columns: repeat(5, 1fr); gap: 2vw; }
    .tv-kpi { display: flex; flex-direction: column; align-items: center; gap: 8px; padding: 3vh 1vw; border-radius: var(--ceskia-radius-xl); background: var(--ceskia-surface); border: 1px solid var(--ceskia-border-subtle); border-top: 4px solid var(--kpi-acento, var(--ceskia-accent-primary)); mat-icon { color: var(--kpi-acento, var(--ceskia-accent-primary)); font-size: 2.2rem; width: 2.2rem; height: 2.2rem; } }
    .tv-kpi[data-acento='primary'], .tv-accion[data-acento='primary'], .tv-vend[data-acento='primary'] { --kpi-acento: var(--ceskia-accent-primary); }
    .tv-kpi[data-acento='success'], .tv-accion[data-acento='success'], .tv-vend[data-acento='success'] { --kpi-acento: var(--ceskia-accent-success); }
    .tv-kpi[data-acento='warning'], .tv-accion[data-acento='warning'], .tv-vend[data-acento='warning'] { --kpi-acento: var(--ceskia-accent-warning); }
    .tv-kpi[data-acento='danger'],  .tv-accion[data-acento='danger'],  .tv-vend[data-acento='danger']  { --kpi-acento: var(--ceskia-accent-danger); }
    .tv-cifra { font-size: 3.6rem; font-weight: 650; line-height: 1; color: var(--ceskia-text-primary); }
    .tv-label { font-size: 1.05rem; color: var(--ceskia-text-tertiary); text-align: center; }

    /* Feed */
    .tv-feed { display: grid; grid-template-columns: 1fr 1fr; gap: 1.2vh 2vw; }
    .tv-accion { display: flex; align-items: center; gap: 16px; padding: 1.4vh 1.2vw; border-radius: var(--ceskia-radius-lg); background: var(--ceskia-surface); border: 1px solid var(--ceskia-border-subtle); border-left: 5px solid var(--kpi-acento, var(--ceskia-accent-primary)); }
    .tv-accion-icono { display: grid; place-items: center; width: 44px; height: 44px; border-radius: 50%; background: var(--ceskia-elevated); flex: none; mat-icon { color: var(--kpi-acento, var(--ceskia-accent-primary)); } }
    .tv-accion-texto { display: flex; flex-direction: column; gap: 3px; min-width: 0; }
    .tv-accion-titulo { font-size: 1.25rem; font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .tv-accion-detalle { font-size: 1rem; color: var(--ceskia-text-tertiary); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .tv-vacio { display: flex; align-items: center; gap: 10px; font-size: 1.4rem; color: var(--ceskia-text-tertiary); mat-icon { color: var(--ceskia-accent-success); } }

    /* Equipo */
    .tv-equipo { display: flex; flex-direction: column; gap: 3vh; }
    .tv-vend { padding: 2.2vh 1.6vw; border-radius: var(--ceskia-radius-xl); background: var(--ceskia-surface); border: 1px solid var(--ceskia-border-subtle); border-left: 6px solid var(--kpi-acento, var(--ceskia-accent-primary)); .tv-fill { background: var(--kpi-acento, var(--ceskia-accent-primary)); } }
    .tv-vend-cab { display: flex; align-items: baseline; gap: 18px; margin-bottom: 1.4vh; }
    .tv-vend-nombre { font-size: 1.9rem; font-weight: 600; }
    .tv-vend-zona { font-size: 1.1rem; color: var(--ceskia-text-tertiary); }
    .tv-vend-pct { margin-left: auto; font-size: 2.2rem; font-weight: 650; }
    .tv-vend-pie { display: flex; gap: 32px; margin-top: 1.4vh; font-size: 1.15rem; color: var(--ceskia-text-secondary); strong { color: var(--ceskia-text-primary); } }

    /* Pedidos + top */
    .tv-dos { display: grid; grid-template-columns: 1.1fr 1fr; gap: 4vw; }
    .tv-total { font-size: 3rem; font-weight: 650; margin: -2vh 0 3vh; small { font-size: 1.1rem; font-weight: 400; color: var(--ceskia-text-tertiary); margin-left: 12px; } }
    .tv-estados { display: flex; flex-direction: column; gap: 2.4vh; }
    .tv-estado { display: flex; align-items: center; gap: 24px; }
    .tv-estado-nombre { width: 180px; font-size: 1.5rem; text-transform: capitalize; color: var(--ceskia-text-secondary); }
    .tv-barra { flex: 1; height: 24px; background: var(--ceskia-elevated); border-radius: 12px; overflow: hidden; }
    .tv-fill { height: 100%; background: var(--ceskia-accent-primary); border-radius: 12px; transition: width .8s ease; }
    .tv-estado-num { width: 80px; text-align: right; font-size: 2rem; font-weight: 600; }
    .tv-top { display: flex; flex-direction: column; gap: 1.8vh; }
    .tv-top-fila { display: flex; align-items: center; gap: 18px; font-size: 1.35rem; }
    .tv-top-pos { width: 40px; height: 40px; border-radius: 50%; display: grid; place-items: center; background: var(--ceskia-elevated); color: var(--ceskia-accent-primary); font-weight: 700; flex: none; }
    .tv-top-nombre { flex: 1; min-width: 0; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; small { display: block; font-size: .95rem; color: var(--ceskia-text-tertiary); } }
    .tv-top-monto { font-weight: 600; }

    .tv-actividad { font-size: 1.2rem; ::ng-deep .ar-frase { font-size: 1.35rem; } ::ng-deep .ar-cuando { font-size: 1rem; } }
    .tv-progreso { position: absolute; bottom: 26px; display: flex; gap: 12px; }
    .tv-punto { width: 10px; height: 10px; border-radius: 50%; background: var(--ceskia-border-strong); transition: background .3s; cursor: pointer; }
    .tv-punto.activo { background: var(--ceskia-accent-primary); }

    @media (max-width: 1100px) {
      .tv-kpis { grid-template-columns: repeat(3, 1fr); }
      .tv-feed, .tv-dos { grid-template-columns: 1fr; }
      .tv-cifra { font-size: 2.6rem; }
    }
  `]
})
export class PantallaComponent implements OnInit, OnDestroy {
  private readonly router = inject(Router);
  private readonly servicio = inject(HoyService);
  readonly config = inject(SiteConfigService);

  readonly slides: SlideTipo[] = ['hoy', 'equipo', 'pedidos', 'actividad'];
  readonly indice = signal(0);
  readonly datos = signal<CentroControl | null>(null);
  readonly hora = signal('');
  readonly fechaLarga = new Intl.DateTimeFormat('es-UY', { weekday: 'long', day: 'numeric', month: 'long' }).format(new Date());

  readonly kpis = computed(() => this.datos()?.kpis ?? null);
  /** En la TV entran 8 acciones (dos columnas de cuatro). */
  readonly acciones = computed<Accion[]>(() => (this.datos()?.acciones ?? []).slice(0, 8));
  readonly vendedores = computed<VendedorResumen[]>(() => this.datos()?.vendedores ?? []);
  readonly pipeline = computed(() => this.datos()?.pipeline ?? []);
  readonly topProductos = computed(() => (this.datos()?.topProductos ?? []).slice(0, 5));
  readonly pipelineTotal = computed(() => this.pipeline().reduce((s, p) => s + p.totalUsd, 0));
  readonly pipelineCantidad = computed(() => this.pipeline().reduce((s, p) => s + p.cantidad, 0));

  private rotacion: ReturnType<typeof setInterval> | null = null;
  private refresco: ReturnType<typeof setInterval> | null = null;
  private reloj: ReturnType<typeof setInterval> | null = null;

  ngOnInit(): void {
    this.cargar();
    this.tic();
    this.rotacion = setInterval(() => this.indice.set((this.indice() + 1) % this.slides.length), 15000);
    this.refresco = setInterval(() => this.cargar(), 60000);
    this.reloj = setInterval(() => this.tic(), 1000);
  }

  ngOnDestroy(): void {
    for (const t of [this.rotacion, this.refresco, this.reloj]) { if (t) { clearInterval(t); } }
  }

  @HostListener('document:keydown.escape')
  salir(): void { this.router.navigate(['/']); }

  slideActual(): SlideTipo { return this.slides[this.indice()] ?? 'hoy'; }

  private cargar(): void {
    this.servicio.centroControl().subscribe({
      next: (d) => this.datos.set(d),
      error: (e) => console.error('[Pantalla] centro-control:', e),
    });
  }

  private tic(): void {
    this.hora.set(new Intl.DateTimeFormat('es-UY', { hour: '2-digit', minute: '2-digit' }).format(new Date()));
  }

  entero(n: number): string {
    return new Intl.NumberFormat('es-UY', { maximumFractionDigits: 0 }).format(n ?? 0);
  }

  pctPipeline(cantidad: number): number {
    const max = Math.max(1, ...this.pipeline().map(p => p.cantidad));
    return (cantidad / max) * 100;
  }

  pctMeta(v: VendedorResumen): number {
    return v.objetivoUsd > 0 ? Math.min(100, (v.vendidoMesUsd / v.objetivoUsd) * 100) : 0;
  }

  /** Mismo criterio que el home (`HoyComponent.semaforoMeta`): el avance contra el ritmo del
   *  mes (día / días del mes): a menos de 20 puntos, verde; a más de la mitad, amarillo; si no, rojo. */
  semaforoMeta(v: VendedorResumen): 'success' | 'warning' | 'danger' | 'primary' {
    if (!v.objetivoUsd || v.objetivoUsd <= 0) { return 'primary'; }
    if (v.avancePorcentaje >= 100) { return 'success'; }
    const hoy = new Date();
    const diasMes = new Date(hoy.getFullYear(), hoy.getMonth() + 1, 0).getDate();
    const ritmo = (hoy.getDate() / diasMes) * 100;
    if (v.avancePorcentaje >= ritmo - 20) { return 'success'; }
    if (v.avancePorcentaje >= ritmo / 2) { return 'warning'; }
    return 'danger';
  }
}
