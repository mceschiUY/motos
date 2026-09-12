import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { usd, UsdPipe } from '../comun/usd.pipe';
import { etiqueta } from '../comun/etiquetas';
import { EtiquetaPipe } from '../comun/etiquetas';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MiniMapaComponent } from '../../../shared/components/mini-mapa/mini-mapa.component';
import { VendedorService } from '../../generated/services/vendedor.service';
import { ClienteService } from '../../generated/services/cliente.service';
import { Vendedor } from '../../generated/models/vendedor.model';
import { Cliente } from '../../generated/models/cliente.model';
import { ActividadFormComponent } from '../../generated/components/actividad/actividad-form/actividad-form.component';
import { AgendaService } from './agenda.service';
import { AvanceVendedor, ClienteSinVisitar, ParadaAgenda } from './agenda.model';

type Rango = 'hoy' | '7d' | '30d';

interface GrupoDia { fecha: string; label: string; esHoy: boolean; ciudades: GrupoCiudad[]; total: number; }
interface GrupoCiudad { ciudad: string; paradas: ParadaAgenda[]; }

/**
 * Agenda del vendedor — artesanal (doc/plan.md §4.2 y §4.8 "Ruta del día con mapa").
 * Hoy y próximos días por `ProximaAccion` (planificadas) más lo ya realizado, agrupado por
 * día y ciudad. Mapa de la parada elegida (mini-mapa, un punto) + ruta completa del día en
 * Google Maps (sin API key). "Registrar visita" abre el form generado de Actividad con el
 * cliente precargado. Filtro "mis clientes" = clientes del vendedor logueado (/Vendedor/mio).
 */
@Component({
  selector: 'app-agenda',
  standalone: true,
  imports: [UsdPipe, EtiquetaPipe, 
    CommonModule, FormsModule,
    MatIconModule, MatButtonModule, MatTooltipModule, MatFormFieldModule, MatSelectModule,
    MatProgressSpinnerModule, MatProgressBarModule, MatSnackBarModule, MatDialogModule,
    MiniMapaComponent,
  ],
  templateUrl: './agenda.component.html',
  styleUrl: './agenda.component.scss',
})
export class AgendaComponent implements OnInit {
  private readonly agendaService = inject(AgendaService);
  private readonly vendedorService = inject(VendedorService);
  private readonly clienteService = inject(ClienteService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly isLoading = signal(false);
  readonly paradas = signal<ParadaAgenda[]>([]);
  readonly vendedores = signal<Vendedor[]>([]);
  readonly vendedorId = signal<number | null>(null);
  readonly esMio = signal(false);
  readonly rango = signal<Rango>('7d');
  /** Celular (plan Etapa F5): el lateral (mapa, ruta del día, alertas) se pliega bajo un botón. */
  readonly mostrarLateral = signal(false);
  readonly seleccionada = signal<ParadaAgenda | null>(null);
  readonly avance = signal<AvanceVendedor | null>(null);
  /** Días de "sin visitar" del parámetro crm.dias_sin_visita (llega con el avance; 30 si no hay vendedor). */
  readonly diasAlerta = computed(() => this.avance()?.diasSinVisitaUmbral ?? 30);
  readonly sinVisitar = signal<ClienteSinVisitar[]>([]);
  readonly mostrarRealizadas = signal(true);

  readonly hoyIso = this.iso(new Date());

  readonly visibles = computed(() =>
    this.paradas().filter(p => this.mostrarRealizadas() || p.tipo === 'planificada'));

  readonly grupos = computed<GrupoDia[]>(() => {
    const porDia = new Map<string, ParadaAgenda[]>();
    for (const p of this.visibles()) {
      const k = p.fecha.substring(0, 10);
      if (!porDia.has(k)) porDia.set(k, []);
      porDia.get(k)!.push(p);
    }
    return [...porDia.entries()].sort(([a], [b]) => a.localeCompare(b)).map(([fecha, items]) => {
      const porCiudad = new Map<string, ParadaAgenda[]>();
      for (const p of items) {
        const c = p.ciudad || 'Sin ciudad';
        if (!porCiudad.has(c)) porCiudad.set(c, []);
        porCiudad.get(c)!.push(p);
      }
      return {
        fecha,
        label: this.labelDia(fecha),
        esHoy: fecha === this.hoyIso,
        total: items.length,
        ciudades: [...porCiudad.entries()].sort(([a], [b]) => a.localeCompare(b, 'es')).map(([ciudad, paradas]) => ({ ciudad, paradas })),
      };
    });
  });

  readonly paradasHoy = computed(() => this.visibles().filter(p => p.fecha.substring(0, 10) === this.hoyIso));
  readonly planificadas = computed(() => this.paradas().filter(p => p.tipo === 'planificada').length);
  readonly realizadas = computed(() => this.paradas().filter(p => p.tipo === 'realizada').length);

  /** Ruta del día en Google Maps con todas las paradas geolocalizadas de hoy (sin key). */
  readonly rutaHoyUrl = computed(() => {
    const puntos = this.paradasHoy()
      .filter(p => p.tipo === 'planificada' && p.latitud && p.longitud)
      .map(p => `${p.latitud},${p.longitud}`);
    const unicos = [...new Set(puntos)];
    return unicos.length >= 1 ? `https://www.google.com/maps/dir/${unicos.join('/')}` : null;
  });

  readonly porcentajeMeta = computed(() => {
    const a = this.avance();
    if (!a || !a.objetivoUsd) return 0;
    return Math.min(100, Math.round((a.vendidoUsd / a.objetivoUsd) * 100));
  });

  ngOnInit(): void {
    this.vendedorService.getAll().subscribe({
      next: (v: Vendedor[]) => this.vendedores.set(v.filter(x => x.activo !== false)),
      error: (err: unknown) => console.error('[Agenda] Error cargando vendedores:', err),
    });
    // `?vendedorId=N` (desde el panel del vendedor o el home) manda: se abre ESA agenda.
    // Si no viene, el vendedor del usuario logueado: la agenda arranca en "lo mío".
    const pedido = Number(this.route.snapshot.queryParamMap.get('vendedorId'));
    if (pedido > 0) {
      this.vendedorId.set(pedido);
      this.vendedorService.mio().subscribe({
        next: (v: Vendedor | null) => { this.esMio.set(!!v && v.id === pedido); this.loadData(); },
        error: () => this.loadData(),
      });
      return;
    }
    this.vendedorService.mio().subscribe({
      next: (v: Vendedor | null) => { if (v) { this.vendedorId.set(v.id); this.esMio.set(true); } this.loadData(); },
      error: () => this.loadData(),
    });
  }

  loadData(): void {
    this.isLoading.set(true);
    const { desde, hasta } = this.rangoFechas();
    this.agendaService.agenda(this.vendedorId(), desde, hasta).subscribe({
      next: (data) => {
        this.paradas.set(data ?? []);
        this.isLoading.set(false);
        const sel = this.seleccionada();
        if (!sel || !data.some(p => p.actividadId === sel.actividadId)) {
          this.seleccionada.set(data.find(p => p.tipo === 'planificada' && p.latitud) ?? data[0] ?? null);
        }
      },
      error: (err: unknown) => {
        console.error('[Agenda] Error cargando agenda:', err);
        this.paradas.set([]);
        this.isLoading.set(false);
        this.showMessage('Error al cargar la agenda', 'error');
      },
    });
    const vid = this.vendedorId();
    if (vid != null) {
      this.agendaService.avance(vid).subscribe({ next: a => this.avance.set(a), error: () => this.avance.set(null) });
    } else {
      this.avance.set(null);
    }
    this.agendaService.clientesSinVisitar().subscribe({
      // Los más abandonados primero (revisión de escenas 2026-09-12).
      next: c => this.sinVisitar.set((vid != null ? c.filter(x => x.vendedorId === vid) : c).sort((a, b) => b.diasSinVisita - a.diasSinVisita)),
      error: () => this.sinVisitar.set([]),
    });
  }

  onVendedorChange(id: number | null): void {
    this.vendedorId.set(id);
    this.esMio.set(false);
    this.loadData();
  }

  setRango(r: Rango): void { this.rango.set(r); this.loadData(); }

  seleccionar(p: ParadaAgenda): void { this.seleccionada.set(p); }

  /** Con parada o con cliente de la alerta ("Agendar"): el form sale con ese cliente fijado. */
  registrarVisita(p?: { clienteId: number }): void {
    const contextoFk = p ? { campo: 'clienteId', valor: p.clienteId } : null;
    const ref = this.dialog.open(ActividadFormComponent, {
      width: '600px', maxWidth: '95vw', panelClass: 'crm-dialog', autoFocus: true,
      data: { item: null, mode: 'create', contextoFk },
    });
    ref.afterClosed().subscribe(res => { if (res) { this.showMessage('Actividad registrada', 'success'); this.loadData(); } });
  }

  /** Guarda la ubicación actual del dispositivo como coordenadas del cliente de la parada. */
  usarMiUbicacion(p: ParadaAgenda): void {
    if (!('geolocation' in navigator)) { this.showMessage('Este dispositivo no tiene geolocalización', 'error'); return; }
    navigator.geolocation.getCurrentPosition(pos => {
      const lat = Number(pos.coords.latitude.toFixed(6));
      const lng = Number(pos.coords.longitude.toFixed(6));
      this.clienteService.getById(p.clienteId).subscribe({
        next: (c: Cliente | undefined) => {
          if (!c) { this.showMessage('No se pudo leer el cliente', 'error'); return; }
          const { id, ...resto } = c as any;
          this.clienteService.update(p.clienteId, { ...resto, latitud: lat, longitud: lng } as Partial<Cliente>).subscribe({
            next: () => { this.showMessage(`Ubicación guardada para ${p.clienteDisplay}`, 'success'); this.loadData(); },
            error: () => this.showMessage('No se pudo guardar la ubicación', 'error'),
          });
        },
        error: () => this.showMessage('No se pudo leer el cliente', 'error'),
      });
    }, () => this.showMessage('No se pudo obtener la ubicación', 'error'), { enableHighAccuracy: true, timeout: 10000 });
  }

  verCliente(p: ParadaAgenda | ClienteSinVisitar): void { this.router.navigate(['/cliente', p.clienteId]); }

  /** Contacto de un tap desde la parada (revisión de escenas 2026-09-12). */
  whatsapp(p: ParadaAgenda, ev: Event): void {
    ev.stopPropagation();
    const tel = (p.telefono || '').replace(/[^0-9]/g, '');
    if (!tel) return;
    const texto = `Hola ${p.clienteDisplay ?? ''}, te escribo de la distribuidora para coordinar la visita.`;
    window.open(`https://wa.me/${tel}?text=${encodeURIComponent(texto)}`, '_blank');
  }
  llamar(p: ParadaAgenda, ev: Event): void {
    ev.stopPropagation();
    const tel = (p.telefono || '').replace(/[^0-9+]/g, '');
    if (tel) window.location.href = `tel:${tel}`;
  }
  /** true si la parada es de un cliente ya en alerta de "sin visitar". */
  urgente(p: ParadaAgenda): boolean { return p.diasSinVisita == null || p.diasSinVisita > this.diasAlerta(); }
  verActividad(p: ParadaAgenda): void { this.router.navigate(['/actividad', p.actividadId]); }
  // Con vendedor elegido va a SU panel (cartera con semáforo); sin vendedor, a la lista de clientes.
  misClientes(): void {
    const vid = this.vendedorId();
    if (vid != null) { this.router.navigate(['/vendedor', vid]); return; }
    this.router.navigate(['/cliente']);
  }
  goBack(): void { this.router.navigate(['/']); }

  nombreVendedor(): string {
    const v = this.vendedores().find(x => x.id === this.vendedorId());
    return v?.nombre ?? 'Todos los vendedores';
  }

  etiquetaResultado(r: string): string {
    return etiqueta(r);
  }

  iconoTipo(t: string): string {
    return ({ visita: 'storefront', llamada: 'call', whatsapp: 'chat', email: 'mail' } as Record<string, string>)[t] ?? 'event_note';
  }

  formatoUsd(n: number): string {
    return usd(n, 0);
  }

  private rangoFechas(): { desde: string; hasta: string } {
    const hoy = new Date();
    const hasta = new Date(hoy);
    hasta.setDate(hoy.getDate() + (this.rango() === 'hoy' ? 0 : this.rango() === '7d' ? 7 : 30));
    const desde = new Date(hoy);
    desde.setDate(hoy.getDate() - (this.rango() === 'hoy' ? 0 : 7)); // realizadas de la última semana
    return { desde: this.iso(desde), hasta: this.iso(hasta) };
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
    if (diff === -1) return `Ayer · ${base}`;
    return base.charAt(0).toUpperCase() + base.slice(1);
  }

  private showMessage(message: string, type: 'success' | 'error'): void {
    this.snackBar.open(message, 'Cerrar', { duration: 3000, panelClass: type === 'success' ? 'snackbar-success' : 'snackbar-error' });
  }
}
