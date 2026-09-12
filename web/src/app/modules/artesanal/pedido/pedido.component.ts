import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { MembreteImpresionComponent } from '../../../shared/components/membrete-impresion/membrete-impresion.component';
import { PedidoApiService } from '../../generated/services/pedido.api.service';
import { PedidoService } from '../../generated/services/pedido.service';
import { PedidoLineaService } from '../../generated/services/pedidolinea.service';
import { AgenciaService } from '../../generated/services/agencia.service';
import { Agencia } from '../../generated/models/agencia.model';
import { PedidoLineaFormComponent } from '../../generated/components/pedidolinea/pedidolinea-form/pedidolinea-form.component';
import { CatalogoService } from '../catalogo/catalogo.service';
import { EtiquetaPipe, etiqueta } from '../comun/etiquetas';
import { UsdPipe, usd } from '../comun/usd.pipe';
import { PedidoEscenaService } from './pedido.service';
import { FichaPedido, FichaPedidoLinea } from './pedido.model';

/** Un paso del ciclo (misma matriz que ciclos-vida.json y que el kanban generado). */
interface Paso {
  destino: string;
  label: string;
  icon: string;
  actor: string;
  condicion: string;
}

const PASOS: Record<string, Paso> = {
  confirmado: { destino: 'confirmado', label: 'Confirmar', icon: 'check_circle', actor: 'Vendedor', condicion: 'Al menos una línea. Si falta stock avisa, pero deja confirmar.' },
  preparado: { destino: 'preparado', label: 'Marcar preparado', icon: 'inventory', actor: 'Depósito', condicion: 'El depósito armó el bulto.' },
  despachado: { destino: 'despachado', label: 'Despachar', icon: 'local_shipping', actor: 'Depósito', condicion: 'Descuenta el stock del depósito y crea el envío con la agencia elegida.' },
  entregado: { destino: 'entregado', label: 'Confirmar entrega', icon: 'task_alt', actor: 'Agencia', condicion: 'Fija la comisión del vendedor. También se dispara al entregar el envío.' },
};
const SIGUIENTE: Record<string, string | null> = { borrador: 'confirmado', confirmado: 'preparado', preparado: 'despachado', despachado: 'entregado', entregado: null, anulado: null };
const ANULABLES = ['borrador', 'confirmado', 'preparado', 'despachado'];
const EDITABLES = ['borrador', 'confirmado'];
const RECORRIDO = ['borrador', 'confirmado', 'preparado', 'despachado', 'entregado'];

/**
 * PEDIDO — escena artesanal (revisión 2026-09-12, ítem 1). Pisa la ficha generada de `/pedido/:id`:
 * cabecera con cliente 360, vendedor y envío enlazados; recorrido del ciclo con el próximo paso
 * guiado (mismas transiciones que `ciclos-vida.json`); líneas con foto, SKU → ficha, talle,
 * color, cantidad, precio (con aviso si está por debajo de lista), subtotal, stock del depósito
 * y pie de totales; WhatsApp al cliente con el detalle y el link público del envío. Las
 * escrituras siguen siendo los commands generados (transiciones, líneas).
 */
@Component({
  selector: 'app-pedido-escena',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterLink,
    MatIconModule, MatButtonModule, MatTooltipModule, MatProgressSpinnerModule, MatSnackBarModule,
    MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MembreteImpresionComponent, EtiquetaPipe, UsdPipe,
  ],
  templateUrl: './pedido.component.html',
  styleUrl: './pedido.component.scss',
})
export class PedidoEscenaComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly escena = inject(PedidoEscenaService);
  private readonly pedidoApi = inject(PedidoApiService);
  private readonly pedidos = inject(PedidoService);
  private readonly lineasSvc = inject(PedidoLineaService);
  private readonly agencias = inject(AgenciaService);
  private readonly catalogo = inject(CatalogoService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  readonly cargando = signal(true);
  readonly pedido = signal<FichaPedido | null>(null);
  readonly fotos = signal<Record<number, string>>({});
  readonly listaAgencias = signal<Agencia[]>([]);
  readonly ejecutando = signal(false);
  readonly anulando = signal(false);
  readonly eligiendoAgencia = signal(false);
  motivoAnulacion = '';
  agenciaElegida: number | null = null;
  id = 0;

  readonly recorrido = RECORRIDO;
  readonly estado = computed(() => (this.pedido()?.estado ?? '').toLowerCase());
  readonly siguiente = computed<Paso | null>(() => { const s = SIGUIENTE[this.estado()]; return s ? PASOS[s] : null; });
  readonly puedeAnular = computed(() => ANULABLES.includes(this.estado()));
  readonly editable = computed(() => EDITABLES.includes(this.estado()));
  readonly indiceEstado = computed(() => RECORRIDO.indexOf(this.estado()));
  readonly comisionEstimada = computed(() => {
    const p = this.pedido();
    if (!p) return 0;
    return p.comisionUsd > 0 ? p.comisionUsd : Math.round(p.totalUsd * p.comisionPorcentaje) / 100;
  });
  readonly lineasConDescuento = computed(() => (this.pedido()?.lineas ?? []).filter(l => l.precioListaUsd > 0 && l.precioUnitarioUsd < l.precioListaUsd).length);
  readonly linkSeguimiento = computed(() => {
    const c = this.pedido()?.envioCodigo;
    return c ? `${window.location.origin}/seguimiento/${encodeURIComponent(c)}` : null;
  });

  ngOnInit(): void {
    this.route.paramMap.subscribe(pm => { this.id = Number(pm.get('id') ?? 0); this.cargar(); });
  }

  cargar(): void {
    if (!this.id) return;
    this.cargando.set(true);
    this.escena.ficha(this.id).subscribe({
      next: p => {
        this.pedido.set(p);
        this.agenciaElegida = p.agenciaId;
        this.cargando.set(false);
        this.cargarFotos(p.lineas);
      },
      error: (e: unknown) => { console.error('[Pedido] ficha:', e); this.pedido.set(null); this.cargando.set(false); },
    });
  }

  private cargarFotos(lineas: FichaPedidoLinea[]): void {
    const ids = [...new Set(lineas.map(l => l.imagenPrincipalId).filter((x): x is number => x != null))];
    if (ids.length === 0) { this.fotos.set({}); return; }
    forkJoin(ids.map(id => this.catalogo.portada(id).pipe(catchError(() => of(null))))).subscribe(srcs => {
      const mapa: Record<number, string> = {};
      ids.forEach((id, i) => { if (srcs[i]) mapa[id] = srcs[i]!; });
      this.fotos.set(mapa);
    });
  }

  foto(l: FichaPedidoLinea): string | null { return l.imagenPrincipalId != null ? this.fotos()[l.imagenPrincipalId] ?? null : null; }

  faltante(l: FichaPedidoLinea): boolean { return l.disponibleDeposito != null && l.cantidad > l.disponibleDeposito; }

  // ─── Ciclo ───────────────────────────────────────────────────────────────
  async avanzar(): Promise<void> {
    const paso = this.siguiente(); const p = this.pedido();
    if (!paso || !p || this.ejecutando()) return;
    if (paso.destino === 'confirmado' && p.lineas.length === 0) { this.aviso('El pedido no tiene líneas: agregá al menos una antes de confirmar.'); return; }
    if (paso.destino === 'despachado' && !p.agenciaId) { this.abrirAgencias(); return; }
    const ok = await (window as unknown as { confirmar: (m: string) => Promise<boolean> }).confirmar(`¿${paso.label} el pedido ${p.numero}?\n${paso.condicion}`);
    if (!ok) return;
    this.ejecutar(paso.destino);
  }

  private ejecutar(destino: string): void {
    this.ejecutando.set(true);
    const llamada = destino === 'confirmado' ? this.pedidoApi.pasarAConfirmado(this.id)
      : destino === 'preparado' ? this.pedidoApi.pasarAPreparado(this.id)
      : destino === 'despachado' ? this.pedidoApi.pasarADespachado(this.id)
      : destino === 'entregado' ? this.pedidoApi.pasarAEntregado(this.id)
      : this.pedidoApi.pasarAAnulado(this.id);
    llamada.subscribe({
      next: () => { this.ejecutando.set(false); this.anulando.set(false); this.aviso(`Pedido ${etiqueta(destino).toLowerCase()}.`, true); this.cargar(); },
      error: (e: { error?: { message?: string; detail?: string } }) => {
        this.ejecutando.set(false);
        this.aviso(e?.error?.detail || e?.error?.message || 'No se pudo cambiar el estado del pedido.');
      },
    });
  }

  abrirAgencias(): void {
    this.eligiendoAgencia.set(true);
    if (this.listaAgencias().length === 0) {
      this.agencias.getAll().subscribe({ next: a => this.listaAgencias.set(a ?? []), error: () => this.listaAgencias.set([]) });
    }
  }

  guardarAgenciaYDespachar(): void {
    const p = this.pedido();
    if (!p || !this.agenciaElegida) { this.aviso('Elegí la agencia con la que sale el pedido.'); return; }
    this.ejecutando.set(true);
    this.pedidos.getById(this.id).subscribe({
      next: (actual) => {
        if (!actual) { this.ejecutando.set(false); return; }
        this.pedidos.update(this.id, { ...actual, agenciaId: this.agenciaElegida }).subscribe({
          next: () => { this.eligiendoAgencia.set(false); this.ejecutar('despachado'); },
          error: () => { this.ejecutando.set(false); this.aviso('No se pudo guardar la agencia.'); },
        });
      },
      error: () => { this.ejecutando.set(false); this.aviso('No se pudo leer el pedido.'); },
    });
  }

  pedirAnulacion(): void { this.anulando.set(true); this.motivoAnulacion = ''; }

  confirmarAnulacion(): void {
    const motivo = this.motivoAnulacion.trim();
    if (!motivo) { this.aviso('Indicá el motivo de la anulación.'); return; }
    this.ejecutando.set(true);
    this.pedidos.getById(this.id).subscribe({
      next: (actual) => {
        if (!actual) { this.ejecutando.set(false); return; }
        this.pedidos.update(this.id, { ...actual, motivoAnulacion: motivo }).subscribe({
          next: () => this.ejecutar('anulado'),
          error: () => { this.ejecutando.set(false); this.aviso('No se pudo guardar el motivo.'); },
        });
      },
      error: () => { this.ejecutando.set(false); this.aviso('No se pudo leer el pedido.'); },
    });
  }

  // ─── Líneas (solo en borrador/confirmado; el hook las congela después) ─────
  agregarLinea(): void {
    const ref = this.dialog.open(PedidoLineaFormComponent, { width: '600px', maxWidth: '95vw', data: { contextoFk: { campo: 'pedidoId', valor: this.id } } });
    ref.afterClosed().subscribe(r => { if (r) { this.aviso('Línea agregada.', true); this.cargar(); } });
  }

  async quitarLinea(l: FichaPedidoLinea): Promise<void> {
    const ok = await (window as unknown as { confirmar: (m: string) => Promise<boolean> }).confirmar(`¿Quitar ${l.productoNombre} ${l.talla ?? ''} ${l.color ?? ''} del pedido?`);
    if (!ok) return;
    this.lineasSvc.delete(l.id).subscribe({
      next: () => { this.aviso('Línea quitada.', true); this.cargar(); },
      error: () => this.aviso('No se pudo quitar la línea.'),
    });
  }

  // ─── Compartir ───────────────────────────────────────────────────────────
  whatsapp(): void {
    const p = this.pedido();
    if (!p) return;
    const detalle = p.lineas.map(l => {
      const v = [l.talla, l.color].filter(Boolean).join(' / ');
      return `• ${l.productoNombre}${v ? ' (' + v + ')' : ''} x${l.cantidad} — ${usd(l.subtotalUsd)}`;
    }).join('\n');
    const lineas = [
      `Pedido ${p.numero} · ${etiqueta(p.estado)}`,
      p.clienteNombre ? `Cliente: ${p.clienteNombre}` : '',
      '',
      detalle,
      '',
      `Total: ${usd(p.totalUsd)}`,
      this.linkSeguimiento() ? `\nSeguimiento del envío: ${this.linkSeguimiento()}` : '',
    ].filter(x => x !== '').join('\n');
    const tel = (p.clienteTelefono || '').replace(/[^0-9]/g, '');
    window.open(`https://wa.me/${tel}?text=${encodeURIComponent(lineas)}`, '_blank');
  }

  imprimir(): void { window.print(); }
  volver(): void { this.router.navigate(['/pedido']); }
  nuevoParaCliente(): void { const p = this.pedido(); if (p) this.router.navigate(['/pedidos/nuevo'], { queryParams: { clienteId: p.clienteId } }); }

  private aviso(texto: string, ok = false): void {
    this.snack.open(texto, 'Cerrar', { duration: 3500, panelClass: ok ? 'snackbar-success' : 'snackbar-error' });
  }
}
