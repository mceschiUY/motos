import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { forkJoin, of } from 'rxjs';

import { ClienteService } from '../../generated/services/cliente.service';
import { VendedorService } from '../../generated/services/vendedor.service';
import { DepositoService } from '../../generated/services/deposito.service';
import { AgenciaService } from '../../generated/services/agencia.service';
import { VarianteService } from '../../generated/services/variante.service';
import { PedidoService } from '../../generated/services/pedido.service';
import { PedidoLineaService } from '../../generated/services/pedidolinea.service';
import { Cliente } from '../../generated/models/cliente.model';
import { Vendedor } from '../../generated/models/vendedor.model';
import { Deposito } from '../../generated/models/deposito.model';
import { Agencia } from '../../generated/models/agencia.model';
import { Variante } from '../../generated/models/variante.model';
import { ExistenciasService } from '../existencias/existencias.service';
import { Existencia } from '../existencias/existencia.model';

/** Una línea del carrito, antes de existir en la base. */
interface LineaArmado {
  varianteId: number;
  sku: string;
  producto: string;
  detalle: string;
  cantidad: number;
  precioUnitarioUsd: number;
  disponible: number;
}

/**
 * Armado de pedido — pantalla artesanal (plan §4.3). Es la pantalla "que vende": el
 * vendedor busca el SKU, ve cuánto hay en el depósito elegido y el precio en dólares,
 * y arma el pedido con el total actualizándose en vivo.
 *
 * El pedido se crea recién al confirmar: primero la cabecera (POST /Pedido) y después
 * una línea por ítem. Queda en BORRADOR — confirmar/despachar es decisión aparte, con
 * sus reglas del backend.
 */
@Component({
  selector: 'app-armado-pedido',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatIconModule, MatButtonModule, MatTooltipModule, MatFormFieldModule, MatSelectModule,
    MatInputModule, MatProgressSpinnerModule, MatSnackBarModule,
  ],
  templateUrl: './armado-pedido.component.html',
  styleUrl: './armado-pedido.component.scss',
})
export class ArmadoPedidoComponent implements OnInit {
  private readonly clienteService = inject(ClienteService);
  private readonly vendedorService = inject(VendedorService);
  private readonly depositoService = inject(DepositoService);
  private readonly agenciaService = inject(AgenciaService);
  private readonly varianteService = inject(VarianteService);
  private readonly existenciasService = inject(ExistenciasService);
  private readonly pedidoService = inject(PedidoService);
  private readonly lineaService = inject(PedidoLineaService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  readonly clientes = signal<Cliente[]>([]);
  readonly vendedores = signal<Vendedor[]>([]);
  readonly depositos = signal<Deposito[]>([]);
  readonly agencias = signal<Agencia[]>([]);
  readonly variantes = signal<Variante[]>([]);
  readonly existencias = signal<Existencia[]>([]);

  // Cabecera
  readonly clienteId = signal<number | null>(null);
  readonly vendedorId = signal<number | null>(null);
  readonly depositoId = signal<number | null>(null);
  readonly agenciaId = signal<number | null>(null);
  readonly observaciones = signal<string>('');

  readonly busqueda = signal<string>('');
  readonly lineas = signal<LineaArmado[]>([]);
  readonly guardando = signal(false);
  readonly cargando = signal(true);

  /** Saldo por variante en el depósito elegido (0 si no hay fila: el Kardex no la trae). */
  private readonly saldoPorVariante = computed(() => {
    const mapa = new Map<number, number>();
    for (const e of this.existencias()) { mapa.set(e.varianteId, e.disponible); }
    return mapa;
  });

  /** Resultados del buscador: hasta 20, para que la lista sea usable en el celular. */
  readonly resultados = computed(() => {
    const t = this.busqueda().trim().toLowerCase();
    if (t.length < 2) { return []; }
    const saldos = this.saldoPorVariante();
    return this.variantes()
      .filter(v => v.activo !== false)
      .filter(v => [v.sku, v.productoDisplay, v.tallaDisplay, v.colorDisplay]
        .some(c => (c ?? '').toLowerCase().includes(t)))
      .slice(0, 20)
      .map(v => ({ variante: v, disponible: saldos.get(v.id) ?? 0 }));
  });

  readonly total = computed(() => this.lineas().reduce((s, l) => s + l.cantidad * l.precioUnitarioUsd, 0));
  readonly unidades = computed(() => this.lineas().reduce((s, l) => s + l.cantidad, 0));
  readonly hayFaltantes = computed(() => this.lineas().some(l => l.cantidad > l.disponible));

  readonly puedeGuardar = computed(() =>
    this.clienteId() != null && this.vendedorId() != null && this.depositoId() != null &&
    this.lineas().length > 0 && !this.guardando());

  ngOnInit(): void {
    this.cargando.set(true);
    this.clienteService.getAll().subscribe({
      next: (d: Cliente[]) => this.clientes.set(d),
      error: (e: unknown) => console.error('[ArmadoPedido] clientes:', e),
    });
    this.vendedorService.getAll().subscribe({
      next: (d: Vendedor[]) => this.vendedores.set(d.filter(v => v.activo !== false)),
      error: (e: unknown) => console.error('[ArmadoPedido] vendedores:', e),
    });
    this.depositoService.getAll().subscribe({
      next: (d: Deposito[]) => {
        this.depositos.set(d);
        // Con un solo depósito no tiene sentido preguntarlo: se elige solo.
        if (d.length > 0 && this.depositoId() == null) { this.depositoId.set(d[0].id); this.cargarExistencias(); }
      },
      error: (e: unknown) => console.error('[ArmadoPedido] depósitos:', e),
    });
    this.agenciaService.getAll().subscribe({
      next: (d: Agencia[]) => this.agencias.set(d),
      error: (e: unknown) => console.error('[ArmadoPedido] agencias:', e),
    });
    this.varianteService.getAll().subscribe({
      next: (d: Variante[]) => { this.variantes.set(d); this.cargando.set(false); },
      error: (e: unknown) => { console.error('[ArmadoPedido] variantes:', e); this.cargando.set(false); },
    });
    // El vendedor logueado arranca elegido (es el que va a estar armando el pedido).
    this.vendedorService.mio().subscribe({
      next: (v: Vendedor | null) => { if (v) { this.vendedorId.set(v.id); } },
      error: () => { /* el usuario no es vendedor: se elige a mano */ },
    });
  }

  /** Al elegir cliente, se propone su vendedor asignado. */
  onClienteChange(id: number | null): void {
    this.clienteId.set(id);
    const c = this.clientes().find(x => x.id === id) as any;
    if (c?.vendedorId) { this.vendedorId.set(c.vendedorId); }
  }

  onDepositoChange(id: number | null): void {
    this.depositoId.set(id);
    this.cargarExistencias();
  }

  private cargarExistencias(): void {
    const dep = this.depositoId();
    if (dep == null) { this.existencias.set([]); return; }
    this.existenciasService.existencias(null, dep).subscribe({
      next: (d: Existencia[]) => { this.existencias.set(d ?? []); this.refrescarDisponibles(); },
      error: (e: unknown) => { console.error('[ArmadoPedido] existencias:', e); this.existencias.set([]); },
    });
  }

  private refrescarDisponibles(): void {
    const saldos = this.saldoPorVariante();
    this.lineas.update(ls => ls.map(l => ({ ...l, disponible: saldos.get(l.varianteId) ?? 0 })));
  }

  agregar(variante: Variante, disponible: number): void {
    const existente = this.lineas().find(l => l.varianteId === variante.id);
    if (existente) {
      this.cambiarCantidad(existente, existente.cantidad + 1);
    } else {
      this.lineas.update(ls => [...ls, {
        varianteId: variante.id,
        sku: variante.sku,
        producto: variante.productoDisplay ?? '',
        detalle: [variante.tallaDisplay, variante.colorDisplay].filter(Boolean).join(' / '),
        cantidad: 1,
        precioUnitarioUsd: variante.precioLista ?? 0,
        disponible,
      }]);
    }
    this.busqueda.set('');
  }

  cambiarCantidad(linea: LineaArmado, cantidad: number): void {
    const n = Number(cantidad);
    if (!isFinite(n) || n <= 0) { return; }
    this.lineas.update(ls => ls.map(l => l.varianteId === linea.varianteId ? { ...l, cantidad: n } : l));
  }

  cambiarPrecio(linea: LineaArmado, precio: number): void {
    const n = Number(precio);
    if (!isFinite(n) || n < 0) { return; }
    this.lineas.update(ls => ls.map(l => l.varianteId === linea.varianteId ? { ...l, precioUnitarioUsd: n } : l));
  }

  quitar(linea: LineaArmado): void {
    this.lineas.update(ls => ls.filter(l => l.varianteId !== linea.varianteId));
  }

  usd(n: number): string {
    return 'US$ ' + new Intl.NumberFormat('es-UY', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(n ?? 0);
  }

  nombreCliente(): string {
    return this.clientes().find(c => c.id === this.clienteId())?.nombre ?? '';
  }

  /**
   * Crea la cabecera y después las líneas. Si alguna línea falla, el pedido igual queda
   * creado (en borrador): se avisa y se abre la ficha para completarlo a mano, que es
   * menos malo que perder lo cargado.
   */
  guardar(): void {
    if (!this.puedeGuardar()) { return; }
    this.guardando.set(true);
    const cabecera = {
      clienteId: this.clienteId(),
      vendedorId: this.vendedorId(),
      depositoId: this.depositoId(),
      agenciaId: this.agenciaId(),
      fecha: new Date(),
      observaciones: this.observaciones() || null,
    } as any;

    this.pedidoService.create(cabecera).subscribe({
      next: (creado: any) => {
        const pedidoId = creado?.id ?? creado;
        const altas = this.lineas().map(l => this.lineaService.create({
          pedidoId,
          varianteId: l.varianteId,
          cantidad: l.cantidad,
          precioUnitarioUsd: l.precioUnitarioUsd,
        } as any));
        forkJoin(altas.length ? altas : [of(null)]).subscribe({
          next: () => {
            this.guardando.set(false);
            this.snackBar.open('Pedido creado en borrador', 'OK', { duration: 3000 });
            this.router.navigate(['/pedido', pedidoId]);
          },
          error: (err: any) => {
            this.guardando.set(false);
            const detalle = err?.error?.detail || 'Algunas líneas no se pudieron guardar';
            this.snackBar.open(detalle, 'OK', { duration: 5000 });
            this.router.navigate(['/pedido', pedidoId]);
          },
        });
      },
      error: (err: any) => {
        this.guardando.set(false);
        const detalle = err?.error?.detail || 'No se pudo crear el pedido';
        this.snackBar.open(detalle, 'OK', { duration: 5000 });
      },
    });
  }

  cancelar(): void { this.router.navigate(['/pedido']); }
}
