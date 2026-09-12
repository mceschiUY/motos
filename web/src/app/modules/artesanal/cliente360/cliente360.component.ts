import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { etiqueta } from '../comun/etiquetas';
import { EtiquetaPipe } from '../comun/etiquetas';
import { UsdPipe } from '../comun/usd.pipe';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MiniMapaComponent } from '../../../shared/components/mini-mapa/mini-mapa.component';
import { ClienteService } from '../../generated/services/cliente.service';
import { Cliente } from '../../generated/models/cliente.model';
import { ActividadFormComponent } from '../../generated/components/actividad/actividad-form/actividad-form.component';
import { ClienteFormComponent } from '../../generated/components/cliente/cliente-form/cliente-form.component';
import { Cliente360Service } from './cliente360.service';
import { Cliente360, Cliente360TimelineItem } from './cliente360.model';

/**
 * Cliente 360 — escena artesanal (plan: escenas). Pisa la ficha generada de Cliente
 * (`/cliente/:id`, ruta declarada antes de GENERATED_ROUTES) y responde "¿quién es este
 * cliente, cómo está la relación y qué hago ahora?": el vendedor la abre antes de entrar
 * a la tienda. Solo LEE (`/api/artesanal/cliente/{id}/360`); las escrituras van por los
 * forms generados (Actividad, Cliente) abiertos en diálogo y por el armado de pedido.
 */
@Component({
  selector: 'app-cliente360',
  standalone: true,
  imports: [UsdPipe, EtiquetaPipe, 
    CommonModule, RouterLink,
    MatIconModule, MatButtonModule, MatTooltipModule, MatProgressSpinnerModule,
    MatSnackBarModule, MatDialogModule,
    MiniMapaComponent,
  ],
  templateUrl: './cliente360.component.html',
  styleUrl: './cliente360.component.scss',
})
export class Cliente360Component implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly service = inject(Cliente360Service);
  private readonly clienteService = inject(ClienteService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly isLoading = signal(true);
  readonly cliente = signal<Cliente360 | null>(null);
  clienteId = 0;

  /** Dígitos del teléfono para `wa.me` (sin espacios, guiones ni "+"); null si no hay teléfono usable. */
  readonly whatsappUrl = computed(() => this.waUrl(this.cliente()?.telefono));
  readonly whatsappVendedorUrl = computed(() => this.waUrl(this.cliente()?.vendedorTelefono));

  readonly tieneUbicacion = computed(() => {
    const c = this.cliente();
    return !!c && c.latitud != null && c.longitud != null;
  });

  readonly mapsUrl = computed(() => {
    const c = this.cliente();
    if (!c || c.latitud == null || c.longitud == null) { return null; }
    return `https://www.google.com/maps/search/?api=1&query=${c.latitud},${c.longitud}`;
  });

  /** Etiqueta de la próxima acción relativa a hoy ("Hoy", "Mañana", "En 5 días"). */
  readonly proximaAccionLabel = computed(() => {
    const iso = this.cliente()?.proximaAccion;
    if (!iso) { return null; }
    const [y, m, d] = iso.substring(0, 10).split('-').map(Number);
    const f = new Date(y, m - 1, d);
    const hoy = new Date(); hoy.setHours(0, 0, 0, 0);
    const diff = Math.round((f.getTime() - hoy.getTime()) / 86400000);
    if (diff === 0) { return 'Hoy'; }
    if (diff === 1) { return 'Mañana'; }
    return `En ${diff} días`;
  });

  ngOnInit(): void {
    this.route.paramMap.subscribe(pm => {
      this.clienteId = Number(pm.get('id') ?? 0);
      this.cargar();
    });
  }

  cargar(): void {
    if (!this.clienteId) { this.cliente.set(null); this.isLoading.set(false); return; }
    this.isLoading.set(true);
    this.service.cliente360(this.clienteId).subscribe({
      next: (c: Cliente360) => { this.cliente.set(c); this.isLoading.set(false); },
      error: (e: unknown) => {
        console.error('[Cliente360] cargar:', e);
        this.cliente.set(null);
        this.isLoading.set(false);
      },
    });
  }

  // ─── Acciones ───

  volver(): void { this.router.navigate(['/cliente']); }

  /** Form generado de Actividad con el cliente fijado por contexto (mismo mecanismo que la agenda). */
  registrarVisita(): void {
    const ref = this.dialog.open(ActividadFormComponent, {
      width: '600px', maxWidth: '95vw', panelClass: 'crm-dialog', autoFocus: true,
      data: { item: null, mode: 'create', contextoFk: { campo: 'clienteId', valor: this.clienteId } },
    });
    ref.afterClosed().subscribe(res => {
      if (res) { this.mensaje('Actividad registrada', 'success'); this.cargar(); }
    });
  }

  nuevoPedido(): void {
    this.router.navigate(['/pedidos/nuevo'], { queryParams: { clienteId: this.clienteId } });
  }

  /** Form generado de Cliente con el item completo (la ficha 360 no es el modelo del form). */
  editarDatos(): void {
    this.clienteService.getById(this.clienteId).subscribe({
      next: (item: Cliente | undefined) => {
        if (!item) { this.mensaje('No se pudo leer el cliente', 'error'); return; }
        const ref = this.dialog.open(ClienteFormComponent, {
          width: '640px', maxWidth: '95vw', panelClass: 'crm-dialog', autoFocus: true,
          data: { item, mode: 'edit' },
        });
        ref.afterClosed().subscribe(res => {
          if (res) { this.mensaje('Datos guardados', 'success'); this.cargar(); }
        });
      },
      error: () => this.mensaje('No se pudo leer el cliente', 'error'),
    });
  }

  irA(item: Cliente360TimelineItem): void { this.router.navigateByUrl(item.ruta); }

  // ─── Presentación ───

  iconoTimeline(tipo: Cliente360TimelineItem['tipo']): string {
    return ({ actividad: 'event_note', pedido: 'receipt_long', envio: 'local_shipping' } as Record<string, string>)[tipo] ?? 'circle';
  }

  iconoSemaforo(s: string): string {
    return ({ verde: 'check_circle', amarillo: 'error', rojo: 'warning' } as Record<string, string>)[s] ?? 'help';
  }

  etiquetaTipoCliente(t: string | null): string {
    return t ? etiqueta(t) : 'Sin tipo';
  }

  etiquetaTipoActividad(t: string | null): string {
    return etiqueta(t);
  }

  /** Estados de pedido y envío y resultados de actividad, del diccionario compartido. */
  etiquetaEstado(e: string | null): string {
    return etiqueta(e);
  }

  private waUrl(telefono: string | null | undefined): string | null {
    const digitos = (telefono ?? '').replace(/\D/g, '');
    return digitos.length >= 6 ? `https://wa.me/${digitos}` : null;
  }

  private mensaje(texto: string, tipo: 'success' | 'error'): void {
    this.snackBar.open(texto, 'Cerrar', { duration: 3000, panelClass: tipo === 'success' ? 'snackbar-success' : 'snackbar-error' });
  }
}
