import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { usd } from '../comun/usd.pipe';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
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
import { Cliente360Service } from '../cliente360/cliente360.service';
import { Cliente360 } from '../cliente360/cliente360.model';
import { PedidoEscenaService } from '../pedido/pedido.service';
import { EtiquetaPipe } from '../comun/etiquetas';
import { UsdPipe } from '../comun/usd.pipe';

/** Una línea del carrito, antes de existir en la base. */
interface LineaArmado {
  varianteId: number;
  sku: string;
  producto: string;
  detalle: string;
  cantidad: number;
  precioUnitarioUsd: number;
  /** Precio de lista y costo de la variante: para ver descuento y margen al tocar el precio. */
  precioListaUsd: number;
  costoUsd: number;
  disponible: number;
}

/**
 * Armado de pedido — pantalla artesanal (plan §4.3). Es la pantalla "que vende": el
 * vendedor busca el SKU, ve cuánto hay en el depósito elegido y el precio en dólares,
 * y arma el pedido con el total actualizándose en vivo.
 *
 * Revisión de escenas 2026-09-12 (ítem 2): cliente con autocompletado; al elegirlo aparece
 * "Este cliente" (semáforo, días sin visita, pedidos abiertos, último pedido con **repetir**,
 * lo que más compra con un clic al buscador); `?pedidoId=N` precarga las líneas de ese pedido;
 * y cada línea muestra el precio de lista, el descuento y el margen cuando se toca el precio.
 *
 * El pedido se crea recién al confirmar: primero la cabecera (POST /Pedido) y después
 * una línea por ítem. Queda en BORRADOR — confirmar/despachar es decisión aparte, con
 * sus reglas del backend.
 */
@Component({
  selector: 'app-armado-pedido',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterLink,
    MatIconModule, MatButtonModule, MatTooltipModule, MatFormFieldModule, MatSelectModule,
    MatInputModule, MatAutocompleteModule, MatProgressSpinnerModule, MatSnackBarModule,
    EtiquetaPipe, UsdPipe,
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
  private readonly cliente360Service = inject(Cliente360Service);
  private readonly pedidoEscena = inject(PedidoEscenaService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
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

  /** Texto del autocompletado de cliente (nombre, ciudad o tipo). */
  readonly clienteTexto = signal<string>('');
  readonly clientesFiltrados = computed(() => {
    const t = this.clienteTexto().trim().toLowerCase();
    const lista = this.clientes();
    const filtrada = t
      ? lista.filter(c => [c.nombre, c.ciudad, c.tipo].some(x => (x ?? '').toLowerCase().includes(t)))
      : lista;
    return filtrada.slice(0, 12);
  });

  /** Contexto del cliente elegido (Cliente 360): lo que necesita el vendedor antes de cargar. */
  readonly contexto = signal<Cliente360 | null>(null);
  readonly contextoCargando = signal(false);
  readonly repitiendo = signal(false);

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
  /** Margen total sobre costo del carrito, en %; null si ningún costo cargado. */
  readonly margenTotal = computed(() => {
    const conCosto = this.lineas().filter(l => l.costoUsd > 0);
    if (conCosto.length === 0) return null;
    const venta = conCosto.reduce((s, l) => s + l.cantidad * l.precioUnitarioUsd, 0);
    const costo = conCosto.reduce((s, l) => s + l.cantidad * l.costoUsd, 0);
    return costo > 0 ? Math.round((venta - costo) / costo * 100) : null;
  });

  readonly puedeGuardar = computed(() =>
    this.clienteId() != null && this.vendedorId() != null && this.depositoId() != null &&
    this.lineas().length > 0 && !this.guardando());

  ngOnInit(): void {
    this.cargando.set(true);
    this.clienteService.getAll().subscribe({
      next: (d: Cliente[]) => { this.clientes.set(d); this.precargarCliente(); },
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
      next: (d: Variante[]) => { this.variantes.set(d); this.cargando.set(false); this.precargarSku(); this.precargarPedido(); this.precargarBusqueda(); },
      error: (e: unknown) => { console.error('[ArmadoPedido] variantes:', e); this.cargando.set(false); },
    });
    // El vendedor logueado arranca elegido (es el que va a estar armando el pedido).
    this.vendedorService.mio().subscribe({
      next: (v: Vendedor | null) => { if (v) { this.vendedorId.set(v.id); } },
      error: () => { /* el usuario no es vendedor: se elige a mano */ },
    });
  }

  /**
   * Etapa C: "Agregar al pedido" de la ficha comercial llega como `?varianteId=N` y deja
   * ese SKU en el carrito. Corre recién con las variantes cargadas, que es de donde sale
   * el precio de lista; el stock lo completa después `refrescarDisponibles()`.
   */
  private precargarSku(): void {
    const id = Number(this.route.snapshot.queryParamMap.get('varianteId'));
    if (!id) { return; }
    const variante = this.variantes().find(v => v.id === id);
    if (!variante) {
      this.snackBar.open('No se encontró el SKU que venía del catálogo', 'Cerrar', { duration: 4000 });
      return;
    }
    this.agregar(variante, this.saldoPorVariante().get(id) ?? 0);
  }

  /** `?buscar=texto` ("Vender" desde la card del catálogo): deja el producto en el buscador. */
  private precargarBusqueda(): void {
    const q = this.route.snapshot.queryParamMap.get('buscar');
    if (q) { this.busqueda.set(q); }
  }

  /** `?pedidoId=N` ("repetir pedido" desde Cliente 360 o desde acá): carga sus líneas. */
  private precargarPedido(): void {
    const id = Number(this.route.snapshot.queryParamMap.get('pedidoId'));
    if (id) { this.repetirPedido(id); }
  }

  /**
   * "Nuevo pedido" de la escena Cliente 360 llega como `?clienteId=N` y deja ese cliente
   * elegido (mismo mecanismo que `?varianteId=`). Corre recién con los clientes cargados
   * y pasa por onClienteChange para que también se proponga su vendedor.
   */
  private precargarCliente(): void {
    const id = Number(this.route.snapshot.queryParamMap.get('clienteId'));
    if (!id || this.clienteId() != null) { return; }
    if (!this.clientes().some(c => c.id === id)) {
      this.snackBar.open('No se encontró el cliente que venía de la ficha', 'Cerrar', { duration: 4000 });
      return;
    }
    this.onClienteChange(id);
  }

  /** Al elegir cliente, se propone su vendedor asignado y se carga su contexto (Cliente 360). */
  onClienteChange(id: number | null): void {
    this.clienteId.set(id);
    const c = this.clientes().find(x => x.id === id);
    this.clienteTexto.set(c?.nombre ?? '');
    if (c?.vendedorId) { this.vendedorId.set(c.vendedorId); }
    this.contexto.set(null);
    if (id == null) { return; }
    this.contextoCargando.set(true);
    this.cliente360Service.cliente360(id).subscribe({
      next: (ctx) => { this.contexto.set(ctx); this.contextoCargando.set(false); },
      error: () => this.contextoCargando.set(false),
    });
  }

  onClienteElegido(ev: MatAutocompleteSelectedEvent): void { this.onClienteChange(Number(ev.option.value)); }

  /** Si el vendedor borra el texto, se suelta el cliente (y su contexto). */
  onClienteTexto(texto: string): void {
    this.clienteTexto.set(texto);
    if (!texto.trim() && this.clienteId() != null) { this.clienteId.set(null); this.contexto.set(null); }
  }

  nombreDeCliente = (id: number | string | null): string =>
    (id == null ? '' : this.clientes().find(c => c.id === Number(id))?.nombre ?? String(id));

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

  agregar(variante: Variante, disponible: number, cantidad = 1, precio?: number): void {
    const existente = this.lineas().find(l => l.varianteId === variante.id);
    if (existente) {
      this.cambiarCantidad(existente, existente.cantidad + cantidad);
    } else {
      this.lineas.update(ls => [...ls, {
        varianteId: variante.id,
        sku: variante.sku,
        producto: variante.productoDisplay ?? '',
        detalle: [variante.tallaDisplay, variante.colorDisplay].filter(Boolean).join(' / '),
        cantidad,
        precioUnitarioUsd: precio ?? variante.precioLista ?? 0,
        precioListaUsd: variante.precioLista ?? 0,
        costoUsd: variante.costoEstandar ?? 0,
        disponible,
      }]);
    }
    this.busqueda.set('');
  }

  /** Carga las líneas de un pedido anterior (repetir): mismas cantidades, precio de lista de hoy. */
  repetirPedido(pedidoId: number): void {
    if (this.repitiendo()) return;
    this.repitiendo.set(true);
    this.pedidoEscena.ficha(pedidoId).subscribe({
      next: (p) => {
        if (this.clienteId() == null && this.clientes().some(c => c.id === p.clienteId)) { this.onClienteChange(p.clienteId); }
        let agregadas = 0;
        for (const l of p.lineas) {
          const v = this.variantes().find(x => x.id === l.varianteId);
          if (!v || v.activo === false) continue;
          this.agregar(v, this.saldoPorVariante().get(v.id) ?? 0, l.cantidad);
          agregadas++;
        }
        this.repitiendo.set(false);
        this.snackBar.open(agregadas > 0
          ? `${agregadas} ${agregadas === 1 ? 'línea' : 'líneas'} del pedido ${p.numero} en el carrito, a precio de lista de hoy`
          : `El pedido ${p.numero} no tiene líneas para repetir`, 'OK', { duration: 4000 });
      },
      error: () => { this.repitiendo.set(false); this.snackBar.open('No se pudo leer el pedido a repetir', 'Cerrar', { duration: 4000 }); },
    });
  }

  /** "Lo que más compra": deja el nombre del producto en el buscador para elegir talle y color. */
  buscarProducto(nombre: string | null): void {
    if (!nombre) return;
    this.busqueda.set(nombre);
    document.querySelector<HTMLInputElement>('.armado .search-input')?.focus();
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

  /** Descuento sobre lista en % (0 si no hay lista o se vende a lista o más). */
  descuento(l: LineaArmado): number {
    if (!l.precioListaUsd || l.precioUnitarioUsd >= l.precioListaUsd) return 0;
    return Math.round((1 - l.precioUnitarioUsd / l.precioListaUsd) * 100);
  }

  /** Margen sobre costo en %; null sin costo cargado. */
  margen(l: LineaArmado): number | null {
    if (!l.costoUsd) return null;
    return Math.round((l.precioUnitarioUsd - l.costoUsd) / l.costoUsd * 100);
  }

  quitar(linea: LineaArmado): void {
    this.lineas.update(ls => ls.filter(l => l.varianteId !== linea.varianteId));
  }

  usd(n: number): string {
    return usd(n);
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
    } as unknown as Parameters<PedidoService['create']>[0];

    this.pedidoService.create(cabecera).subscribe({
      next: (creado: unknown) => {
        const pedidoId = (creado as { id?: number })?.id ?? (creado as number);
        const altas = this.lineas().map(l => this.lineaService.create({
          pedidoId,
          varianteId: l.varianteId,
          cantidad: l.cantidad,
          precioUnitarioUsd: l.precioUnitarioUsd,
        } as unknown as Parameters<PedidoLineaService['create']>[0]));
        forkJoin(altas.length ? altas : [of(null)]).subscribe({
          next: () => {
            this.guardando.set(false);
            this.snackBar.open('Pedido creado en borrador', 'OK', { duration: 3000 });
            this.router.navigate(['/pedido', pedidoId]);
          },
          error: (err: { error?: { detail?: string } }) => {
            this.guardando.set(false);
            const detalle = err?.error?.detail || 'Algunas líneas no se pudieron guardar';
            this.snackBar.open(detalle, 'OK', { duration: 5000 });
            this.router.navigate(['/pedido', pedidoId]);
          },
        });
      },
      error: (err: { error?: { detail?: string } }) => {
        this.guardando.set(false);
        const detalle = err?.error?.detail || 'No se pudo crear el pedido';
        this.snackBar.open(detalle, 'OK', { duration: 5000 });
      },
    });
  }

  cancelar(): void {
    const id = this.clienteId();
    if (id != null) { this.router.navigate(['/cliente', id]); return; }
    this.router.navigate(['/pedido']);
  }
}
