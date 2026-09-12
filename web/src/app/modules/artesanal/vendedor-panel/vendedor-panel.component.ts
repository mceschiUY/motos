import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { etiqueta } from '../comun/etiquetas';
import { EtiquetaPipe } from '../comun/etiquetas';
import { UsdPipe } from '../comun/usd.pipe';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { VendedorService } from '../../generated/services/vendedor.service';
import { Vendedor } from '../../generated/models/vendedor.model';
import { VendedorFormComponent } from '../../generated/components/vendedor/vendedor-form/vendedor-form.component';
import { ActividadFormComponent } from '../../generated/components/actividad/actividad-form/actividad-form.component';
import { AgendaService } from '../agenda/agenda.service';
import { AvanceVendedor, ParadaAgenda } from '../agenda/agenda.model';
import { VendedorPanelService } from './vendedor-panel.service';
import { PanelVendedor, PanelVendedorCliente, PanelVendedorSemana } from './vendedor-panel.model';

type Semaforo = 'verde' | 'amarillo' | 'rojo' | 'sin-meta';

interface GrupoDia { fecha: string; label: string; esHoy: boolean; paradas: ParadaAgenda[]; }
interface BarraSemana extends PanelVendedorSemana { label: string; altura: number; esActual: boolean; }

/**
 * Panel del vendedor — escena artesanal (`/vendedor/:id`, pisa la ficha generada).
 * "Lo mío" para el vendedor y "quién vendió" para la gerencia: meta del mes con semáforo,
 * comisión sellada y proyectada, agenda de la semana, cartera con salud y pedidos abiertos.
 * Reutiliza `/artesanal/vendedor/{id}/avance` y `/artesanal/agenda` (AgendaService) y suma
 * `/artesanal/vendedor/{id}/panel` (VendedorPanelService). No escribe: las altas y ediciones
 * van por los forms generados (Vendedor, Actividad) en diálogo.
 */
@Component({
  selector: 'app-vendedor-panel',
  standalone: true,
  imports: [UsdPipe, EtiquetaPipe, 
    CommonModule, FormsModule,
    MatIconModule, MatButtonModule, MatTooltipModule,
    MatProgressSpinnerModule, MatSnackBarModule, MatDialogModule,
  ],
  templateUrl: './vendedor-panel.component.html',
  styleUrl: './vendedor-panel.component.scss',
})
export class VendedorPanelComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly panelService = inject(VendedorPanelService);
  private readonly agendaService = inject(AgendaService);
  private readonly vendedorService = inject(VendedorService);

  readonly isLoading = signal(true);
  readonly notFound = signal(false);
  readonly panel = signal<PanelVendedor | null>(null);
  readonly avance = signal<AvanceVendedor | null>(null);
  readonly paradas = signal<ParadaAgenda[]>([]);
  /** Slider "¿y si vende US$ X más?": solo front, no persiste. */
  readonly extraUsd = signal(0);

  vendedorId = 0;
  readonly hoy = new Date();
  readonly hoyIso = this.iso(this.hoy);

  readonly vendedor = computed(() => this.panel()?.vendedor ?? null);

  // ─── Meta del mes ────────────────────────────────────────────────────────
  readonly objetivo = computed(() => this.avance()?.objetivoUsd ?? 0);
  readonly vendido = computed(() => this.avance()?.vendidoUsd ?? 0);
  readonly comisionSellada = computed(() => this.avance()?.comisionUsd ?? 0);
  readonly porcentajeComision = computed(() => this.panel()?.vendedor.comisionPorcentaje ?? 0);

  /** Cuánto debería llevar vendido a la fecha si el mes fuera parejo. */
  readonly esperadoHoy = computed(() => {
    const diasMes = new Date(this.hoy.getFullYear(), this.hoy.getMonth() + 1, 0).getDate();
    return this.objetivo() * this.hoy.getDate() / diasMes;
  });
  readonly porcentajeReal = computed(() => this.pct(this.vendido()));
  readonly semaforoMeta = computed<Semaforo>(() => this.semaforoDe(this.vendido()));

  readonly maxExtra = computed(() => {
    const falta = this.objetivo() - this.vendido();
    const base = falta > 0 ? falta : this.objetivo() || 5000;
    return Math.ceil(base * 1.5 / 500) * 500;
  });
  readonly vendidoSimulado = computed(() => this.vendido() + this.extraUsd());
  readonly porcentajeSimulado = computed(() => this.pct(this.vendidoSimulado()));
  readonly semaforoSimulado = computed<Semaforo>(() => this.semaforoDe(this.vendidoSimulado()));
  readonly comisionSimulada = computed(() => this.comisionSellada() + this.extraUsd() * this.porcentajeComision() / 100);
  readonly faltaParaMeta = computed(() => Math.max(0, this.objetivo() - this.vendidoSimulado()));

  // ─── Ventas por semana ───────────────────────────────────────────────────
  readonly barras = computed<BarraSemana[]>(() => {
    const semanas = this.panel()?.ventasPorSemana ?? [];
    const max = Math.max(0, ...semanas.map(s => s.totalUsd));
    return semanas.map((s, i) => ({
      ...s,
      label: this.labelSemana(s.semanaInicio),
      altura: max > 0 ? Math.max(s.totalUsd > 0 ? 4 : 0, Math.round(s.totalUsd / max * 100)) : 0,
      esActual: i === semanas.length - 1,
    }));
  });
  readonly totalSemanas = computed(() => this.barras().reduce((acc, b) => acc + b.totalUsd, 0));

  // ─── Esta semana (agenda hoy..+7) ────────────────────────────────────────
  readonly gruposAgenda = computed<GrupoDia[]>(() => {
    const porDia = new Map<string, ParadaAgenda[]>();
    for (const p of this.paradas()) {
      const k = p.fecha.substring(0, 10);
      if (!porDia.has(k)) porDia.set(k, []);
      porDia.get(k)!.push(p);
    }
    return [...porDia.entries()].sort(([a], [b]) => a.localeCompare(b)).map(([fecha, paradas]) => ({
      fecha, paradas, label: this.labelDia(fecha), esHoy: fecha === this.hoyIso,
    }));
  });
  readonly planificadas = computed(() => this.paradas().filter(p => p.tipo === 'planificada').length);

  // ─── Cartera ─────────────────────────────────────────────────────────────
  /** Primero los que hay que atender (rojo), después amarillo y verde; dentro, por nombre. */
  readonly clientes = computed<PanelVendedorCliente[]>(() => {
    const peso = { rojo: 0, amarillo: 1, verde: 2 } as Record<string, number>;
    return [...(this.panel()?.clientes ?? [])].sort((a, b) =>
      (peso[a.semaforo] ?? 9) - (peso[b.semaforo] ?? 9) || (a.nombre ?? '').localeCompare(b.nombre ?? '', 'es'));
  });
  readonly clientesRojos = computed(() => this.clientes().filter(c => c.semaforo === 'rojo').length);
  readonly totalCarteraAnio = computed(() => this.clientes().reduce((acc, c) => acc + c.totalAnioUsd, 0));
  readonly totalAbiertos = computed(() => (this.panel()?.pedidosAbiertos ?? []).reduce((acc, p) => acc + p.totalUsd, 0));

  readonly rankingLabel = computed(() => {
    const r = this.panel()?.ranking;
    if (!r || !r.posicion) return null;
    return `${r.posicion}.º de ${r.total}`;
  });

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.vendedorId = Number(params.get('id'));
      this.extraUsd.set(0);
      this.cargar();
    });
  }

  cargar(): void {
    if (!this.vendedorId) { this.notFound.set(true); this.isLoading.set(false); return; }
    this.isLoading.set(true);
    this.notFound.set(false);
    const hasta = new Date(this.hoy); hasta.setDate(this.hoy.getDate() + 7);
    forkJoin({
      panel: this.panelService.panel(this.vendedorId),
      avance: this.agendaService.avance(this.vendedorId).pipe(catchError(() => of(null))),
      agenda: this.agendaService.agenda(this.vendedorId, this.hoyIso, this.iso(hasta)).pipe(catchError(() => of([] as ParadaAgenda[]))),
    }).subscribe({
      next: ({ panel, avance, agenda }) => {
        this.panel.set(panel);
        this.avance.set(avance);
        this.paradas.set(agenda ?? []);
        this.isLoading.set(false);
      },
      error: (err: unknown) => {
        console.error('[VendedorPanel] Error cargando el panel:', err);
        this.panel.set(null);
        this.notFound.set(true);
        this.isLoading.set(false);
      },
    });
  }

  // ─── Acciones ────────────────────────────────────────────────────────────
  verAgenda(): void { this.router.navigate(['/agenda'], { queryParams: { vendedorId: this.vendedorId } }); }
  verActividades(): void { this.router.navigate(['/actividad'], { queryParams: { vendedorId: this.vendedorId } }); }
  verComisiones(): void { this.router.navigate(['/comisiones']); }
  verCliente(clienteId: number): void { this.router.navigate(['/cliente', clienteId]); }
  verPedido(pedidoId: number): void { this.router.navigate(['/pedido', pedidoId]); }
  nuevoPedido(clienteId?: number): void {
    this.router.navigate(['/pedidos/nuevo'], clienteId ? { queryParams: { clienteId } } : {});
  }
  volver(): void { this.router.navigate(['/vendedor']); }

  editar(): void {
    this.vendedorService.getById(this.vendedorId).subscribe({
      next: (item: Vendedor | undefined) => {
        if (!item) { this.mensaje('No se pudo leer el vendedor', 'error'); return; }
        const ref = this.dialog.open(VendedorFormComponent, {
          width: '600px', maxWidth: '95vw', panelClass: 'crm-dialog',
          data: { item, mode: 'edit' },
        });
        ref.afterClosed().subscribe((ok: unknown) => { if (ok) { this.mensaje('Datos guardados', 'success'); this.cargar(); } });
      },
      error: () => this.mensaje('No se pudo leer el vendedor', 'error'),
    });
  }

  /**
   * Alta de actividad con el cliente fijado. `contextoFk` admite UN solo campo: se fija
   * clienteId; el vendedor lo preselecciona el propio form si el logueado es vendedor.
   */
  registrarVisita(clienteId?: number): void {
    const contextoFk = clienteId ? { campo: 'clienteId', valor: clienteId } : null;
    const ref = this.dialog.open(ActividadFormComponent, {
      width: '600px', maxWidth: '95vw', panelClass: 'crm-dialog', autoFocus: true,
      data: { item: null, mode: 'create', contextoFk },
    });
    ref.afterClosed().subscribe(res => { if (res) { this.mensaje('Actividad registrada', 'success'); this.cargar(); } });
  }

  onExtraChange(valor: string | number): void { this.extraUsd.set(Number(valor) || 0); }

  // ─── Etiquetas ───────────────────────────────────────────────────────────
  etiquetaSemaforo(s: Semaforo): string {
    return ({ verde: 'En ritmo', amarillo: 'Atrasado', rojo: 'Lejos de la meta', 'sin-meta': 'Sin meta cargada' } as Record<Semaforo, string>)[s];
  }
  etiquetaSalud(s: string): string {
    return ({ verde: 'Al día', amarillo: 'Enfriándose', rojo: 'Sin visitar' } as Record<string, string>)[s] ?? s;
  }
  etiquetaResultado(r: string): string {
    return etiqueta(r);
  }
  iconoTipo(t: string): string {
    return ({ visita: 'storefront', llamada: 'call', whatsapp: 'chat', email: 'mail' } as Record<string, string>)[t] ?? 'event_note';
  }
  diasSinVisitaLabel(c: PanelVendedorCliente): string {
    if (c.diasSinVisita == null) return 'nunca';
    if (c.diasSinVisita === 0) return 'hoy';
    return `${c.diasSinVisita} d`;
  }

  // ─── Helpers ─────────────────────────────────────────────────────────────
  private pct(vendido: number): number {
    const obj = this.objetivo();
    return obj > 0 ? Math.round(vendido / obj * 100) : 0;
  }

  private semaforoDe(vendido: number): Semaforo {
    if (this.objetivo() <= 0) return 'sin-meta';
    if (vendido >= this.objetivo()) return 'verde';
    const esperado = this.esperadoHoy();
    if (esperado <= 0) return 'verde';
    const ratio = vendido / esperado;
    return ratio >= 0.8 ? 'verde' : ratio >= 0.5 ? 'amarillo' : 'rojo';
  }

  private iso(d: Date): string {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }

  private labelDia(iso: string): string {
    const [y, m, d] = iso.split('-').map(Number);
    const f = new Date(y, m - 1, d);
    const hoy = new Date(); hoy.setHours(0, 0, 0, 0);
    const diff = Math.round((f.getTime() - hoy.getTime()) / 86400000);
    const base = new Intl.DateTimeFormat('es-UY', { weekday: 'long', day: 'numeric', month: 'short' }).format(f);
    if (diff === 0) return `Hoy · ${base}`;
    if (diff === 1) return `Mañana · ${base}`;
    return base.charAt(0).toUpperCase() + base.slice(1);
  }

  private labelSemana(iso: string): string {
    const [y, m, d] = iso.substring(0, 10).split('-').map(Number);
    return `${String(d).padStart(2, '0')}/${String(m).padStart(2, '0')}`;
  }

  private mensaje(texto: string, tipo: 'success' | 'error'): void {
    this.snackBar.open(texto, 'Cerrar', { duration: 3000, panelClass: tipo === 'success' ? 'snackbar-success' : 'snackbar-error' });
  }
}
