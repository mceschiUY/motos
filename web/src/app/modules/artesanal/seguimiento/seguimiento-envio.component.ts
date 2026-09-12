import { Component, ElementRef, OnInit, ViewChild, computed, inject, signal } from '@angular/core';
import { EtiquetaPipe } from '../comun/etiquetas';
import { UsdPipe } from '../comun/usd.pipe';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Observable, switchMap } from 'rxjs';
import QRCode from 'qrcode';
import { MembreteImpresionComponent } from '../../../shared/components/membrete-impresion/membrete-impresion.component';
import { confirmar } from '../../../shared/confirm/confirmar';
import { EnvioApiService } from '../../generated/services/envio.api.service';
import { Envio } from '../../generated/models/envio.model';
import { ObservacionFormComponent } from '../../generated/components/observacion/observacion-form/observacion-form.component';
import { SeguimientoService } from './seguimiento.service';
import { SeguimientoEnvio, SeguimientoEtapa, estimarEntrega } from './seguimiento.model';

/** Un movimiento del ciclo que se ofrece desde el estado actual. */
interface AccionCiclo {
  destino: 'facturado' | 'despachado' | 'entregado' | 'anulado';
  label: string;
  icon: string;
  guia: string;
  peligro?: boolean;
}

/**
 * Escena "Seguimiento del envío" (interna). Pisa la ficha generada de Envío (`/envio/:id`):
 * línea de estados estilo courier con SLA por etapa, pedido origen, bitácora, acciones de
 * ciclo y el link/QR público que se le comparte al cliente.
 *
 * Solo lectura por su endpoint (`api/artesanal/envio/{id}/seguimiento`); las escrituras van
 * por los commands generados: transiciones con EnvioApiService, observación con el form
 * generado en diálogo, y el motivo de anulación con el PUT del envío antes de anular
 * (la transición `pasar-a-anulado` no acepta payload).
 */
@Component({
  selector: 'app-seguimiento-envio',
  standalone: true,
  imports: [UsdPipe, EtiquetaPipe, 
    CommonModule, FormsModule, RouterLink,
    MatIconModule, MatButtonModule, MatTooltipModule, MatProgressSpinnerModule,
    MatSnackBarModule, MatDialogModule,
    MembreteImpresionComponent,
  ],
  templateUrl: './seguimiento-envio.component.html',
  styleUrl: './seguimiento-envio.component.scss',
})
export class SeguimientoEnvioComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly service = inject(SeguimientoService);
  private readonly envioApi = inject(EnvioApiService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  @ViewChild('qrCanvas') qrCanvas?: ElementRef<HTMLCanvasElement>;

  readonly isLoading = signal(true);
  readonly noEncontrado = signal(false);
  readonly datos = signal<SeguimientoEnvio | null>(null);
  readonly ejecutando = signal(false);
  readonly anulando = signal(false);
  readonly compartiendo = signal(false);
  readonly qrFallo = signal(false);
  motivoAnulacion = '';
  id = 0;

  /** Atajo para el template (solo se usa dentro del @else que ya garantiza datos()). */
  get d(): SeguimientoEnvio { return this.datos()!; }

  private readonly acciones: Record<string, AccionCiclo[]> = {
    recibido: [
      { destino: 'facturado', label: 'Confirmar', icon: 'check_circle', guia: 'Confirma el envío y sella la fecha de confirmación' },
      { destino: 'anulado', label: 'Anular', icon: 'cancel', guia: 'Motivo de anulación obligatorio', peligro: true },
    ],
    facturado: [
      { destino: 'despachado', label: 'Despachar', icon: 'local_shipping', guia: 'Sale del depósito; sella la fecha de envío' },
      { destino: 'anulado', label: 'Anular', icon: 'cancel', guia: 'Motivo de anulación obligatorio', peligro: true },
    ],
    despachado: [
      { destino: 'entregado', label: 'Entregar', icon: 'task_alt', guia: 'Confirma la entrega, cierra el ciclo y arrastra al pedido' },
      { destino: 'anulado', label: 'Anular', icon: 'cancel', guia: 'Motivo de anulación obligatorio', peligro: true },
    ],
  };

  /** Movimientos permitidos desde el estado actual (matriz de ciclos-vida.json). */
  readonly accionesPosibles = computed<AccionCiclo[]>(() => this.acciones[this.datos()?.estado ?? ''] ?? []);
  readonly etapaEnCurso = computed<SeguimientoEtapa | null>(() => this.datos()?.etapas.find(e => e.actual && !e.cumplida) ?? null);
  readonly cerrado = computed(() => { const e = this.datos()?.estado; return e === 'entregado' || e === 'anulado'; });
  readonly linkPublico = computed(() => { const c = this.datos()?.codigoRastreo; return c ? this.service.linkPublico(c) : ''; });
  readonly unidades = computed(() => (this.datos()?.pedido?.lineas ?? []).reduce((s, l) => s + Number(l.cantidad || 0), 0));
  /** Fecha estimada de entrega (revisión de escenas 2026-09-12). */
  readonly eta = computed<Date | null>(() => { const d = this.datos(); return d ? estimarEntrega(d.etapas, d.estado) : null; });
  imagen(id: number | null): string | null { return id != null ? this.service.imagenPublicaUrl(id) : null; }
  /** Le manda al cliente el estado y el link público por WhatsApp. */
  avisarCliente(): void {
    const d = this.datos(); if (!d) return;
    const tel = (d.cliente.telefono || '').replace(/[^0-9]/g, '');
    const eta = this.eta();
    const estado = (d.estadoLabel || d.estado).toLowerCase();
    const texto = [
      `Hola ${d.cliente.nombre ?? ''}, tu envío ${d.codigoRastreo} ${d.estado === 'despachado' ? 'ya salió' : 'está ' + estado}${d.agencia.nombre ? ' por ' + d.agencia.nombre : ''}.`,
      eta ? `Llegaría alrededor del ${eta.getDate()}/${eta.getMonth() + 1}.` : '',
      `Seguilo acá: ${this.linkPublico()}`,
    ].filter(Boolean).join(' ');
    window.open(`https://wa.me/${tel}?text=${encodeURIComponent(texto)}`, '_blank', 'noopener');
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe(pm => {
      this.id = Number(pm.get('id') ?? 0);
      this.compartiendo.set(false);
      this.anulando.set(false);
      this.cargar();
    });
  }

  cargar(): void {
    if (!this.id) { this.noEncontrado.set(true); this.isLoading.set(false); return; }
    this.isLoading.set(true);
    this.service.interno(this.id).subscribe({
      next: (d: SeguimientoEnvio) => { this.datos.set(d); this.noEncontrado.set(false); this.isLoading.set(false); },
      error: (e: unknown) => { console.error('[SeguimientoEnvio] cargar:', e); this.datos.set(null); this.noEncontrado.set(true); this.isLoading.set(false); },
    });
  }

  volver(): void { this.router.navigate(['/envio']); }
  imprimir(): void { window.print(); }

  /** Etiqueta del semáforo SLA para pills y tooltips. */
  labelSla(sla: string | null | undefined): string {
    switch (sla) {
      case 'ok': return 'En plazo';
      case 'advertencia': return 'Atención';
      case 'vencido': return 'Vencido';
      default: return 'Sin SLA';
    }
  }

  /** Texto de los días de una etapa: en curso ("hace 3 días") o cumplida ("tardó 2 días"). */
  textoDias(e: SeguimientoEtapa): string {
    if (e.diasTranscurridos == null) { return ''; }
    const n = e.diasTranscurridos;
    const dias = n === 1 ? '1 día' : `${n} días`;
    return e.cumplida ? `tardó ${dias}` : (n === 0 ? 'hoy' : `hace ${dias}`);
  }

  // ═══ Ciclo de vida ═══

  async ejecutar(a: AccionCiclo): Promise<void> {
    if (this.ejecutando()) { return; }
    if (a.destino === 'anulado') { this.anulando.set(true); this.motivoAnulacion = ''; return; }
    const codigo = this.datos()?.codigoRastreo ?? '';
    const ok = await confirmar({ titulo: `${a.label} el envío ${codigo}`, mensaje: `${a.guia}.\n\n¿Continuar?`, textoConfirmar: a.label });
    if (!ok) { return; }
    this.correr(this.accionDe(a.destino), `Envío ${a.label.toLowerCase()}`);
  }

  cancelarAnulacion(): void { this.anulando.set(false); this.motivoAnulacion = ''; }

  /** El motivo se guarda con el PUT del envío (Modificar conserva el estado) y recién después se anula. */
  async confirmarAnulacion(): Promise<void> {
    const motivo = this.motivoAnulacion.trim();
    if (!motivo) { this.aviso('Indicá el motivo de la anulación'); return; }
    const codigo = this.datos()?.codigoRastreo ?? '';
    const ok = await confirmar({ titulo: `Anular el envío ${codigo}`, mensaje: `Motivo: ${motivo}\n\nEsta acción no se puede deshacer.`, textoConfirmar: 'Anular', peligro: true });
    if (!ok) { return; }
    const flujo = this.envioApi.getById(this.id).pipe(
      switchMap((envio: Envio) => this.envioApi.update(this.id, { ...envio, motivoAnulacion: motivo })),
      switchMap(() => this.envioApi.pasarAAnulado(this.id)),
    );
    this.correr(flujo, 'Envío anulado');
    this.anulando.set(false);
  }

  private accionDe(destino: AccionCiclo['destino']): Observable<unknown> {
    switch (destino) {
      case 'facturado': return this.envioApi.pasarAFacturado(this.id);
      case 'despachado': return this.envioApi.pasarADespachado(this.id);
      case 'entregado': return this.envioApi.pasarAEntregado(this.id);
      case 'anulado': return this.envioApi.pasarAAnulado(this.id);
    }
  }

  private correr(flujo: Observable<unknown>, mensajeOk: string): void {
    this.ejecutando.set(true);
    flujo.subscribe({
      next: () => { this.ejecutando.set(false); this.aviso(mensajeOk); this.cargar(); },
      error: (err: any) => {
        this.ejecutando.set(false);
        const msg = err?.error?.[0]?.message || err?.error?.errors?.[0]?.message || err?.error?.message || err?.error?.detail || 'El sistema impidió este paso';
        this.aviso(msg);
      },
    });
  }

  // ═══ Bitácora ═══

  agregarObservacion(): void {
    const ref = this.dialog.open(ObservacionFormComponent, {
      width: '600px', maxWidth: '95vw', panelClass: 'crm-dialog', autoFocus: true,
      data: { item: null, mode: 'create', contextoFk: { campo: 'envioId', valor: this.id } },
    });
    ref.afterClosed().subscribe(res => { if (res) { this.aviso('Observación agregada'); this.cargar(); } });
  }

  // ═══ Compartir: link público + QR ═══

  alternarCompartir(): void {
    const abrir = !this.compartiendo();
    this.compartiendo.set(abrir);
    if (abrir) { setTimeout(() => this.dibujarQr(), 0); }
  }

  private dibujarQr(): void {
    const canvas = this.qrCanvas?.nativeElement;
    const link = this.linkPublico();
    if (!canvas || !link) { return; }
    QRCode.toCanvas(canvas, link, { width: 196, margin: 1 })
      .then(() => this.qrFallo.set(false))
      .catch((e: unknown) => { console.error('[SeguimientoEnvio] QR:', e); this.qrFallo.set(true); });
  }

  async copiarLink(): Promise<void> {
    const link = this.linkPublico();
    if (!link) { return; }
    try {
      await navigator.clipboard.writeText(link);
      this.aviso('Link copiado');
    } catch {
      this.aviso('No se pudo copiar; seleccioná el link y copialo a mano');
    }
  }

  abrirLink(): void { window.open(this.linkPublico(), '_blank', 'noopener'); }

  private aviso(msg: string): void { this.snackBar.open(msg, 'OK', { duration: 3500 }); }
}
