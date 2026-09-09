import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { environment } from '../../../../../../environments/environment';
import { AsistenteFormBridgeService } from '../../../../../core/services/asistente-form-bridge.service';

import { Pedido } from '../../../models/pedido.model';
import { PedidoService } from '../../../services/pedido.service';
import { PedidoLinea } from '../../../models/pedidolinea.model';
import { PedidoLineaService } from '../../../services/pedidolinea.service';
import { Cliente } from '../../../models/cliente.model';
import { ClienteService } from '../../../services/cliente.service';
import { PedidoFormComponent } from '../pedido-form/pedido-form.component';
import { FotoGaleriaComponent } from '../../../../../shared/components/foto-galeria/foto-galeria.component';
import { MembreteImpresionComponent } from '../../../../../shared/components/membrete-impresion/membrete-impresion.component';

interface EventoHistoria { fecha: string; quien: string; que: string; ok: boolean; }
interface RelacionFicha { nombre: string; label: string; icon: string; ruta: string; endpoint: string; }

@Component({
  selector: 'app-pedido-ficha',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatTooltipModule, MatDialogModule, MatSnackBarModule, FotoGaleriaComponent, MembreteImpresionComponent],
  templateUrl: './pedido-ficha.component.html',
  styleUrl: './pedido-ficha.component.scss'
})
export class PedidoFichaComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly http = inject(HttpClient);
  private readonly service = inject(PedidoService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly puenteVoz = inject(AsistenteFormBridgeService);
  private readonly lineaService = inject(PedidoLineaService);
  private readonly clienteService = inject(ClienteService);

  readonly item = signal<Pedido | null>(null);
  readonly cargando = signal<boolean>(true);
  readonly historia = signal<EventoHistoria[]>([]);
  readonly relacionados = signal<Record<string, any[]>>({});
  readonly tabActiva = signal<string>('');
  id: string | number = '';

  readonly relaciones: RelacionFicha[] = [
    { nombre: 'pedidolinea', label: 'Líneas', icon: 'list', ruta: '/pedidolinea', endpoint: '/PedidoLinea/by-pedido/' }
  ];

  // ═══ Proceso guiado: el ciclo de vida como camino ═══
  readonly pasos = [
    { id: 'borrador', label: 'Borrador', clase: 'estado-0' },
    { id: 'confirmado', label: 'Confirmado', clase: 'estado-1' },
    { id: 'preparado', label: 'Preparado', clase: 'estado-2' },
    { id: 'despachado', label: 'Despachado', clase: 'estado-3' },
    { id: 'entregado', label: 'Entregado', clase: 'estado-4' },
    { id: 'anulado', label: 'Anulado', clase: 'estado-5' }
  ];
  private readonly permitidos: Record<string, string[]> = {
    'confirmado': ['borrador'],
    'preparado': ['confirmado'],
    'despachado': ['preparado'],
    'entregado': ['despachado'],
    'anulado': ['borrador', 'confirmado', 'preparado', 'despachado']
  };
  readonly guia: Record<string, { condicion: string; actor: string }> = {
    'confirmado': { condicion: 'Al menos una línea. Si falta stock avisa, pero deja confirmar', actor: 'Vendedor' },
    'preparado': { condicion: 'El depósito armó el bulto', actor: 'Depósito' },
    'despachado': { condicion: 'Descuenta el stock del depósito y crea el envío con la agencia elegida', actor: 'Depósito' },
    'entregado': { condicion: 'Fija la comisión del vendedor. Se dispara solo cuando se entrega el envío', actor: 'Agencia' },
    'anulado': { condicion: 'Si ya había salido del depósito, devuelve la mercadería al stock', actor: 'Administración' }
  };

  private normEstado(v: unknown): string {
    return (v ?? '').toString().trim().toLowerCase().replace(/ /g, '_');
  }

  estadoActual(): string { return this.normEstado(((this.item() as any) || {}).estado); }

  indicePaso(): number {
    return this.pasos.findIndex(p => p.id === this.estadoActual());
  }

  claseEstado(): string {
    const i = this.indicePaso();
    return i >= 0 ? this.pasos[i].clase : 'estado-off';
  }

  // Solo los movimientos que el negocio permite desde el estado actual
  pasosPosibles(): { id: string; label: string }[] {
    const actual = this.estadoActual();
    return this.pasos.filter(p => (this.permitidos[p.id] || []).includes(actual));
  }

  ejecutarPaso(destino: string): void {
    const accion = this.accionDe(destino);
    if (!accion) { return; }
    accion.subscribe({
      next: () => {
        this.snackBar.open('→ ' + destino.replace(/_/g, ' '), 'OK', { duration: 2500 });
        this.cargar();
      },
      error: (err: any) => {
        const msg = err?.error?.[0]?.message || err?.error?.message || 'El sistema impidió este paso';
        this.snackBar.open(msg, 'OK', { duration: 4500 });
      }
    });
  }

  private accionDe(destino: string): import('rxjs').Observable<unknown> | null {
    switch (destino) {
      case 'confirmado': return this.service.pasarAConfirmado(this.id);
      case 'preparado': return this.service.pasarAPreparado(this.id);
      case 'despachado': return this.service.pasarADespachado(this.id);
      case 'entregado': return this.service.pasarAEntregado(this.id);
      case 'anulado': return this.service.pasarAAnulado(this.id);
      default: return null;
    }
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe(pm => {
      this.id = pm.get('id') ?? '';
      if (this.relaciones.length > 0) { this.tabActiva.set(this.relaciones[0].nombre); }
      this.cargar();
    });
    // Puente del asistente de voz: "editá este registro" abre el form de ESTA ficha
    this.puenteVoz.registrarEditor('pedido', () => this.editar());
  }

  ngOnDestroy(): void {
    this.puenteVoz.quitarEditor('pedido');
  }

  cargar(): void {
    this.cargando.set(true);
    this.service.getById(String(this.id)).subscribe({
      next: (data) => { this.item.set(data ?? null); this.cargando.set(false); },
      error: () => { this.item.set(null); this.cargando.set(false); }
    });
    this.cargarHistoria();
    for (const r of this.relaciones) { this.cargarRelacion(r); }
  }

  titulo(): string {
    const i = this.item() as any;
    return i ? (i.numero || ('Pedido #' + i.id)) : '';
  }

  volver(): void { this.router.navigate(['/pedido']); }

  editar(): void {
    const ref = this.dialog.open(PedidoFormComponent, {
      width: '600px', maxWidth: '95vw', panelClass: 'crm-dialog',
      data: { item: this.item(), mode: 'edit' }
    });
    ref.afterClosed().subscribe((ok: unknown) => { if (ok) { this.cargar(); } });
  }

  async eliminar(): Promise<void> {
    if (!(await (window as any).confirmar('¿Eliminar este registro?\n\nEsta acción no se puede deshacer.'))) { return; }
    this.service.delete(String(this.id)).subscribe({ next: () => this.volver() });
  }

  documentos(): void {
    this.router.navigate(['/documento'], { queryParams: { relacionId: this.id, relacionNombre: 'Pedido' } });
  }

  // PDF con marca: el navegador imprime; el membrete y el @media print hacen el resto
  imprimir(): void { window.print(); }

  /**
   * Compartir por WhatsApp (plan §4.8): arma el texto del pedido y abre wa.me con el
   * teléfono del cliente. Sin integración ni API — es un enlace, que es el flujo real
   * del rubro. Si el cliente no tiene teléfono, wa.me abre igual para elegir contacto.
   */
  compartirPorWhatsApp(): void {
    const p = this.item() as any;
    if (!p) { return; }
    this.lineaService.getByPedidoId(p.id).subscribe({
      next: (lineas: PedidoLinea[]) => {
        this.clienteService.getById(String(p.clienteId)).subscribe({
          next: (c: Cliente | undefined) => this.abrirWhatsApp(p, lineas, c),
          error: () => this.abrirWhatsApp(p, lineas, undefined)
        });
      },
      error: () => this.snackBar.open('No se pudieron leer las líneas del pedido', 'OK', { duration: 3000 })
    });
  }

  private abrirWhatsApp(pedido: any, lineas: PedidoLinea[], cliente?: Cliente): void {
    const detalle = lineas.map(l => {
      const variante = [l.tallaDisplay, l.colorDisplay].filter(Boolean).join(' / ');
      const nombre = l.productoDisplay || l.varianteDisplay || 'SKU';
      return `• ${nombre}${variante ? ' (' + variante + ')' : ''} x${l.cantidad} — US$ ${l.subtotalUsd}`;
    }).join('\n');
    const texto = [
      `Pedido ${pedido.numero}`,
      cliente?.nombre ? `Cliente: ${cliente.nombre}` : '',
      '',
      detalle,
      '',
      `Total: US$ ${pedido.totalUsd}`
    ].filter(l => l !== '').join('\n');
    const telefono = ((cliente as any)?.telefono || '').replace(/[^0-9]/g, '');
    window.open(`https://wa.me/${telefono}?text=${encodeURIComponent(texto)}`, '_blank');
  }

  // ═══ Historia: la auditoría que el sistema YA registra, por fin visible ═══
  cargarHistoria(): void {
    this.http.get<any[]>(`${environment.apiUrl}/Auditoria/entidad/Pedido/${this.id}`).subscribe({
      next: (logs) => this.historia.set((logs || []).map(l => ({
        fecha: l.timestamp, quien: l.userName || 'sistema',
        que: this.traducirAccion(l.action), ok: l.success !== false
      }))),
      error: () => this.historia.set([])
    });
  }

  private traducirAccion(a: string): string {
    const s = a || '';
    const paso = s.match(/PasarA([A-Za-z]+)/);
    if (paso) { return 'Pasó a ' + paso[1].replace(/([A-Z])/g, ' $1').trim().toLowerCase(); }
    if (s.includes('Crear')) { return 'Creado'; }
    if (s.includes('Modificar')) { return 'Modificado'; }
    if (s.includes('Eliminar')) { return 'Eliminado'; }
    return s;
  }

  // ═══ Relacionados ═══
  cargarRelacion(r: RelacionFicha): void {
    this.http.get<any[]>(`${environment.apiUrl}${r.endpoint}${this.id}`).subscribe({
      next: (items) => this.relacionados.update(m => ({ ...m, [r.nombre]: items || [] })),
      error: () => this.relacionados.update(m => ({ ...m, [r.nombre]: [] }))
    });
  }

  itemsDe(nombre: string): any[] { return this.relacionados()[nombre] || []; }

  columnasDe(nombre: string): string[] {
    const items = this.itemsDe(nombre);
    if (items.length === 0) { return []; }
    return Object.keys(items[0]).filter(k => k !== 'id' && !k.toLowerCase().endsWith('id')).slice(0, 4);
  }

  irA(r: RelacionFicha, row: any): void { this.router.navigate([r.ruta, row.id]); }

  verEnLista(r: RelacionFicha): void { this.router.navigate([r.ruta]); }
}
