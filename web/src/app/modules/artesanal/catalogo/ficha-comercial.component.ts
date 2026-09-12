import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DepositoService } from '../../generated/services/deposito.service';
import { Deposito } from '../../generated/models/deposito.model';
import { MembreteImpresionComponent } from '../../../shared/components/membrete-impresion/membrete-impresion.component';
import { FotoGaleriaComponent } from '../../../shared/components/foto-galeria/foto-galeria.component';
import { CatalogoService } from './catalogo.service';
import { FichaProducto, FichaProductoVariante } from './catalogo.model';

/** Fila de la matriz: una talla y sus celdas, una por color. */
interface FilaTalla {
  tallaId: number | null;
  tallaDisplay: string;
  celdas: (FichaProductoVariante | null)[];
}

/** Columna de la matriz: un color. */
interface ColumnaColor {
  colorId: number | null;
  colorDisplay: string;
  colorHex: string | null;
}

/**
 * Ficha comercial del producto — pantalla artesanal de SOLO LECTURA (plan §4.5, Etapa C).
 * Es la hoja que se le muestra al cliente: foto grande, ficha técnica, y la matriz
 * talla × color con existencias, precio USD y margen. Se imprime con el membrete del
 * sitio (mismo mecanismo que las fichas generadas: window.print() + @media print).
 *
 * "Agregar al pedido" abre el armado de pedido con ese SKU ya en el carrito
 * (`/pedidos/nuevo?varianteId=N`): no escribe nada acá, el alta sigue siendo del command.
 */
@Component({
  selector: 'app-ficha-comercial',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatIconModule, MatButtonModule, MatTooltipModule,
    MatFormFieldModule, MatSelectModule, MatProgressSpinnerModule,
    MembreteImpresionComponent, FotoGaleriaComponent,
  ],
  templateUrl: './ficha-comercial.component.html',
  styleUrl: './ficha-comercial.component.scss',
})
export class FichaComercialComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly service = inject(CatalogoService);
  private readonly depositoService = inject(DepositoService);

  readonly isLoading = signal(true);
  readonly ficha = signal<FichaProducto | null>(null);
  readonly foto = signal<string | null>(null);
  readonly depositos = signal<Deposito[]>([]);
  readonly depositoId = signal<number | null>(null);
  productoId = 0;

  /** Colores presentes, en el orden en que los devolvió el backend (talla, color, sku). */
  readonly columnas = computed<ColumnaColor[]>(() => {
    const vistos = new Map<string, ColumnaColor>();
    for (const v of this.ficha()?.variantes ?? []) {
      const clave = String(v.colorId ?? 'sin');
      if (!vistos.has(clave)) {
        vistos.set(clave, { colorId: v.colorId, colorDisplay: v.colorDisplay || 'Único', colorHex: v.colorHex });
      }
    }
    return [...vistos.values()];
  });

  /** Una fila por talla; las celdas vacías son combinaciones que no existen como SKU. */
  readonly filas = computed<FilaTalla[]>(() => {
    const cols = this.columnas();
    const porTalla = new Map<string, FilaTalla>();
    for (const v of this.ficha()?.variantes ?? []) {
      const clave = String(v.tallaId ?? 'sin');
      if (!porTalla.has(clave)) {
        porTalla.set(clave, {
          tallaId: v.tallaId,
          tallaDisplay: v.tallaDisplay || 'Única',
          celdas: cols.map(() => null),
        });
      }
      const fila = porTalla.get(clave)!;
      const i = cols.findIndex(c => c.colorId === v.colorId);
      if (i >= 0) { fila.celdas[i] = v; }
    }
    return [...porTalla.values()];
  });

  readonly skusConStock = computed(() => (this.ficha()?.variantes ?? []).filter(v => v.disponible > 0).length);
  readonly homologacionVencida = computed(() => this.ficha()?.homologacionVigente === false);

  ngOnInit(): void {
    this.depositoService.getAll().subscribe({
      next: (d: Deposito[]) => this.depositos.set(d),
      error: (e: unknown) => console.error('[FichaComercial] depósitos:', e),
    });
    this.route.paramMap.subscribe(pm => {
      this.productoId = Number(pm.get('id') ?? 0);
      this.cargar();
    });
  }

  cargar(): void {
    if (!this.productoId) { return; }
    this.isLoading.set(true);
    this.service.ficha(this.productoId, this.depositoId()).subscribe({
      next: (f: FichaProducto) => {
        this.ficha.set(f);
        this.isLoading.set(false);
        this.foto.set(null);
        if (f?.imagenPrincipalId != null) {
          this.service.portada(f.imagenPrincipalId).subscribe(src => this.foto.set(src));
        }
      },
      error: (e: unknown) => { console.error('[FichaComercial] ficha:', e); this.ficha.set(null); this.isLoading.set(false); },
    });
  }

  onDepositoChange(id: number | null): void {
    this.depositoId.set(id);
    this.cargar();
  }

  volver(): void { this.router.navigate(['/catalogo']); }

  /** La ficha CRUD del producto, para editar ficha técnica, destacado o portada. */
  editarProducto(): void { this.router.navigate(['/producto', this.productoId]); }

  imprimir(): void { window.print(); }

  agregarAlPedido(v: FichaProductoVariante): void {
    this.router.navigate(['/pedidos/nuevo'], { queryParams: { varianteId: v.varianteId } });
  }

  verVariante(v: FichaProductoVariante): void { this.router.navigate(['/variante', v.varianteId]); }
}
